using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BugTracker.Models;
using Microsoft.AspNet.Identity;

namespace BugTracker.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Tickets
        public ActionResult Index()
        {
            var tickets = db.Tickets.Include(t => t.AssignedUser).Include(t => t.CreatedByUser).Include(t => t.Project);
            return View(tickets.ToList());
        }

        // GET: Tickets/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            return View(ticket);
        }

        // GET: Tickets/Create
        [Route("Projects/{projectId}/Tickets/Create")]
        public ActionResult Create(int projectId)
        {
            ViewBag.ProjectId = projectId;
            ViewBag.AssignedUserId = new SelectList(db.Users, "Id", "Email");
            ViewBag.CreatedByUserId = new SelectList(db.Users, "Id", "Email");
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Projects/{projectId:int}/Tickets/Create")]
        public ActionResult Create([Bind(Include = "Id,Title,Description,Created,Updated,ProjectId,CreatedByUserId,AssignedUserId,TypeId,PriorityId,StatusId")] Ticket ticket, int projectId)
        {
            if (ModelState.IsValid)
            {
                ticket.ProjectId = projectId;
                db.Tickets.Add(ticket);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AssignedUserId = new SelectList(db.Users, "Id", "Email", ticket.AssignedUserId);
            ViewBag.CreatedByUserId = new SelectList(db.Users, "Id", "Email", ticket.CreatedByUserId);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ProjectId);
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            ViewBag.AssignedUserId = new SelectList(db.Users, "Id", "Email", ticket.AssignedUserId);
            ViewBag.CreatedByUserId = new SelectList(db.Users, "Id", "Email", ticket.CreatedByUserId);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ProjectId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,Description,Created,Updated,ProjectId,CreatedByUserId,AssignedUserId,TypeId,PriorityId,StatusId")] Ticket ticket)
        {
            var editable = new List<string>() { "Title", "Description" };
            if(User.IsInRole("Admin"))
                editable.Add("ProjectId");
            if (User.IsInRole("Admin") || User.IsInRole("Project Manager"))
                editable.AddRange(new string[] {"AssignedUserId", "TypeId", "PriorityId", "StatusId"});

            if (ModelState.IsValid)
            {
                var oldTicket = db.Tickets.AsNoTracking()
                    .FirstOrDefault(t => t.Id == ticket.Id);
                var histories = GetTicketHistories(oldTicket, ticket)
                    .Where(h => editable.Contains(h.History.Property));

                var mailer = new EmailService();

                foreach (var item in histories)
                {
                    db.TicketHistories.Add(item.History);
                    if (item.Notification != null)
                        mailer.SendAsync(item.Notification);
                }

                db.Update(ticket, editable.ToArray());
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            ViewBag.AssignedUserId = new SelectList(db.Users, "Id", "Email", ticket.AssignedUserId);
            ViewBag.CreatedByUserId = new SelectList(db.Users, "Id", "Email", ticket.CreatedByUserId);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ProjectId);
            return View(ticket);
        }

        private List<TicketHistoryWithNotification> GetTicketHistories(Ticket oldTicket, Ticket newTicket)
        {
            var histories = new List<TicketHistoryWithNotification>();
            if (oldTicket.AssignedUserId != newTicket.AssignedUserId)
            {
                var newUser = db.Users.Find(newTicket.AssignedUserId);

                histories.Add(new TicketHistoryWithNotification()
                {
                    History = new TicketHistory()
                    { 
                        Property = "AssignedUserId",
                        PropertyDisplay = "Assigned User",
                        OldValue = oldTicket.AssignedUserId,
                        OldValueDisplay = db.Users.Find(oldTicket.AssignedUserId)?.UserName,
                        NewValue = newTicket.AssignedUserId,
                        NewValueDisplay = newUser?.UserName
                    },
                    Notification = newUser != null ? new IdentityMessage()
                    {
                        Subject = "You have a new Notification",
                        Destination = newUser.Email,
                        Body = "You have been assigned to a new ticket with Id " + newTicket.Id + "!"
                    } : null
                });
            }
            if(oldTicket.Description != newTicket.Description)
               histories.Add(new TicketHistoryWithNotification()
               {
                   History = new TicketHistory()
                   { 
                       Property = "Description",
                       PropertyDisplay = "Description",
                       OldValue = oldTicket.Description,
                       OldValueDisplay = null,
                       NewValue = newTicket.Description,
                       NewValueDisplay = null
                   },
                   Notification = null
               });
            if(oldTicket.PriorityId != newTicket.PriorityId)
                histories.Add(new TicketHistoryWithNotification()
                {
                    History = new TicketHistory()
                    {
                        Property = "PriorityId",
                        PropertyDisplay = "Priority",
                        OldValue = oldTicket.PriorityId.ToString(),
                        OldValueDisplay = oldTicket.TicketPriority?.Name,
                        NewValue = newTicket.PriorityId.ToString(),
                        NewValueDisplay = db.TicketPriorities.Find(newTicket.PriorityId)?.Name
                    },
                    Notification = null
                });

            return histories;
        }

        private class TicketHistoryWithNotification
        {
            public TicketHistory History { get; set; }
            public IdentityMessage Notification { get; set; }
        }

        // GET: Tickets/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Ticket ticket = db.Tickets.Find(id);
            db.Tickets.Remove(ticket);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
