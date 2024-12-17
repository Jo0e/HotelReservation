using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructures.Migrations
{
    /// <inheritdoc />
    public partial class updateCompanyVm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "Companies",
                newName: "UserName");

            migrationBuilder.AddColumn<string>(
                name: "Passwords",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Passwords",
                table: "Companies");

            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "Companies",
                newName: "PasswordHash");
        }
    }
}
