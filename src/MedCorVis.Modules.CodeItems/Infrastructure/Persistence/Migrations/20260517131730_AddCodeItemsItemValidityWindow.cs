using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedCorVis.Modules.CodeItems.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCodeItemsItemValidityWindow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "ValidFrom",
                schema: "CodeItems",
                table: "Items",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ValidTo",
                schema: "CodeItems",
                table: "Items",
                type: "date",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_Validity",
                schema: "CodeItems",
                table: "Items",
                columns: new[] { "IsActive", "IsDeleted", "ValidFrom", "ValidTo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Items_Validity",
                schema: "CodeItems",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ValidFrom",
                schema: "CodeItems",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ValidTo",
                schema: "CodeItems",
                table: "Items");
        }
    }
}
