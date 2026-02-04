using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Faceleads.Leads.Infrastructure.Migrations
{
    public partial class RemoveCriadoEmUtcFromConsultoresV2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop CriadoEmUtc column if present
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'CriadoEmUtc' AND Object_ID = Object_ID(N'[dbo].[Consultores]'))
BEGIN
    ALTER TABLE [Consultores] DROP COLUMN [CriadoEmUtc]
END");

            // Backfill CreatedOn for existing rows
            migrationBuilder.Sql("UPDATE [Consultores] SET [CreatedOn] = SYSUTCDATETIME() WHERE [CreatedOn] IS NULL");

            // Alter CreatedOn to be NOT NULL
            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOn",
                table: "Consultores",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Recreate CriadoEmUtc with default current UTC
            migrationBuilder.AddColumn<DateTime>(
                name: "CriadoEmUtc",
                table: "Consultores",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            // Make CreatedOn nullable again
            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOn",
                table: "Consultores",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }
    }
}
