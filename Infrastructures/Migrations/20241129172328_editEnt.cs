using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructures.Migrations
{
    /// <inheritdoc />
    public partial class editEnt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MealPrice",
                table: "Reservations");

            migrationBuilder.AddColumn<int>(
                name: "MealPrice",
                table: "RoomTypes",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MealPrice",
                table: "RoomTypes");

            migrationBuilder.AddColumn<int>(
                name: "MealPrice",
                table: "Reservations",
                type: "int",
                nullable: true);
        }
    }
}
