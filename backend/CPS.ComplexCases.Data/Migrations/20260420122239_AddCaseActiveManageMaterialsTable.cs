using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CPS.ComplexCases.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCaseActiveManageMaterialsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "case_active_manage_materials",
                schema: "large_complex_cases",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    case_id = table.Column<int>(type: "integer", nullable: false),
                    operation_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    source_paths = table.Column<string>(type: "jsonb", nullable: false),
                    destination_paths = table.Column<string>(type: "jsonb", nullable: true),
                    user_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_case_active_manage_materials", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_case_active_manage_materials_case_id",
                schema: "large_complex_cases",
                table: "case_active_manage_materials",
                column: "case_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "case_active_manage_materials",
                schema: "large_complex_cases");
        }
    }
}
