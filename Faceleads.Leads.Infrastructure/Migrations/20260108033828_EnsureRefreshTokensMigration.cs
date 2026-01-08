using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Faceleads.Leads.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnsureRefreshTokensMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeadsConsultores_Consultores_ConsultorId",
                table: "LeadsConsultores");

            migrationBuilder.AlterColumn<Guid>(
                name: "ConsultorId",
                table: "LeadsConsultores",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Username = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ExpiresUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_LeadsConsultores_Consultores_ConsultorId",
                table: "LeadsConsultores",
                column: "ConsultorId",
                principalTable: "Consultores",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeadsConsultores_Consultores_ConsultorId",
                table: "LeadsConsultores");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.AlterColumn<Guid>(
                name: "ConsultorId",
                table: "LeadsConsultores",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LeadsConsultores_Consultores_ConsultorId",
                table: "LeadsConsultores",
                column: "ConsultorId",
                principalTable: "Consultores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
