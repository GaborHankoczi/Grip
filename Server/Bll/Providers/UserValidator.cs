using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Grip.Bll.Providers;

public class UserValidator<T> : IUserValidator<T> where T : IdentityUser
{
    public async Task<IdentityResult> ValidateAsync(UserManager<T> manager, T user)
    {
        var userWithSameEmail = await manager.FindByEmailAsync(user.Email ?? throw new ArgumentNullException(nameof(user.Email)));
        if (userWithSameEmail != null)
            return IdentityResult.Failed(new IdentityError
            {
                Code = "EmailAlreadyExists",
                Description = "Email is already taken"
            });
        return IdentityResult.Success;
    }
}