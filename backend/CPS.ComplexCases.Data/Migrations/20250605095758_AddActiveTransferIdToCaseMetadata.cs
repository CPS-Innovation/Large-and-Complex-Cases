using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CPS.ComplexCases.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddActiveTransferIdToCaseMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "active_transfer_id",
                schema: "large_complex_cases",
                table: "case_metadata",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "active_transfer_id",
                schema: "large_complex_cases",
                table: "case_metadata");
        }
    }
}
