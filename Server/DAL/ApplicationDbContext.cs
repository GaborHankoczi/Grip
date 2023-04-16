using Grip.DAL.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Grip.DAL;

public class ApplicationDbContext : IdentityDbContext<User,Role,int>
{
    public DbSet<PassiveTag> PassiveTags { get; set; } = null!;
    public DbSet<Attendance> Attendances { get; set; } = null!;
    public DbSet<Group> Group { get; set; } = null!;
    public DbSet<Class> Class { get; set; } = null!;
    public DbSet<Station> Station { get; set; } = null!;
    
    IConfiguration _configuration;
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration)
        : base(options)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseNpgsql(_configuration.GetConnectionString("DatabaseConnection"));
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<PassiveTag>().HasOne(p => p.User).WithMany(u => u.PassiveTags);
        builder.Entity<Attendance>().HasOne(a => a.User).WithMany(u => u.Attendances);
        builder.Entity<Attendance>().HasOne(a => a.Station).WithMany(s => s.Attendances);
        builder.Entity<Group>().HasMany(g => g.User).WithMany(u => u.Groups);
        builder.Entity<Group>().HasMany(g => g.Class).WithOne(c => c.Group);
    }
}
