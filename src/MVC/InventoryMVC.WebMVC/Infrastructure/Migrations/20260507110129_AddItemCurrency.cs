using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryMVC.WebMVC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddItemCurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "InventoryItems",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "UAH");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Currency",
                table: "InventoryItems");
        }
    }
}
