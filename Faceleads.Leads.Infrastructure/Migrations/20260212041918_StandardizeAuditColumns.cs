using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Faceleads.Leads.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StandardizeAuditColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "UsuariosRoles",
                newName: "ModifiedOn");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "UsuariosRoles",
                newName: "CreatedOn");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "Usuarios",
                newName: "ModifiedOn");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Usuarios",
                newName: "CreatedOn");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "RolesPermissoes",
                newName: "ModifiedOn");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "RolesPermissoes",
                newName: "CreatedOn");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Roles",
                newName: "ModifiedOn");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Roles",
                newName: "CreatedOn");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Permissoes",
                newName: "ModifiedOn");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Permissoes",
                newName: "CreatedOn");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ModifiedOn",
                table: "UsuariosRoles",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "UsuariosRoles",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "ModifiedOn",
                table: "Usuarios",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "Usuarios",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "ModifiedOn",
                table: "RolesPermissoes",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "RolesPermissoes",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "ModifiedOn",
                table: "Roles",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "Roles",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "ModifiedOn",
                table: "Permissoes",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "Permissoes",
                newName: "CreatedAt");
        }
    }
}
