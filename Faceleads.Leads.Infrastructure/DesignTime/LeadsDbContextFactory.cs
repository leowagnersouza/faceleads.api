using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
// Intentionally not using Microsoft.Extensions.Configuration to keep design-time factory simple

namespace Faceleads.Leads.Infrastructure
{
    // Design-time factory used by EF Core tools to create a DbContext when running migrations
    public sealed class LeadsDbContextFactory : IDesignTimeDbContextFactory<LeadsDbContext>
    {
        public LeadsDbContext CreateDbContext(string[] args)
        {
            // First, prefer an explicit environment variable for CI/Dev scenarios
            var connectionString = Environment.GetEnvironmentVariable("LEADS_CONNECTIONSTRING");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                // No explicit connection string provided via LEADS_CONNECTIONSTRING.
                // For design-time, fallback to a local default. Prefer setting LEADS_CONNECTIONSTRING
                // in the environment for CI or dev machines instead of relying on this fallback.
            }
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                // Fallback to a sensible local default (update password if necessary)
                connectionString = "Server=localhost,1433;Database=Faceleads;User Id=sa;Password=Gi@ele0804;TrustServerCertificate=True;";
            }

            var optionsBuilder = new DbContextOptionsBuilder<LeadsDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new LeadsDbContext(optionsBuilder.Options);
        }
    }
}
