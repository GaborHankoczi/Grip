using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Grip.DAL.Model;

[Index(nameof(StationNumber), IsUnique = true)]
[Table("Station")]
public class Station
{
    public int Id { get; set; }

    public int StationNumber { get; set; }

    /// <summary>
    /// The name of the station. Eg. Room 103
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The secret key used to generate tokens for this station
    /// </summary>
    public string SecretKey { get; set; } = null!;

    public ICollection<Attendance> Attendances { get; set; } = null!;

    /// <summary>
    /// Classes held at this station
    /// </summary>
    public ICollection<Class> Classes { get; set; } = null!;
}