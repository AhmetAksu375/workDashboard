using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace workDashboard.Migrations
{
    /// <inheritdoc />
    public partial class CompanyRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DepartmantId",
                table: "Companies",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepartmantId",
                table: "Companies");
        }
    }
}
