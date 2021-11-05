using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ReservationRestaurant.Migrations
{
    public partial class DropGuestFromSittingAndDataSeedingforSitting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Guest",
                table: "Sittings");

            migrationBuilder.InsertData(
                table: "Sittings",
                columns: new[] { "Id", "Capacity", "EndTime", "IsClosed", "Name", "RestaurantId", "SittingTypeId", "StartTime" },
                values: new object[] { 1, 30, new DateTime(2021, 1, 1, 12, 0, 0, 0, DateTimeKind.Unspecified), false, "Middle Eastern Breakfast", 1, 1, new DateTime(2021, 1, 1, 9, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                table: "Sittings",
                columns: new[] { "Id", "Capacity", "EndTime", "IsClosed", "Name", "RestaurantId", "SittingTypeId", "StartTime" },
                values: new object[] { 2, 30, new DateTime(2021, 1, 1, 16, 0, 0, 0, DateTimeKind.Unspecified), false, "Middle Eastern Lunch", 1, 2, new DateTime(2021, 1, 1, 13, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                table: "Sittings",
                columns: new[] { "Id", "Capacity", "EndTime", "IsClosed", "Name", "RestaurantId", "SittingTypeId", "StartTime" },
                values: new object[] { 3, 30, new DateTime(2021, 1, 1, 21, 0, 0, 0, DateTimeKind.Unspecified), false, "Middle Eastern Dinner", 1, 3, new DateTime(2021, 1, 1, 17, 0, 0, 0, DateTimeKind.Unspecified) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Sittings",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Sittings",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Sittings",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.AddColumn<int>(
                name: "Guest",
                table: "Sittings",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
