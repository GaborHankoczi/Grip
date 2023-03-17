using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grip.DAL.Model;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Grip.Providers;

public class EmailConfirmationTokenProvieder : DataProtectorTokenProvider<User>
{
    public EmailConfirmationTokenProvieder(IDataProtectionProvider dataProtectionProvider, IOptions<DataProtectionTokenProviderOptions> options, ILogger<DataProtectorTokenProvider<User>> logger) : base(dataProtectionProvider, options, logger)
    {
    }

    public override Task<string> GenerateAsync(string purpose, UserManager<User> manager, User user)
    {
        if(purpose == "ConfirmEmail"){
            return Task.FromResult("AAA");
        }
        return base.GenerateAsync(purpose, manager, user);
    }

    public override Task<bool> ValidateAsync(string purpose, string token, UserManager<User> manager, User user)
    {
        if(purpose == "ConfirmEmail"){
            return Task.FromResult(token == "AAA");
        }
        return base.ValidateAsync(purpose, token, manager, user);
    }
    

    public override Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<User> manager, User user)
    {
        return Task.FromResult(false);
    }
}
