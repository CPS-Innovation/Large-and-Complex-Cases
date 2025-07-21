using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CPS.ComplexCases.Data.Migrations
{
    /// <inheritdoc />
    public partial class ActivityLogIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "idx_activity_log_action_type",
                schema: "large_complex_cases",
                table: "activity_log",
                column: "action_type");

            migrationBuilder.CreateIndex(
                name: "idx_activity_log_resource_id",
                schema: "large_complex_cases",
                table: "activity_log",
                column: "resource_id");

            migrationBuilder.CreateIndex(
                name: "idx_activity_log_timestamp",
                schema: "large_complex_cases",
                table: "activity_log",
                column: "timestamp",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_activity_log_user_name",
                schema: "large_complex_cases",
                table: "activity_log",
                column: "user_name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_activity_log_action_type",
                schema: "large_complex_cases",
                table: "activity_log");

            migrationBuilder.DropIndex(
                name: "idx_activity_log_resource_id",
                schema: "large_complex_cases",
                table: "activity_log");

            migrationBuilder.DropIndex(
                name: "idx_activity_log_timestamp",
                schema: "large_complex_cases",
                table: "activity_log");

            migrationBuilder.DropIndex(
                name: "idx_activity_log_user_name",
                schema: "large_complex_cases",
                table: "activity_log");
        }
    }
}
