using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Web;
using Microsoft.AspNet.Identity;
using Octokit;

namespace BugTracker.Models
{
    public class TeamManager
    {
        private ApplicationUserManager Manager { get; }

        //private ApplicationDbContext Context { get; }

        //public TeamManager(ApplicationUserManager userManager, ApplicationDbContext context)
        //{
        //    Manager = userManager;
        //    Context = context;
        //}

        //public bool InviteUserToTeam(int teamId, string invitedById, string email)
        //{
        //    using (var crypto = new RNGCryptoServiceProvider())
        //    {
        //        var tokenData = new byte[8];
        //        crypto.GetBytes(tokenData);
        //        var key = Convert.ToBase64String(tokenData);

        //        Context.TeamInvitations.Add(new TeamInvite()
        //        {
        //            Key = key,
        //            Email = email,
        //            InvitedById = invitedById,
        //            TeamId = teamId
        //        });
        //        Context.SaveChanges();

        //        //Publish notification to signalR
        //        //Send Email to user
        //        return true;
        //    }
        //    return false;
        //}

        //public bool AcceptTeamInvite(int teamId, string email, string key)
        //{
        //    var invite = Context.TeamInvitations.FirstOrDefault(
        //        t => t.TeamId == teamId && t.Key == key && t.Email == email);
        //    var user = Manager.FindByEmail(email);
        //    var team = Context.Teams.Find(teamId);

        //    if (invite == null || user == null || team == null)
        //        return false;

        //    team.Members.Add(user);

        //    Manager.AddClaim(user.Id, new Claim("Team", teamId.ToString()));

        //    return true;
        //}

        //public bool AddTeamRoleToUser(string userId, string role, int teamId)
        //{
        //    try
        //    {
        //        var dbRole = Context.Roles.FirstOrDefault(r => r.Name == role);
        //        if (dbRole == null)
        //            return false;

        //        Context.UserTeamRoles.Add(new UserTeamRole()
        //        {
        //            UserId = userId,
        //            TeamId = teamId,
        //            RoleId = dbRole.Id
        //        });

        //        Context.SaveChanges();
        //    }
        //    catch (DbException e)
        //    {
        //        var ex = e.GetBaseException() as SqlException;
        //        //It's not SQL exception, or it's not an update key or unique constraint error
        //        if (ex == null || (ex.Number != 2601 && ex.Number != 123))
        //        {
        //            throw;
        //        }
        //        return false;
        //    }
        //    //Add User Claim to team role
        //    return true;
        //}

        //public bool RemoveTeamRoleFromUser(string userId, string role, int teamId)
        //{
        //    return true;
        //}

    }
}