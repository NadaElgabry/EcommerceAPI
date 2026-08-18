using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EcommerceAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedDataV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "ImageUrl", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "https://www.flaticon.com/free-icon/electronics_1555401", "Electronics" },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "https://www.flaticon.com/free-icon/electronics_1555401", "Groceries" }
                });

            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "New Arrival" },
                    { 2, "Bestseller" },
                    { 3, "Sale" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "BirthDate", "CategoryId", "CreatedAt", "Email", "FirstName", "Guid", "HashedPassword", "IsActive", "LastName", "PhoneNumber", "Role", "UpdatedAt" },
                values: new object[] { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@example.com", "admin", new Guid("658737c3-35ee-4f28-bd40-decde11dc19d"), "$2y$10$7rLSvRVyTQORapkDOqmkhetjF6H9lJHngr4hJMSM2lHObJbW5EQh6", "True", "user", "01200032134", "Admin", null });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "AltText", "Brand", "CategoryId", "CreationDate", "Description", "Name", "Price", "ProductImage", "Slug", "StockQuantity" },
                values: new object[,]
                {
                    { 1, null, "AudioTech", 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "High quality noise-canceling headphones.", "Wireless Headphones", 199.99m, null, "wireless-headphones", 50 },
                    { 2, null, "Cadbury", 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Has chocolate in it.", "Moro Dark Chocolate", 5.12m, null, "moro-dark-chocolate", 100 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
