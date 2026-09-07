using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SnapshotOrderItemProductDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_Products_ProductId",
                table: "OrderItem");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "OrderItem",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "ProductAltText",
                table: "OrderItem",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductDescription",
                table: "OrderItem",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProductImageUrl",
                table: "OrderItem",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "OrderItem",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProductSlug",
                table: "OrderItem",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            // Backfill every existing row (not just the HasData-seeded ones below) from the
            // live Product table, before ProductId can ever become NULL for real orders.
            migrationBuilder.Sql(@"
                UPDATE oi
                SET oi.ProductName = p.Name,
                    oi.ProductSlug = p.Slug,
                    oi.ProductDescription = p.Description,
                    oi.ProductImageUrl = p.ProductImage,
                    oi.ProductAltText = p.AltText
                FROM OrderItem oi
                INNER JOIN Products p ON oi.ProductId = p.Id;
            ");

            migrationBuilder.UpdateData(
                table: "OrderItem",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ProductAltText", "ProductDescription", "ProductImageUrl", "ProductName", "ProductSlug" },
                values: new object[] { "Wireless Headphones", "High quality noise-canceling headphones.", "https://example.com/images/wireless-headphones.jpg", "Wireless Headphones", "wireless-headphones" });

            migrationBuilder.UpdateData(
                table: "OrderItem",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ProductAltText", "ProductDescription", "ProductImageUrl", "ProductName", "ProductSlug" },
                values: new object[] { "Moro Dark Chocolate", "Has chocolate in it.", "https://example.com/images/moro-dark-chocolate.jpg", "Moro Dark Chocolate", "moro-dark-chocolate" });

            migrationBuilder.UpdateData(
                table: "OrderItem",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ProductAltText", "ProductDescription", "ProductImageUrl", "ProductName", "ProductSlug" },
                values: new object[] { "Moro Dark Chocolate", "Has chocolate in it.", "https://example.com/images/moro-dark-chocolate.jpg", "Moro Dark Chocolate", "moro-dark-chocolate" });

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_Products_ProductId",
                table: "OrderItem",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_Products_ProductId",
                table: "OrderItem");

            migrationBuilder.DropColumn(
                name: "ProductAltText",
                table: "OrderItem");

            migrationBuilder.DropColumn(
                name: "ProductDescription",
                table: "OrderItem");

            migrationBuilder.DropColumn(
                name: "ProductImageUrl",
                table: "OrderItem");

            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "OrderItem");

            migrationBuilder.DropColumn(
                name: "ProductSlug",
                table: "OrderItem");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "OrderItem",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_Products_ProductId",
                table: "OrderItem",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
