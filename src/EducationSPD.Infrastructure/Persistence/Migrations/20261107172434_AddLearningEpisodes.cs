using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EducationSPD.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLearningEpisodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LearningEpisodes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    episode_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    module_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    task_id = table.Column<Guid>(type: "uuid", nullable: false),
                    finalized_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningEpisodes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "SkillObservations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    learning_episode_id = table.Column<Guid>(type: "uuid", nullable: false),
                    skill_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source = table.Column<short>(type: "smallint", nullable: false),
                    outcome = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillObservations", x => x.id);
                    table.CheckConstraint("CK_SkillObservations_Outcome", "outcome IN (1, 2)");
                    table.CheckConstraint("CK_SkillObservations_Source", "source IN (1, 2)");
                    table.ForeignKey(
                        name: "FK_SkillObservations_LearningEpisodes_learning_episode_id",
                        column: x => x.learning_episode_id,
                        principalTable: "LearningEpisodes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SkillObservations_Skills_skill_id",
                        column: x => x.skill_id,
                        principalTable: "Skills",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LearningEpisodes_StudentId_FinalizedAt",
                table: "LearningEpisodes",
                columns: new[] { "student_id", "finalized_at" });

            migrationBuilder.CreateIndex(
                name: "UX_LearningEpisodes_EventId",
                table: "LearningEpisodes",
                column: "event_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_LearningEpisodes_ModuleKey_EpisodeId",
                table: "LearningEpisodes",
                columns: new[] { "module_key", "episode_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SkillObservations_skill_id",
                table: "SkillObservations",
                column: "skill_id");

            migrationBuilder.CreateIndex(
                name: "UX_SkillObservations_EpisodeId_SkillId",
                table: "SkillObservations",
                columns: new[] { "learning_episode_id", "skill_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SkillObservations");

            migrationBuilder.DropTable(
                name: "LearningEpisodes");
        }
    }
}
