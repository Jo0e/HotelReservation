using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructures.Migrations
{
    /// <inheritdoc />
    public partial class editCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfirmPassword",
                table: "Companies");

            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "Companies",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "Passwords",
                table: "Companies",
                newName: "Addres");

            migrationBuilder.AddColumn<string>(
                name: "ProfileImage",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfileImage",
                table: "Companies");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "Companies",
                newName: "UserName");

            migrationBuilder.RenameColumn(
                name: "Addres",
                table: "Companies",
                newName: "Passwords");

            migrationBuilder.AddColumn<string>(
                name: "ConfirmPassword",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
