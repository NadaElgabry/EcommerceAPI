using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedDataV3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Guid",
                value: new Guid("f47ac10b-58cc-4372-a567-0e02b2c3d479"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Guid",
                value: new Guid("658737c3-35ee-4f28-bd40-decde11dc19d"));
        }
    }
}
