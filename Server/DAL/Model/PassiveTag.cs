using System;

namespace Grip.DAL.Model
{
    /// <summary>
    /// Represents a passive tag in the database.
    /// </summary>
    public class PassiveTag
    {
        /// <summary>
        /// Gets or sets the ID of the passive tag.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the serial number of the passive tag.
        /// </summary>
        public Int64 SerialNumber { get; set; }

        /// <summary>
        /// Gets or sets the user associated with the passive tag.
        /// </summary>
        public User User { get; set; } = null!;
    }
}
