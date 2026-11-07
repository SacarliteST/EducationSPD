using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EducationSPD.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentSkillStates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "applied_model_version",
                table: "LearningEpisodes",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.Sql("ALTER TABLE \"LearningEpisodes\" ALTER COLUMN applied_model_version DROP DEFAULT;");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_ModelVersions_version",
                table: "ModelVersions",
                column: "version");

            migrationBuilder.CreateTable(
                name: "StudentSkillStates",
                columns: table => new
                {
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    module_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    skill_id = table.Column<Guid>(type: "uuid", nullable: false),
                    model_version = table.Column<int>(type: "integer", nullable: false),
                    mastery_probability = table.Column<decimal>(type: "numeric(18,16)", precision: 18, scale: 16, nullable: false),
                    observations_count = table.Column<int>(type: "integer", nullable: false),
                    last_observed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentSkillStates", x => new { x.student_id, x.module_key, x.skill_id, x.model_version });
                    table.CheckConstraint("CK_StudentSkillStates_MasteryProbability", "mastery_probability BETWEEN 0 AND 1");
                    table.CheckConstraint("CK_StudentSkillStates_ObservationsCount", "observations_count > 0");
                    table.ForeignKey(
                        name: "FK_StudentSkillStates_ModelVersions_model_version",
                        column: x => x.model_version,
                        principalTable: "ModelVersions",
                        principalColumn: "version",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentSkillStates_Skills_skill_id",
                        column: x => x.skill_id,
                        principalTable: "Skills",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LearningEpisodes_applied_model_version",
                table: "LearningEpisodes",
                column: "applied_model_version");

            migrationBuilder.CreateIndex(
                name: "IX_StudentSkillStates_model_version",
                table: "StudentSkillStates",
                column: "model_version");

            migrationBuilder.CreateIndex(
                name: "IX_StudentSkillStates_skill_id",
                table: "StudentSkillStates",
                column: "skill_id");

            migrationBuilder.AddForeignKey(
                name: "FK_LearningEpisodes_ModelVersions_applied_model_version",
                table: "LearningEpisodes",
                column: "applied_model_version",
                principalTable: "ModelVersions",
                principalColumn: "version",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LearningEpisodes_ModelVersions_applied_model_version",
                table: "LearningEpisodes");

            migrationBuilder.DropTable(
                name: "StudentSkillStates");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_ModelVersions_version",
                table: "ModelVersions");

            migrationBuilder.DropIndex(
                name: "IX_LearningEpisodes_applied_model_version",
                table: "LearningEpisodes");

            migrationBuilder.DropColumn(
                name: "applied_model_version",
                table: "LearningEpisodes");
        }
    }
}
