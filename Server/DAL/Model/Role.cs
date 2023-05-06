using Microsoft.AspNetCore.Identity;

namespace Grip.DAL.Model;

public class Role : IdentityRole<int>
{
    public const string Admin = "Admin";
    public const string Teacher = "Teacher";
    public const string Student = "Student";
    
}
