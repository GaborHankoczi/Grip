using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grip.DAL.Model
{
    public class Group
    {
        public int Id { get; set; }
        public ICollection<User> User { get; set; } = null!;
        public string Name { get; set; } = null!;
        public ICollection<Class> Class { get; set; } = null!;
    }
}