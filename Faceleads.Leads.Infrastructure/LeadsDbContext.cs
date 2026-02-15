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

    // Identity-like sets
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permissao> Permissoes => Set<Permissao>();
    public DbSet<UsuarioRole> UsuariosRoles => Set<UsuarioRole>();
    public DbSet<RolePermissao> RolesPermissoes => Set<RolePermissao>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<LeadConsultor> LeadsConsultores => Set<LeadConsultor>();
    
    public DbSet<Tenant> Tenants => Set<Tenant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Auditing properties are now CLR properties on the entities (CreatedOn, CreatedBy, etc.)
        // No need to add shadow properties.

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

            // Use CLR auditing property
            builder.Property(l => l.CreatedOn).IsRequired();
            builder.Property(l => l.CreatedBy);
            builder.Property(l => l.ModifiedOn);
            builder.Property(l => l.ModifiedBy);

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

            // Configure CLR audit properties for Consultor
            // CreatedOn was made nullable by a later migration
            builder.Property(c => c.CreatedOn);
            builder.Property(c => c.CreatedBy);
            builder.Property(c => c.ModifiedOn);
            builder.Property(c => c.ModifiedBy);

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

        // Identity/authorization mappings
        modelBuilder.Entity<Faceleads.Leads.Domain.Usuario>(builder =>
        {
            builder.ToTable("Usuarios");
            builder.HasKey(u => u.Id);
            builder.Property(u => u.NomeUsuario).IsRequired().HasMaxLength(200);
            builder.Property(u => u.NormalizedNomeUsuario).IsRequired().HasMaxLength(200);
            builder.Property(u => u.Email).IsRequired().HasMaxLength(200);
            builder.Property(u => u.NormalizedEmail).IsRequired().HasMaxLength(200);
            builder.Property(u => u.SenhaHash).IsRequired();
            builder.Property(u => u.TenantId).IsRequired();
            builder.Property(u => u.ConsultorId);
            builder.HasIndex("TenantId", "NormalizedEmail").IsUnique();
            builder.HasIndex("TenantId", "NormalizedNomeUsuario").IsUnique();
        });

        modelBuilder.Entity<Faceleads.Leads.Domain.Role>(builder =>
        {
            builder.ToTable("Roles");
            builder.HasKey(r => r.Id);
            builder.Property(r => r.Nome).IsRequired().HasMaxLength(200);
            builder.Property(r => r.NormalizedNome).IsRequired().HasMaxLength(200);
            builder.Property(r => r.TenantId);
            builder.HasIndex("TenantId", "NormalizedNome").IsUnique();
        });

        modelBuilder.Entity<Faceleads.Leads.Domain.Permissao>(builder =>
        {
            builder.ToTable("Permissoes");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Nome).IsRequired().HasMaxLength(200);
            builder.Property(p => p.NormalizedNome).IsRequired().HasMaxLength(200);
            builder.HasIndex(p => p.Nome).IsUnique();
        });

        modelBuilder.Entity<Faceleads.Leads.Domain.UsuarioRole>(builder =>
        {
            builder.ToTable("UsuariosRoles");
            builder.HasKey(ur => new { ur.UsuarioId, ur.RoleId });
            builder.HasOne(ur => ur.Usuario).WithMany(u => u.Roles!).HasForeignKey(ur => ur.UsuarioId);
            builder.HasOne(ur => ur.Role).WithMany(r => r.Usuarios!).HasForeignKey(ur => ur.RoleId);
        });

        modelBuilder.Entity<Faceleads.Leads.Domain.RolePermissao>(builder =>
        {
            builder.ToTable("RolesPermissoes");
            builder.HasKey(rp => new { rp.RoleId, rp.PermissaoId });
            builder.HasOne(rp => rp.Role).WithMany(r => r.Permissoes!).HasForeignKey(rp => rp.RoleId);
            builder.HasOne(rp => rp.Permissao).WithMany(p => p.Roles!).HasForeignKey(rp => rp.PermissaoId);
        });

        modelBuilder.Entity<LeadConsultor>(builder =>
        {
            builder.ToTable("LeadsConsultores");

            builder.HasKey(lc => lc.Id);

            builder.Property(lc => lc.AtribuidoEmUtc)
                .IsRequired();

            builder.Property(lc => lc.EncerradoEmUtc);
            builder.Property(lc => lc.CreatedOn);
            builder.Property(lc => lc.CreatedBy);
            builder.Property(lc => lc.ModifiedOn);
            builder.Property(lc => lc.ModifiedBy);
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
            builder.Property(rt => rt.CreatedOn);
            builder.Property(rt => rt.CreatedBy);
            builder.Property(rt => rt.ModifiedOn);
            builder.Property(rt => rt.ModifiedBy);
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

                // Use CLR CreatedOn for tenants
                builder.Property(t => t.CreatedOn).IsRequired();
                builder.Property(t => t.CreatedBy);
                builder.Property(t => t.ModifiedOn);
                builder.Property(t => t.ModifiedBy);
            });
    }
}
