using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grip.DAL.Model
{
    public class Attendance
    {
        public int Id { get; set; }
        public User User { get; set; } = null!;

        public Station Station { get; set; } = null!;

        public DateTime Time { get; set; }

        // Maybe add a bool for if the user is entering or leaving the station

    }
}