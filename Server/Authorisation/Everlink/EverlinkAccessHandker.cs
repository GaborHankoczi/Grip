using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Server.Authorisation.Everlink
{
    public class IsEverlinkMachineClient : AuthorizationHandler<EverlinkAccessRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, EverlinkAccessRequirement requirement)
        {
            if(context.User.HasClaim(c => c.Type == "client_EverlinkAccess" && c.Value == "true") || context.User.IsInRole("Admin"))
                context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}