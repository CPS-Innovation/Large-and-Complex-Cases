using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CPS.ComplexCases.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueStorageConnectionIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ux_case_metadata_egress_workspace_id",
                schema: "large_complex_cases",
                table: "case_metadata",
                column: "egress_workspace_id",
                unique: true,
                filter: "egress_workspace_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_case_metadata_netapp_folder_path",
                schema: "large_complex_cases",
                table: "case_metadata",
                column: "netapp_folder_path",
                unique: true,
                filter: "netapp_folder_path IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_case_metadata_egress_workspace_id",
                schema: "large_complex_cases",
                table: "case_metadata");

            migrationBuilder.DropIndex(
                name: "ux_case_metadata_netapp_folder_path",
                schema: "large_complex_cases",
                table: "case_metadata");
        }
    }
}
