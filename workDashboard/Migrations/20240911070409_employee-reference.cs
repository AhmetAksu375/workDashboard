using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace workDashboard.Migrations
{
    /// <inheritdoc />
    public partial class employeereference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DepartmantId",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DepartmantId",
                table: "Employees",
                column: "DepartmantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Departmants_DepartmantId",
                table: "Employees",
                column: "DepartmantId",
                principalTable: "Departmants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Departmants_DepartmantId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_DepartmantId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "DepartmantId",
                table: "Employees");
        }
    }
}
