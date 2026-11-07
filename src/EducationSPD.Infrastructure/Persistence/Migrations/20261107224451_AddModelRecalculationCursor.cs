using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EducationSPD.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddModelRecalculationCursor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "recalculation_cursor_module_key",
                table: "ModelVersions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "recalculation_cursor_student_id",
                table: "ModelVersions",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "recalculation_cursor_module_key",
                table: "ModelVersions");

            migrationBuilder.DropColumn(
                name: "recalculation_cursor_student_id",
                table: "ModelVersions");
        }
    }
}
