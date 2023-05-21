using System;

namespace Grip.DAL.Model
{
    /// <summary>
    /// Represents the attendance information of a user at a station.
    /// </summary>
    public class Attendance
    {
        /// <summary>
        /// Gets or sets the ID of the attendance record.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the user associated with the attendance.
        /// </summary>
        public User User { get; set; } = null!;

        /// <summary>
        /// Gets or sets the station associated with the attendance.
        /// </summary>
        public Station Station { get; set; } = null!;

        /// <summary>
        /// Gets or sets the timestamp of the attendance.
        /// </summary>
        public DateTime Time { get; set; }

        // TODO: Add a bool property to indicate if the user is entering or leaving the station.
        // For example:
        // public bool IsEntering { get; set; }
    }
}
