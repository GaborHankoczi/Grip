using Microsoft.AspNetCore.Identity;

namespace Grip.DAL.Model;

/// <summary>
/// Represents a role in the database.
/// </summary>
public class Role : IdentityRole<int>
{
    /// <summary>
    /// Constant string representing the admin role.
    /// </summary>
    public const string Admin = "Admin";
    /// <summary>
    /// Constant string representing the teacher role.
    /// </summary>
    public const string Teacher = "Teacher";
    /// <summary>
    /// Constant string representing the student role.
    /// </summary>
    public const string Student = "Student";
    /// <summary>
    /// Constant string representing the doorman role.
    /// </summary>
    public const string Doorman = "Doorman";

}
