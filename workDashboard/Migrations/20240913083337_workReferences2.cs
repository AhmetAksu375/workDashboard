using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace workDashboard.Migrations
{
    /// <inheritdoc />
    public partial class workReferences2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "EmployeeId",
                table: "Works",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "DepartmantId",
                table: "Works",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "CompanyId",
                table: "Works",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Works_CompanyId",
                table: "Works",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Works_DepartmantId",
                table: "Works",
                column: "DepartmantId");

            migrationBuilder.CreateIndex(
                name: "IX_Works_EmployeeId",
                table: "Works",
                column: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Works_Companies_CompanyId",
                table: "Works",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Works_Departmants_DepartmantId",
                table: "Works",
                column: "DepartmantId",
                principalTable: "Departmants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Works_Employees_EmployeeId",
                table: "Works",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Works_Companies_CompanyId",
                table: "Works");

            migrationBuilder.DropForeignKey(
                name: "FK_Works_Departmants_DepartmantId",
                table: "Works");

            migrationBuilder.DropForeignKey(
                name: "FK_Works_Employees_EmployeeId",
                table: "Works");

            migrationBuilder.DropIndex(
                name: "IX_Works_CompanyId",
                table: "Works");

            migrationBuilder.DropIndex(
                name: "IX_Works_DepartmantId",
                table: "Works");

            migrationBuilder.DropIndex(
                name: "IX_Works_EmployeeId",
                table: "Works");

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeId",
                table: "Works",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "DepartmantId",
                table: "Works",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "CompanyId",
                table: "Works",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
