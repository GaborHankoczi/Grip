using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grip.Bll.DTO
{

    /// <summary>
    /// Represents the data transfer object for a student detail.
    /// </summary>
    public record StudentDetailDTO()
    {
        /// <summary>
        /// Id of the user
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Email address of the user
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// Username of the user
        /// </summary>
        public string UserName { get; set; } = null!;

        /// <summary>
        /// Absences of the user
        /// </summary>
        public IEnumerable<AbsenceDTO> Absences { get; set; } = null!;
    };
}