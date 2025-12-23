using Faceleads.Leads.Domain;
using Microsoft.EntityFrameworkCore;

namespace Faceleads.Leads.Infrastructure;

public sealed class LeadsDbContext : DbContext
{
    public LeadsDbContext(DbContextOptions<LeadsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Lead> Leads => Set<Lead>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Lead>(builder =>
        {
            builder.ToTable("Leads");

            builder.HasKey(l => l.Id);

            builder.Property(l => l.FullName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(l => l.Email)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(l => l.Phone)
                .HasMaxLength(30);

            builder.Property(l => l.Source)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(l => l.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(l => l.CreatedAtUtc)
                .IsRequired();

            builder.Property(l => l.AssignedAtUtc);
        });
    }
}
