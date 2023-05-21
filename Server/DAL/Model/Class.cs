using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grip.DAL.Model
{
    /// <summary>
    /// Represents a class in the database.
    /// </summary>
    [Table("Class")]
    public record Class
    {
        /// <summary>
        /// Gets or sets the ID of the class.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the class.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Gets or sets the start date and time of the class.
        /// </summary>
        public DateTime StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the teacher associated with the class.
        /// </summary>
        public User Teacher { get; set; } = null!;

        /// <summary>
        /// Gets or sets the group associated with the class.
        /// </summary>
        public Group Group { get; set; } = null!;

        /// <summary>
        /// Gets or sets the station associated with the class.
        /// </summary>
        public Station Station { get; set; } = null!;
    }
}
