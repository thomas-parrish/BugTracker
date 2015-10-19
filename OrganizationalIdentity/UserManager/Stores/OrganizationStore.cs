using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrganizationalIdentity.Interfaces;

namespace OrganizationalIdentity.UserManager
{
    public class OrganizationStore<TUser> :
        IOrganizationStore<TUser>
        where TUser : OrganizationUser
    {
        //RUH ROH
        protected OrganizationDbContext<TUser> Context { get; private set; }
        public bool AutoSaveChanges { get; set; } = true;
        public IQueryable<Organization> Organizations => Context.Organizations;
        private bool _disposed = false; // To detect redundant calls
        public bool DisposeContext { get; set; } // Do we also want to dispose the context?

        //RUH ROH
        public OrganizationStore(OrganizationDbContext<TUser> context)
        {
            if(context == null)
                throw new ArgumentNullException(nameof(context));
            Context = context;
        }  

        public async Task CreateAsync(Organization organization)
        {
            ThrowIfDisposed();
            if (organization == null)
            {
                throw new ArgumentNullException(nameof(organization));
            }
            Context.Organizations.Add(organization);
            await SaveChangesAsync();
        }

        public async Task UpdateAsync(Organization organization)
        {
            ThrowIfDisposed();
            if (organization == null)
            {
                throw new ArgumentNullException(nameof(organization));
            }

            Context.Entry(organization).State = EntityState.Modified;
            await SaveChangesAsync();
        }

        public async Task DeleteAsync(Organization organization)
        {
            ThrowIfDisposed();
            if (organization == null)
            {
                throw new ArgumentNullException(nameof(organization));
            }
            Context.Entry(organization).State = EntityState.Deleted;
            await SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            ThrowIfDisposed();
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException($"The {nameof(id)} parameter is required.",nameof(id));
            }

            var organization = new Organization() {Id = id};
            if (Context.Set<Organization>().Local.All(o => o.Id != id))
                Context.Set<Organization>().Attach(organization);
            Context.Entry(organization).State = EntityState.Deleted;
            await SaveChangesAsync();
        }

        public async Task AddUser(Organization organization, TUser user)
        {
            ThrowIfDisposed();
            if (organization == null)
                throw new ArgumentNullException(nameof(organization));
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            organization.Users.Add(user);
            await SaveChangesAsync();
        }

        public async Task RemoveUser(Organization organization, TUser user)
        {
            ThrowIfDisposed();
            if (organization == null)
                throw new ArgumentNullException(nameof(organization));
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            organization.Users.Remove(user);
            await SaveChangesAsync();
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        private async Task SaveChangesAsync()
        {
            if (AutoSaveChanges)
            {
                await Context.SaveChangesAsync().WithCurrentCulture();
            }
        }

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            if (DisposeContext && disposing)
            {
                Context?.Dispose();
            }
            _disposed = true;
            Context = null;
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
