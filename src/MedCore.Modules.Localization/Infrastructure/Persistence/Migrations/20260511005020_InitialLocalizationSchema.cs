using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedCore.Modules.Localization.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialLocalizationSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Localization");

            migrationBuilder.CreateTable(
                name: "Translations",
                schema: "Localization",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Culture = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Key = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModifiedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Translations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Translations_Culture",
                schema: "Localization",
                table: "Translations",
                column: "Culture");

            migrationBuilder.CreateIndex(
                name: "IX_Translations_Culture_Key",
                schema: "Localization",
                table: "Translations",
                columns: new[] { "Culture", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Translations_IsActive",
                schema: "Localization",
                table: "Translations",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Translations",
                schema: "Localization");
        }
    }
}
