using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Grip.DAL.Model;

public class Identification
{
    [Key]
    public int Id { get; set; }
}