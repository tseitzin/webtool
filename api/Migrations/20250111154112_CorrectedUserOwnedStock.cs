using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class CorrectedUserOwnedStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AverageurchasePrice",
                table: "user_owned_stocks",
                newName: "AveragePurchasePrice");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AveragePurchasePrice",
                table: "user_owned_stocks",
                newName: "AverageurchasePrice");
        }
    }
}
