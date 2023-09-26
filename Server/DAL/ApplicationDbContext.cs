using System.ComponentModel.DataAnnotations.Schema;
using Duende.IdentityServer.EntityFramework.Entities;
using Duende.IdentityServer.EntityFramework.Interfaces;
using Grip.DAL.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Grip.DAL;

/// <summary>
/// Represents the application's database context.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<User, Role, int>
{
    /// <summary>
    /// Gets or sets the DbSet for the PassiveTag entity.
    /// </summary>
    public DbSet<PassiveTag> PassiveTags { get; set; } = null!;

    /// <summary>
    /// Gets or sets the DbSet for the Attendance entity.
    /// </summary>
    public DbSet<Attendance> Attendances { get; set; } = null!;

    /// <summary>
    /// Gets or sets the DbSet for the Group entity.
    /// </summary>
    public DbSet<Group> Groups { get; set; } = null!;

    /// <summary>
    /// Gets or sets the DbSet for the Class entity.
    /// </summary>
    public DbSet<Class> Classes { get; set; } = null!;

    /// <summary>
    /// Gets or sets the DbSet for the Station entity.
    /// </summary>
    public DbSet<Station> Stations { get; set; } = null!;

    /// <summary>
    /// Gets or sets the DbSet for the Exempt entity.
    /// </summary>
    public DbSet<Exempt> Exempts { get; set; } = null!;

    IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the ApplicationDbContext class.
    /// </summary>
    /// <param name="options">The DbContext options.</param>
    /// <param name="configuration">The application configuration.</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration)
        : base(options)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Configures the DbContext options before they are used to connect to the database.
    /// </summary>
    /// <param name="optionsBuilder">The options builder.</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        if (optionsBuilder.IsConfigured) return;
        optionsBuilder.UseNpgsql(_configuration.GetConnectionString("DatabaseConnection"));
        //TODO remove
        optionsBuilder.EnableSensitiveDataLogging();
    }

    /// <summary>
    /// Configures the model and relationships between entities.
    /// </summary>
    /// <param name="builder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<PassiveTag>().HasOne(p => p.User).WithMany(u => u.PassiveTags);
        builder.Entity<Attendance>().HasOne(a => a.User).WithMany(u => u.Attendances);
        builder.Entity<Attendance>().HasOne(a => a.Station).WithMany(s => s.Attendances);
        builder.Entity<Group>().HasMany(g => g.Users).WithMany(u => u.Groups);
        builder.Entity<Group>().HasMany(g => g.Classes).WithOne(c => c.Group);
        builder.Entity<Exempt>().HasOne(e => e.IssuedBy).WithMany(u => u.IssuedExemptions);
        builder.Entity<Exempt>().HasOne(e => e.IssuedTo).WithMany(u => u.Exemptions);
        builder.Entity<Class>().HasOne(c => c.Station).WithMany(s => s.Classes);
    }
}
