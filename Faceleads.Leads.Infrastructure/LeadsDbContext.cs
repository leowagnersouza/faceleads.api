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

    public DbSet<Consultor> Consultores => Set<Consultor>();

    public DbSet<LeadConsultor> LeadsConsultores => Set<LeadConsultor>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Lead>(builder =>
        {
            builder.ToTable("Leads");

            builder.HasKey(l => l.Id);

            builder.Property(l => l.NomeCompleto)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(l => l.Email)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(l => l.Telefone)
                .HasMaxLength(30);

            builder.Property(l => l.Origem)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(l => l.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(l => l.CriadoEmUtc)
                .IsRequired();

            builder.Property(l => l.AtribuidoEmUtc);

            builder.HasMany(l => l.Consultores)
                .WithOne(lc => lc.Lead)
                .HasForeignKey(lc => lc.LeadId);
        });

        modelBuilder.Entity<Consultor>(builder =>
        {
            builder.ToTable("Consultores");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.NomeCompleto)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.Email)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.Telefone)
                .HasMaxLength(30);

            builder.Property(c => c.Ativo)
                .IsRequired();

            builder.Property(c => c.CriadoEmUtc)
                .IsRequired();

            builder.HasMany(c => c.Leads)
                .WithOne(lc => lc.Consultor)
                .HasForeignKey(lc => lc.ConsultorId);
        });

        modelBuilder.Entity<LeadConsultor>(builder =>
        {
            builder.ToTable("LeadsConsultores");

            builder.HasKey(lc => lc.Id);

            builder.Property(lc => lc.AtribuidoEmUtc)
                .IsRequired();

            builder.Property(lc => lc.EncerradoEmUtc);
        });
    }
}
