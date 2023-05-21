using Microsoft.AspNetCore.Identity;

namespace Grip.DAL.Model
{
    /// <summary>
    /// Represents a user in the database.
    /// </summary>
    public class User : IdentityUser<int>
    {
        /// <summary>
        /// Gets or sets the collection of passive tags associated with the user.
        /// </summary>
        public ICollection<PassiveTag> PassiveTags { get; set; } = null!;

        /// <summary>
        /// Gets or sets the collection of attendances associated with the user.
        /// </summary>
        public ICollection<Attendance> Attendances { get; set; } = null!;

        /// <summary>
        /// Gets or sets the collection of groups the user belongs to.
        /// </summary>
        public ICollection<Group> Groups { get; set; } = null!;

        /// <summary>
        /// Gets or sets the collection of exemptions associated with the user.
        /// </summary>
        public ICollection<Exempt> Exemptions { get; set; } = null!;

        /// <summary>
        /// Gets or sets the collection of exemptions issued by the user.
        /// </summary>
        public ICollection<Exempt> IssuedExemptions { get; set; } = null!;
    }
}