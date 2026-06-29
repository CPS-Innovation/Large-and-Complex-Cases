using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CPS.ComplexCases.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEgressWorkspaceNameToCaseMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "egress_workspace_name",
                schema: "large_complex_cases",
                table: "case_metadata",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "egress_workspace_name",
                schema: "large_complex_cases",
                table: "case_metadata");
        }
    }
}
