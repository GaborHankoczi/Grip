using System;
using System.ComponentModel.DataAnnotations;

namespace Grip.Bll.DTO
{
    /// <summary>
    /// Represents the data transfer object for creating a class.
    /// </summary>
    public record CreateClassDTO
    {
        /// <summary>
        /// Gets or sets the name of the class.
        /// </summary>
        [Required]
        public string Name { get; init; } = null!;

        /// <summary>
        /// Gets or sets the start date and time of the class.
        /// </summary>
        [Required]
        public DateTime StartDateTime { get; init; }

        /// <summary>
        /// Gets or sets the ID of the group associated with the class.
        /// </summary>
        [Required]
        public int GroupId { get; init; }

        /// <summary>
        /// Gets or sets the ID of the teacher associated with the class.
        /// </summary>
        [Required]
        public int TeacherId { get; init; }

        /// <summary>
        /// Gets or sets the ID of the station associated with the class.
        /// </summary>
        [Required]
        public int StationId { get; init; }
    }
}
