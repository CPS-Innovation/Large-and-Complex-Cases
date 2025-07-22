using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CPS.ComplexCases.Data.Migrations
{
    /// <inheritdoc />
    public partial class ActivityLogRemoveUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "user_id",
                schema: "large_complex_cases",
                table: "activity_log");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "user_id",
                schema: "large_complex_cases",
                table: "activity_log",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
