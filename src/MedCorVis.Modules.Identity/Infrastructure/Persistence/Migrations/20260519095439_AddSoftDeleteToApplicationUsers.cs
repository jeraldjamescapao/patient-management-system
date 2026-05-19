using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedCorVis.Modules.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToApplicationUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAtUtc",
                schema: "Identity",
                table: "Users",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "Identity",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletionRequestedAtUtc",
                schema: "Identity",
                table: "Users",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "Identity",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Users_DeletionRequestedAtUtc",
                schema: "Identity",
                table: "Users",
                column: "DeletionRequestedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_DeletionRequestedAtUtc",
                schema: "Identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                schema: "Identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "Identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeletionRequestedAtUtc",
                schema: "Identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "Identity",
                table: "Users");
        }
    }
}
