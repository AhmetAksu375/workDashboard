using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace workDashboard.Migrations
{
    /// <inheritdoc />
    public partial class DeleteCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Companies_CompanyId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Departmants_DepartmantId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Departmants_DepartmantId1",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Works_Employees_EmployeeId",
                table: "Works");

            migrationBuilder.DropIndex(
                name: "IX_Employees_DepartmantId1",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "DepartmantId1",
                table: "Employees");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Companies_CompanyId",
                table: "Employees",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Departmants_DepartmantId",
                table: "Employees",
                column: "DepartmantId",
                principalTable: "Departmants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Works_Employees_EmployeeId",
                table: "Works",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Companies_CompanyId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Departmants_DepartmantId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Works_Employees_EmployeeId",
                table: "Works");

            migrationBuilder.AddColumn<int>(
                name: "DepartmantId1",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DepartmantId1",
                table: "Employees",
                column: "DepartmantId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Companies_CompanyId",
                table: "Employees",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Departmants_DepartmantId",
                table: "Employees",
                column: "DepartmantId",
                principalTable: "Departmants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Departmants_DepartmantId1",
                table: "Employees",
                column: "DepartmantId1",
                principalTable: "Departmants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Works_Employees_EmployeeId",
                table: "Works",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
