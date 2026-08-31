using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OrderIndexing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Guid",
                table: "Order",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "OrderNumber",
                table: "Order",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Order",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Guid", "OrderNumber" },
                values: new object[] { new Guid("a0a0a0a0-a0a0-a0a0-a0a0-a0a0a0a0a0a0"), "100001" });

            migrationBuilder.UpdateData(
                table: "Order",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Guid", "OrderNumber" },
                values: new object[] { new Guid("b0b0b0b0-b0b0-b0b0-b0b0-b0b0b0b0b0b0"), "100002" });

            migrationBuilder.CreateIndex(
                name: "IX_Order_Guid",
                table: "Order",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Order_OrderNumber",
                table: "Order",
                column: "OrderNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Order_Guid",
                table: "Order");

            migrationBuilder.DropIndex(
                name: "IX_Order_OrderNumber",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "Guid",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "OrderNumber",
                table: "Order");
        }
    }
}
