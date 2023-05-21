using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Grip.DAL.Model
{
    /// <summary>
    /// Represents a station in the database.
    /// </summary>
    [Index(nameof(StationNumber), IsUnique = true)]
    [Table("Station")]
    public class Station
    {
        /// <summary>
        /// Gets or sets the ID of the station.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the station number.
        /// </summary>
        public int StationNumber { get; set; }

        /// <summary>
        /// Gets or sets the name of the station. (E.g., Room 103)
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the secret key used to generate tokens for this station.
        /// </summary>
        public string SecretKey { get; set; } = null!;

        /// <summary>
        /// Gets or sets the collection of attendances associated with this station.
        /// </summary>
        public ICollection<Attendance> Attendances { get; set; } = null!;

        /// <summary>
        /// Gets or sets the collection of classes held at this station.
        /// </summary>
        public ICollection<Class> Classes { get; set; } = null!;
    }
}
