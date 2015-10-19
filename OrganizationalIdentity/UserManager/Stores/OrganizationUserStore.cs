using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace OrganizationalIdentity.UserManager
{
    public class OrganizationUserStore<TUser>  :
        IUserLoginStore<TUser, string>,
        IUserClaimStore<TUser, string>,
        IUserRoleStore<TUser, string>,
        IUserPasswordStore<TUser, string>,
        IUserSecurityStampStore<TUser, string>,
        IQueryableUserStore<TUser, string>,
        IUserEmailStore<TUser, string>,
        IUserPhoneNumberStore<TUser, string>,
        IUserTwoFactorStore<TUser, string>,
        IUserLockoutStore<TUser, string>
        where TUser : OrganizationUser, IUser<string>
    {
        protected OrganizationDbContext<TUser> Context { get; private set; }
        public bool AutoSaveChanges { get; set; } = true;
        public IQueryable<TUser> Users => Context.Users;
        /// <summary>
        ///     If true will call dispose on the DbContext during Dispose
        /// </summary>
        public bool DisposeContext { get; set; }
        private bool _disposed = false; // To detect redundant calls
        private IDbSet<IdentityUserClaim> Claims { get; set; } 

        public OrganizationUserStore(OrganizationDbContext<TUser> context)
        {
            Context = context;
            Claims = context.Set<IdentityUserClaim>();
        }

        public async Task CreateAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            Context.Users.Add(user);
            await SaveChanges().WithCurrentCulture();
        }

        public async Task DeleteAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            Context.Users.Remove(user);
            await SaveChanges().WithCurrentCulture();
        }

        public Task<TUser> FindByNameAsync(string userName)
        {
            ThrowIfDisposed();
            if(string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException(nameof(userName));
            return Context.Set<TUser>().Include(c => c.Claims)
                .Include(l => l.Logins)
                .Include(r => r.Roles)
                .FirstOrDefaultAsync(u => u.UserName == userName);
        }

        public async Task UpdateAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            Context.Entry(user).State = EntityState.Modified;
            await SaveChanges().WithCurrentCulture();
        }

        public virtual async Task<IList<Claim>> GetClaimsAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return user.Claims.Select(c => new Claim(c.ClaimType, c.ClaimValue)).ToList();
        }

        public Task AddClaimAsync(TUser user, Claim claim)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (claim == null)
            {
                throw new ArgumentNullException("claim");
            }
            Claims.Add(new IdentityUserClaim { UserId = user.Id, ClaimType = claim.Type, ClaimValue = claim.Value });
            return Task.FromResult(0);
        }

        public async Task RemoveClaimAsync(TUser user, Claim claim)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (claim == null)
            {
                throw new ArgumentNullException("claim");
            }
            IEnumerable<IdentityUserClaim> claims = user.Claims.Where(uc => uc.ClaimValue == claim.Value && uc.ClaimType == claim.Type).ToList();
            var claimValue = claim.Value;
            var claimType = claim.Type;

            foreach (var c in claims)
            {
                user.Claims.Remove(c);
            }
            await SaveChanges();
        }

        public async Task AddToRoleAsync(TUser user, string roleName)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (String.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentException("roleName cannot be empty", "roleName");
            }
            var roleEntity = await Context.Set<OrganizationRole>().SingleOrDefaultAsync(r => r.Name.ToUpper() ==  roleName.ToUpper()).WithCurrentCulture();
            if (roleEntity == null)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                    "Role not found", roleName));
            }
            var ur = new OrganizationUserRole() { UserId = user.Id, RoleId = roleEntity.Id };

            Context.Set<OrganizationUserRole>().Add(ur);
            await SaveChanges();
        }
        
        //New
        public async Task AddToRoleAsync(TUser user, string organizationId, string roleName)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (String.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentException("roleName cannot be empty", "roleName");
            }
            var roleEntity = await Context.Set<OrganizationRole>().SingleOrDefaultAsync(r => r.Name.ToUpper() == roleName.ToUpper()).WithCurrentCulture();
            if (roleEntity == null)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                    "Role not found", roleName));
            }
            var orgUserEntity = await Context.Set<Organization>().SingleOrDefaultAsync(o => o.Id.Equals(organizationId));
            if (orgUserEntity == null)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                    "Organization not found", roleName));
            }
            if (!orgUserEntity.Users.Any(u => u.Id.Equals(user.Id)))
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                    "The user is not a member of the organization", roleName));
            }

            var ur = new OrganizationUserRole() { UserId = user.Id, RoleId = roleEntity.Id, OrganizationId = organizationId };
            if (!orgUserEntity.Roles.Any(our => our.OrganizationId.Equals(organizationId) && our.RoleId.Equals(roleEntity.Id) && our.UserId.Equals(user.Id)))
            {
                Context.Set<OrganizationUserRole>().Add(ur);
                await SaveChanges();
            }
        }

        public async Task RemoveFromRoleAsync(TUser user, string roleName)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (String.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(roleName));
            }
            var roleEntity = await Context.Set<OrganizationRole>().SingleOrDefaultAsync(r => r.Name.ToUpper() == roleName.ToUpper()).WithCurrentCulture();
            if (roleEntity != null)
            {
                var roleId = roleEntity.Id;
                var userId = user.Id;
                var userRole = await Context.Set<OrganizationUserRole>().FirstOrDefaultAsync(r => roleId.Equals(r.RoleId) && r.UserId.Equals(userId)).WithCurrentCulture();
                if (userRole != null)
                {
                    Context.Set<OrganizationUserRole>().Remove(userRole);
                }
            }
        }

        //New
        public async Task RemoveFromRoleAsync(TUser user, string organizationId, string roleName)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (String.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(roleName));
            }
            if (String.IsNullOrWhiteSpace(organizationId))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(organizationId));
            }
            var roleEntity = await Context.Set<OrganizationRole>().SingleOrDefaultAsync(r => r.Name.ToUpper() == roleName.ToUpper()).WithCurrentCulture();
            if (roleEntity != null)
            {
                var roleId = roleEntity.Id;
                var userId = user.Id;
                var userRole = await Context.Set<OrganizationUserRole>().FirstOrDefaultAsync(r => roleId.Equals(r.RoleId) && r.UserId.Equals(userId) && r.OrganizationId.Equals(organizationId)).WithCurrentCulture();
                if (userRole != null)
                {
                    Context.Set<OrganizationUserRole>().Remove(userRole);
                }
            }
        }

        //New - Only gets global roles, does not return organization roles
        public async Task<IList<string>> GetRolesAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            var userId = user.Id;
            var query = from userRole in Context.Set<OrganizationUserRole>()
                        where userRole.UserId.Equals(userId) && 
                        (userRole.OrganizationId == null || userRole.OrganizationId == "")
                        join role in Context.Set<OrganizationRole>() on userRole.RoleId equals role.Id
                        select role.Name;
            return await query.ToListAsync().WithCurrentCulture();
        }

        //New - Gets only roles with organization
        public async Task<IList<OrganizationUserRole>> GetOrganizationRolesAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            var userId = user.Id;
            var query = from userRole in Context.Set<OrganizationUserRole>().Include(i=>i.Role)
                        where userRole.UserId.Equals(userId) &&
                        (userRole.OrganizationId != null && userRole.OrganizationId != "")
                        select userRole;
            return await query.ToListAsync().WithCurrentCulture();
        }

        //New
        public async Task<IList<string>> GetRolesAsync(TUser user, string organizationId)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (string.IsNullOrWhiteSpace(organizationId))
            {
                throw new ArgumentException(nameof(organizationId));
            }
            var userId = user.Id;
            var query = from userRole in Context.Set<OrganizationUserRole>()
                        where userRole.UserId.Equals(userId) && userRole.OrganizationId.Equals(organizationId)
                        join role in Context.Set<OrganizationRole>() on userRole.RoleId equals role.Id
                        select role.Name;
            return await query.ToListAsync().WithCurrentCulture();
        }

        //New
        public async Task<bool> IsInRoleAsync(TUser user, string organizationId, string roleName)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (String.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentException(nameof(roleName));
            }
            var role = await Context.Set<OrganizationRole>().SingleOrDefaultAsync(r => r.Name.ToUpper() == roleName.ToUpper()).WithCurrentCulture();
            if (role != null)
            {
                var userId = user.Id;
                var roleId = role.Id;
                return await Context.Set<OrganizationUserRole>().AnyAsync(ur => ur.RoleId.Equals(roleId) && ur.UserId.Equals(userId) && ur.OrganizationId.Equals(organizationId)).WithCurrentCulture();
            }
            return false;
        }

        public async Task<bool> IsInRoleAsync(TUser user, string roleName)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (String.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentException(nameof(roleName));
            }
            var role = await Context.Set<OrganizationRole>().SingleOrDefaultAsync(r => r.Name.ToUpper() == roleName.ToUpper()).WithCurrentCulture();
            if (role != null)
            {
                var userId = user.Id;
                var roleId = role.Id;
                return await Context.Set<OrganizationUserRole>().AnyAsync(ur => ur.RoleId.Equals(roleId) && ur.UserId.Equals(userId)).WithCurrentCulture();
            }
            return false;
        }

        public Task SetPasswordHashAsync(TUser user, string passwordHash)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        public Task<string> GetPasswordHashAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(TUser user)
        {
            return Task.FromResult(user.PasswordHash != null);
        }

        public Task SetSecurityStampAsync(TUser user, string stamp)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.SecurityStamp = stamp;
            return Task.FromResult(0);
        }

        public Task<string> GetSecurityStampAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult(user.SecurityStamp);
        }

        public Task AddLoginAsync(TUser user, UserLoginInfo login)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (login == null)
            {
                throw new ArgumentNullException("login");
            }
            Context.Set<IdentityUserLogin>().Add(new IdentityUserLogin()
            {
                UserId = user.Id,
                ProviderKey = login.ProviderKey,
                LoginProvider = login.LoginProvider
            });
            return Task.FromResult(0);
        }

        public async Task RemoveLoginAsync(TUser user, UserLoginInfo login)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (login == null)
            {
                throw new ArgumentNullException("login");
            }
            IdentityUserLogin entry;
            var provider = login.LoginProvider;
            var key = login.ProviderKey;
            if (AreLoginsLoaded(user))
            {
                entry = user.Logins.SingleOrDefault(ul => ul.LoginProvider == provider && ul.ProviderKey == key);
            }
            else
            {
                var userId = user.Id;
                entry = await Context.Set<IdentityUserLogin>().SingleOrDefaultAsync(ul => ul.LoginProvider == provider && ul.ProviderKey == key && ul.UserId.Equals(userId)).WithCurrentCulture();
            }
            if (entry != null)
            {
                Context.Set<IdentityUserLogin>().Remove(entry);
            }
        }

        public async Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            await EnsureLoginsLoaded(user).WithCurrentCulture();
            return user.Logins.Select(l => new UserLoginInfo(l.LoginProvider, l.ProviderKey)).ToList();
        }

        public async Task<TUser> FindAsync(UserLoginInfo login)
        {
            ThrowIfDisposed();
            if (login == null)
            {
                throw new ArgumentNullException("login");
            }
            var provider = login.LoginProvider;
            var key = login.ProviderKey;
            var userLogin =
                await Context.Set<IdentityUserLogin>().FirstOrDefaultAsync(l => l.LoginProvider == provider && l.ProviderKey == key).WithCurrentCulture();
            if (userLogin != null)
            {
                var userId = userLogin.UserId;
                return await FindByIdAsync(userId);
            }
            return null;
        }

        public Task<TUser> FindByIdAsync(string userId)
        {
            ThrowIfDisposed();
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException(nameof(userId));
            return Context.Set<TUser>().Include(c => c.Claims)
                .Include(l => l.Logins)
                .Include(r => r.Roles)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public Task SetEmailAsync(TUser user, string email)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.Email = email;
            return Task.FromResult(0);
        }

        public Task<string> GetEmailAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult(user.EmailConfirmed);
        }

        public Task SetEmailConfirmedAsync(TUser user, bool confirmed)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.EmailConfirmed = confirmed;
            return Task.FromResult(0);
        }

        public Task<TUser> FindByEmailAsync(string email)
        {
            ThrowIfDisposed();
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException(nameof(email));
            return Context.Set<TUser>().Include(c => c.Claims)
                .Include(l => l.Logins)
                .Include(r => r.Roles)
                .FirstOrDefaultAsync(u => u.UserName == email);
        }

        public Task SetPhoneNumberAsync(TUser user, string phoneNumber)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.PhoneNumber = phoneNumber;
            return Task.FromResult(0);
        }

        public Task<string> GetPhoneNumberAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult(user.PhoneNumber);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult(user.PhoneNumberConfirmed);
        }

        public Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.PhoneNumberConfirmed = confirmed;
            return Task.FromResult(0);
        }

        public Task SetTwoFactorEnabledAsync(TUser user, bool enabled)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.TwoFactorEnabled = enabled;
            return Task.FromResult(0);
        }

        public Task<bool> GetTwoFactorEnabledAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult(user.TwoFactorEnabled);
        }

        public Task<DateTimeOffset> GetLockoutEndDateAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return
                Task.FromResult(user.LockoutEndDateUtc.HasValue
                    ? new DateTimeOffset(DateTime.SpecifyKind(user.LockoutEndDateUtc.Value, DateTimeKind.Utc))
                    : new DateTimeOffset());
        }

        public Task SetLockoutEndDateAsync(TUser user, DateTimeOffset lockoutEnd)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.LockoutEndDateUtc = lockoutEnd == DateTimeOffset.MinValue ? (DateTime?)null : lockoutEnd.UtcDateTime;
            return Task.FromResult(0);
        }

        public Task<int> IncrementAccessFailedCountAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.AccessFailedCount++;
            return Task.FromResult(user.AccessFailedCount);
        }

        public Task ResetAccessFailedCountAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.AccessFailedCount = 0;
            return Task.FromResult(0);
        }

        public Task<int> GetAccessFailedCountAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult(user.AccessFailedCount);
        }

        public Task<bool> GetLockoutEnabledAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult(user.LockoutEnabled);
        }

        public Task SetLockoutEnabledAsync(TUser user, bool enabled)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.LockoutEnabled = enabled;
            return Task.FromResult(0);
        }

        private async Task SaveChanges()
        {
            if (AutoSaveChanges)
            {
                await Context.SaveChangesAsync().WithCurrentCulture();
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        private bool AreLoginsLoaded(TUser user)
        {
            return Context.Entry(user).Collection(u => u.Logins).IsLoaded;
        }

        private async Task EnsureLoginsLoaded(TUser user)
        {
            if (!AreLoginsLoaded(user))
            {
                var userId = user.Id;
                await Context.Set<IdentityUserLogin>().Where(uc => uc.UserId.Equals(userId)).LoadAsync().WithCurrentCulture();
                Context.Entry(user).Collection(u => u.Logins).IsLoaded = true;
            }
        }

        private bool AreClaimsLoaded(TUser user)
        {
            return Context.Entry(user).Collection(u => u.Claims).IsLoaded;
        }

        private async Task EnsureClaimsLoaded(TUser user)
        {
            if (!AreClaimsLoaded(user))
            {
                var userId = user.Id;
                await Context.Set<IdentityUserClaim>().Where(uc => uc.UserId.Equals(userId)).LoadAsync().WithCurrentCulture();
                Context.Entry(user).Collection(u => u.Claims).IsLoaded = true;
            }
        }

        private async Task EnsureRolesLoaded(TUser user)
        {
            if (!Context.Entry(user).Collection(u => u.Roles).IsLoaded)
            {
                var userId = user.Id;
                await Context.Set<OrganizationUserRole>().Where(uc => uc.UserId.Equals(userId)).LoadAsync().WithCurrentCulture();
                Context.Entry(user).Collection(u => u.Roles).IsLoaded = true;
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
