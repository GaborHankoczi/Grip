using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grip.DAL.Model;

public class PassiveTag
{
    public int Id { get; set; }

    public Int64 SerialNumber { get; set; }

    public User User { get; set; } = null!;
}