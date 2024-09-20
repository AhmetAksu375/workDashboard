using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace workDashboard.Migrations
{
    /// <inheritdoc />
    public partial class UpdateReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DepartmantId1",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DepartmantId1",
                table: "Admins",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DepartmantId1",
                table: "Employees",
                column: "DepartmantId1");

            migrationBuilder.CreateIndex(
                name: "IX_Admins_DepartmantId1",
                table: "Admins",
                column: "DepartmantId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Admins_Departmants_DepartmantId1",
                table: "Admins",
                column: "DepartmantId1",
                principalTable: "Departmants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Departmants_DepartmantId1",
                table: "Employees",
                column: "DepartmantId1",
                principalTable: "Departmants",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Admins_Departmants_DepartmantId1",
                table: "Admins");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Departmants_DepartmantId1",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_DepartmantId1",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Admins_DepartmantId1",
                table: "Admins");

            migrationBuilder.DropColumn(
                name: "DepartmantId1",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "DepartmantId1",
                table: "Admins");
        }
    }
}
