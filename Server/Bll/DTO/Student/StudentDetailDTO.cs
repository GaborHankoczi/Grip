using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grip.Bll.DTO
{
    public record StudentDetailDTO()
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public IEnumerable<AbsenceDTO> Absences { get; set; }
    };
}