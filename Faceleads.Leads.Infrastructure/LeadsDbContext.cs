using Faceleads.Leads.Domain;
using Microsoft.EntityFrameworkCore;

namespace Faceleads.Leads.Infrastructure;

using Faceleads.Leads.Application.Common;
using System.Linq.Expressions;

public sealed class LeadsDbContext : DbContext
{
    private readonly ICurrentTenantService? _currentTenantService;

    public LeadsDbContext(DbContextOptions<LeadsDbContext> options)
        : base(options)
    {
    }

    // Used at runtime when DI can provide the current tenant
    public LeadsDbContext(DbContextOptions<LeadsDbContext> options, ICurrentTenantService currentTenantService)
        : base(options)
    {
        _currentTenantService = currentTenantService;
    }

    // Exposed property used by EF Core query filters. Returns Guid.Empty when no tenant available.
    public Guid CurrentTenantId => _currentTenantService?.TenantId ?? Guid.Empty;

    public DbSet<Lead> Leads => Set<Lead>();

    public DbSet<Consultor> Consultores => Set<Consultor>();

    public DbSet<Faceleads.Leads.Domain.RefreshToken> RefreshTokens => Set<Faceleads.Leads.Domain.RefreshToken>();

    public DbSet<LeadConsultor> LeadsConsultores => Set<LeadConsultor>();
    
    public DbSet<Tenant> Tenants => Set<Tenant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Add shadow properties for auditing across all entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Skip owned types
            if (entityType.IsOwned()) continue;

            // Add shadow properties if not already present
            if (entityType.FindProperty("CreatedBy") == null)
            {
                modelBuilder.Entity(entityType.ClrType).Property<string?>("CreatedBy");
            }
            if (entityType.FindProperty("CreatedOn") == null)
            {
                modelBuilder.Entity(entityType.ClrType).Property<DateTime?>("CreatedOn");
            }
            if (entityType.FindProperty("ModifiedBy") == null)
            {
                modelBuilder.Entity(entityType.ClrType).Property<string?>("ModifiedBy");
            }
            if (entityType.FindProperty("ModifiedOn") == null)
            {
                modelBuilder.Entity(entityType.ClrType).Property<DateTime?>("ModifiedOn");
            }
        }

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

            // CreatedOn is tracked as a shadow audit property; make it required
            builder.Property<DateTime>("CreatedOn").IsRequired();

            builder.Property(l => l.AtribuidoEmUtc);

            builder.HasMany(l => l.Consultores)
                .WithOne(lc => lc.Lead)
                .HasForeignKey(lc => lc.LeadId);
        });

        // Apply a query filter for TenantId when the property exists
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var tenantProperty = entityType.FindProperty("TenantId");
            if (tenantProperty == null) continue;

            var clrType = entityType.ClrType;
            var parameter = Expression.Parameter(clrType, "e");

            // Build EF.Property<Guid>(e, "TenantId")
            var propertyMethod = typeof(EF).GetMethod("Property", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)?.MakeGenericMethod(typeof(Guid));
            var propertyAccess = Expression.Call(propertyMethod!, parameter, Expression.Constant("TenantId"));

            // Build access to this.CurrentTenantId
            var currentTenantProperty = Expression.Property(Expression.Constant(this), nameof(CurrentTenantId));

            // Build equality expression: EF.Property<Guid>(e, "TenantId") == this.CurrentTenantId
            var body = Expression.Equal(propertyAccess, currentTenantProperty);

            var lambda = Expression.Lambda(body, parameter);

            modelBuilder.Entity(clrType).HasQueryFilter(lambda);
        }

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

            // Configure shadow audit property CreatedOn as required for Consultor
            builder.Property<DateTime>("CreatedOn").IsRequired();

            // Soft delete column
            builder.Property(c => c.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            // Global query filter to exclude soft deleted consultores by default
            // Tenant filter will be applied later when TenantProvider is implemented
            builder.HasQueryFilter(c => !c.IsDeleted);

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

        modelBuilder.Entity<RefreshToken>(builder =>
        {
            builder.ToTable("RefreshTokens");

            builder.HasKey(rt => rt.Id);

            builder.Property(rt => rt.Token)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(rt => rt.Username)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(rt => rt.ExpiresUtc)
                .IsRequired();

            builder.Property(rt => rt.CreatedUtc)
                .IsRequired();

            builder.Property(rt => rt.RevokedUtc);
        });

            modelBuilder.Entity<Tenant>(builder =>
            {
                builder.ToTable("Tenants");

                builder.HasKey(t => t.Id);

                builder.Property(t => t.Nome)
                    .IsRequired()
                    .HasMaxLength(200);

                builder.Property(t => t.Descricao)
                    .HasMaxLength(1000);

                builder.Property(t => t.Ativo)
                    .IsRequired()
                    .HasDefaultValue(true);

                // Use shadow CreatedOn audit property for tenants and make it required
                builder.Property<DateTime>("CreatedOn").IsRequired();
            });
    }
}
