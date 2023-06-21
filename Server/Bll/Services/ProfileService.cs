using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;

namespace Grip.Bll.Services
{
    public class ProfileService : IProfileService
    {
        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var requestedClaimTypes = context.RequestedClaimTypes;
            var user = context.Subject;
            // your implementation to retrieve the requested information
            context.IssuedClaims.AddRange(user.Claims);
            context.IssuedClaims.Add(new Claim("idp", "https://localhost:7258/"));

            return Task.CompletedTask;
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            context.IsActive = true;
            return Task.CompletedTask;
        }
    }
}