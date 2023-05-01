using System.ComponentModel.DataAnnotations.Schema;

namespace Grip.DAL.Model;


[Table("Group")]
public class Group
{
    public int Id { get; set; }
    [Column("User")]
    public ICollection<User> Users { get; set; } = null!;
    public string Name { get; set; } = null!;
    [Column("Class")]
    public ICollection<Class> Classes { get; set; } = null!;
}