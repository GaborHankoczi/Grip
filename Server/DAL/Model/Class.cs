using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grip.DAL.Model
{
    public class Class
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime StartDateTime { get; set; }
        public User Teacher { get; set; } = null!;
        public Group Group { get; set; } = null!;
    }
}