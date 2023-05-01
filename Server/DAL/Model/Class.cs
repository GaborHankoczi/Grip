using System.ComponentModel.DataAnnotations.Schema;

namespace Grip.DAL.Model;


[Table("Class")]
public class Class
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public DateTime StartDateTime { get; set; }
    public User Teacher { get; set; } = null!;
    public Group Group { get; set; } = null!;
}