using System;

namespace Grip.DAL.Model
{
    /// <summary>
    /// Represents an exemption in the database.
    /// </summary>
    public class Exempt
    {
        /// <summary>
        /// Gets or sets the ID of the exemption.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the user who issued the exemption.
        /// </summary>
        public virtual User IssuedBy { get; set; } = null!;

        /// <summary>
        /// Gets or sets the ID of the user who issued the exemption.
        /// </summary>
        public int IssuedById { get; set; }

        /// <summary>
        /// Gets or sets the user to whom the exemption is issued.
        /// </summary>
        public virtual User IssuedTo { get; set; } = null!;

        /// <summary>
        /// Gets or sets the ID of the user to whom the exemption is issued.
        /// </summary>
        public int IssuedToId { get; set; }

        /// <summary>
        /// Gets or sets the date and time from which the exemption is valid.
        /// </summary>
        public DateTime ValidFrom { get; set; }

        /// <summary>
        /// Gets or sets the date and time until which the exemption is valid.
        /// </summary>
        public DateTime ValidTo { get; set; }
    }
}
