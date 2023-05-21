using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grip.DAL.Model
{
    /// <summary>
    /// Represents a group in the database.
    /// </summary>
    [Table("Group")]
    public class Group
    {
        /// <summary>
        /// Gets or sets the ID of the group.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the collection of users associated with the group.
        /// </summary>
        [Column("User")]
        public ICollection<User> Users { get; set; } = null!;

        /// <summary>
        /// Gets or sets the name of the group.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Gets or sets the collection of classes associated with the group.
        /// </summary>
        [Column("Class")]
        public ICollection<Class> Classes { get; set; } = null!;
    }
}
