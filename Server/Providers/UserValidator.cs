using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Grip.Providers;

public class UserValidator<T> : IUserValidator<T> where T : IdentityUser
{
    public async Task<IdentityResult> ValidateAsync(UserManager<T> manager, T user)
    {
        //TODO allow non unique usernames
        return await Task.FromResult(IdentityResult.Success);
        /*var userWithSameEmail = await manager.FindByEmailAsync(user.Email ?? throw new ArgumentNullException(nameof(user.Email)));
        if(userWithSameEmail != null)*/
    }
}