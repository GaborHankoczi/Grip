using Microsoft.AspNetCore.Identity;

namespace Grip.DAL.Model;

public class User : IdentityUser<int>
{
    public ICollection<PassiveTag> PassiveTags { get; set; } = null!;
    public ICollection<Attendance> Attendances { get; set; } = null!;
    public ICollection<Group> Groups { get; set; } = null!;
    public ICollection<Exempt> Exemptions { get; set; } = null!;
    public ICollection<Exempt> IssuedExemptions { get; set; } = null!;
}