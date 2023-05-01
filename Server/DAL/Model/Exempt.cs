namespace Grip.DAL.Model;

public class Exempt
{
    public int Id { get; set; }
    public virtual User IssuedBy { get; set; } = null!;
    public int IssuedById { get; set; }
    public virtual User IssuedTo { get; set; } = null!;
    public int IssuedToId { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
}