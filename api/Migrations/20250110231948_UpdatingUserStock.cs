using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class UpdatingUserStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AverageurchasePrice",
                table: "user_owned_stocks",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentValue",
                table: "user_owned_stocks",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCost",
                table: "user_owned_stocks",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AverageurchasePrice",
                table: "user_owned_stocks");

            migrationBuilder.DropColumn(
                name: "CurrentValue",
                table: "user_owned_stocks");

            migrationBuilder.DropColumn(
                name: "TotalCost",
                table: "user_owned_stocks");
        }
    }
}
