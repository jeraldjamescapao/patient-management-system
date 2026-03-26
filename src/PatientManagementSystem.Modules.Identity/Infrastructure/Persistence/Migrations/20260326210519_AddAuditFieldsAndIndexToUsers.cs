using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientManagementSystem.Modules.Identity.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditFieldsAndIndexToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "created_by",
                schema: "identity",
                table: "users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "System");

            migrationBuilder.AddColumn<string>(
                name: "modified_by",
                schema: "identity",
                table: "users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "System");

            migrationBuilder.CreateIndex(
                name: "ix_users_is_active",
                schema: "identity",
                table: "users",
                column: "is_active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_users_is_active",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "created_by",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "modified_by",
                schema: "identity",
                table: "users");
        }
    }
}
