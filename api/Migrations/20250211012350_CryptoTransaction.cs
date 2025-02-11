using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class CryptoTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StockSymbol",
                table: "transactions",
                newName: "Symbol");

            migrationBuilder.RenameIndex(
                name: "IX_transactions_StockSymbol",
                table: "transactions",
                newName: "IX_transactions_Symbol");

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "transactions",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "transactions",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "transactions");

            migrationBuilder.RenameColumn(
                name: "Symbol",
                table: "transactions",
                newName: "StockSymbol");

            migrationBuilder.RenameIndex(
                name: "IX_transactions_Symbol",
                table: "transactions",
                newName: "IX_transactions_StockSymbol");

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "transactions",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");
        }
    }
}
