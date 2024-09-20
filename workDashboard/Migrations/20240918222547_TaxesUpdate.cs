using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace workDashboard.Migrations
{
    /// <inheritdoc />
    public partial class TaxesUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TaxType",
                table: "Taxes",
                newName: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Taxes",
                newName: "TaxType");
        }
    }
}
