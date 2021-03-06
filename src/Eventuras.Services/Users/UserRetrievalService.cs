using Eventuras.Domain;
using Eventuras.Infrastructure;
using Eventuras.Services.Organizations;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eventuras.Services.Users
{
    internal class UserRetrievalService : IUserRetrievalService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentOrganizationAccessorService _currentOrganizationAccessorService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserRetrievalService(
            ApplicationDbContext context,
            ICurrentOrganizationAccessorService currentOrganizationAccessorService,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _currentOrganizationAccessorService = currentOrganizationAccessorService ?? throw new ArgumentNullException(nameof(currentOrganizationAccessorService));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException(nameof(userId));
            }

            return await _context.ApplicationUsers
                .AsNoTracking()
                .SingleAsync(u => u.Id == userId);
        }
        public async Task<List<ApplicationUser>> ListAccessibleUsers(UserRetrievalOptions options)
        {
            options ??= new UserRetrievalOptions();

            var user = _httpContextAccessor.HttpContext.User;
            if (!user.IsInRole(Roles.Admin) &&
                !user.IsInRole(Roles.SuperAdmin))
            {
                throw new AccessViolationException($"Should have {Roles.Admin} role to access other users.");
            }

            var query = _context.Users.AsNoTracking()
                .UseOptions(options);

            if (!user.IsInRole(Roles.SuperAdmin))
            {
                if (!user.IsInRole(Roles.Admin))
                {
                    // Not an admin of the current org => can't see org member list.
                    return new List<ApplicationUser>();
                }

                var organization = await _currentOrganizationAccessorService.RequireCurrentOrganizationAsync();
                if (!organization.IsRoot)
                {
                    query = query.HavingOrganization(organization);
                }
            }

            return await query.ToListAsync();
        }
    }
}
