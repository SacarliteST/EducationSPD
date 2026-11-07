using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EducationSPD.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddModelVersions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ModelVersions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    initial_mastery_probability = table.Column<decimal>(type: "numeric(10,9)", precision: 10, scale: 9, nullable: false),
                    learn_probability = table.Column<decimal>(type: "numeric(10,9)", precision: 10, scale: 9, nullable: false),
                    validation_rule_guess_probability = table.Column<decimal>(type: "numeric(10,9)", precision: 10, scale: 9, nullable: false),
                    validation_rule_slip_probability = table.Column<decimal>(type: "numeric(10,9)", precision: 10, scale: 9, nullable: false),
                    task_tag_guess_probability = table.Column<decimal>(type: "numeric(10,9)", precision: 10, scale: 9, nullable: false),
                    task_tag_slip_probability = table.Column<decimal>(type: "numeric(10,9)", precision: 10, scale: 9, nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    recalculation_started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    activated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    archived_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ahp_matrix = table.Column<decimal[]>(type: "numeric[]", nullable: false),
                    ahp_weights = table.Column<decimal[]>(type: "numeric[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelVersions", x => x.id);
                    table.CheckConstraint("CK_ModelVersions_AhpMatrixSize", "cardinality(ahp_matrix) = 16");
                    table.CheckConstraint("CK_ModelVersions_AhpWeightsSize", "cardinality(ahp_weights) = 4");
                    table.CheckConstraint("CK_ModelVersions_Probabilities", "initial_mastery_probability BETWEEN 0 AND 1 AND learn_probability BETWEEN 0 AND 1 AND validation_rule_guess_probability BETWEEN 0 AND 1 AND validation_rule_slip_probability BETWEEN 0 AND 1 AND task_tag_guess_probability BETWEEN 0 AND 1 AND task_tag_slip_probability BETWEEN 0 AND 1 AND validation_rule_guess_probability + validation_rule_slip_probability < 1 AND task_tag_guess_probability + task_tag_slip_probability < 1 AND validation_rule_guess_probability <= task_tag_guess_probability AND validation_rule_slip_probability <= task_tag_slip_probability");
                    table.CheckConstraint("CK_ModelVersions_Status", "status IN (1, 2, 3, 4)");
                    table.CheckConstraint("CK_ModelVersions_Version", "version > 0");
                });

            migrationBuilder.CreateIndex(
                name: "UX_ModelVersions_Active",
                table: "ModelVersions",
                column: "status",
                unique: true,
                filter: "status = 3");

            migrationBuilder.CreateIndex(
                name: "UX_ModelVersions_Version",
                table: "ModelVersions",
                column: "version",
                unique: true);

            migrationBuilder.InsertData(
                table: "ModelVersions",
                columns:
                [
                    "id",
                    "version",
                    "initial_mastery_probability",
                    "learn_probability",
                    "validation_rule_guess_probability",
                    "validation_rule_slip_probability",
                    "task_tag_guess_probability",
                    "task_tag_slip_probability",
                    "status",
                    "created_at",
                    "activated_at",
                    "ahp_matrix",
                    "ahp_weights",
                ],
                values:
                [
                    new Guid("ba951de1-7479-46f8-97dc-111534030001"),
                    1,
                    0.20m,
                    0.10m,
                    0.10m,
                    0.10m,
                    0.25m,
                    0.25m,
                    (short)3,
                    new DateTimeOffset(2026, 11, 7, 0, 0, 0, TimeSpan.Zero),
                    new DateTimeOffset(2026, 11, 7, 0, 0, 0, TimeSpan.Zero),
                    new decimal[]
                    {
                        1m, 3m, 5m, 7m,
                        1m / 3m, 1m, 3m, 4m,
                        1m / 5m, 1m / 3m, 1m, 2m,
                        1m / 7m, 1m / 4m, 1m / 2m, 1m,
                    },
                    new decimal[]
                    {
                        0.573144407258619m,
                        0.253210512343443m,
                        0.108193561807287m,
                        0.0654515185906505m,
                    },
                ]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModelVersions");
        }
    }
}
