using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVRentalSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_StationId_Status",
                table: "Vehicles",
                columns: new[] { "StationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_Status",
                table: "Vehicles",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleInspections_InspectionDate",
                table: "VehicleInspections",
                column: "InspectionDate");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleInspections_InspectorId",
                table: "VehicleInspections",
                column: "InspectorId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleInspections_IsPickup",
                table: "VehicleInspections",
                column: "IsPickup");

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsVerified",
                table: "Users",
                column: "IsVerified");

            migrationBuilder.CreateIndex(
                name: "IX_Users_PhoneNumber",
                table: "Users",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Role",
                table: "Users",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_Stations_IsActive",
                table: "Stations",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Stations_Latitude_Longitude",
                table: "Stations",
                columns: new[] { "Latitude", "Longitude" });

            migrationBuilder.CreateIndex(
                name: "IX_Rentals_PickupTime",
                table: "Rentals",
                column: "PickupTime");

            migrationBuilder.CreateIndex(
                name: "IX_Rentals_Status",
                table: "Rentals",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Rentals_UserId_Status",
                table: "Rentals",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentDate",
                table: "Payments",
                column: "PaymentDate");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Status",
                table: "Payments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_UserId_Status",
                table: "Payments",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BookingDate",
                table: "Bookings",
                column: "BookingDate");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_StationId_Status",
                table: "Bookings",
                columns: new[] { "StationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_Status",
                table: "Bookings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_UserId_Status",
                table: "Bookings",
                columns: new[] { "UserId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vehicles_StationId_Status",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_Status",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_VehicleInspections_InspectionDate",
                table: "VehicleInspections");

            migrationBuilder.DropIndex(
                name: "IX_VehicleInspections_InspectorId",
                table: "VehicleInspections");

            migrationBuilder.DropIndex(
                name: "IX_VehicleInspections_IsPickup",
                table: "VehicleInspections");

            migrationBuilder.DropIndex(
                name: "IX_Users_IsVerified",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_PhoneNumber",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Role",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Stations_IsActive",
                table: "Stations");

            migrationBuilder.DropIndex(
                name: "IX_Stations_Latitude_Longitude",
                table: "Stations");

            migrationBuilder.DropIndex(
                name: "IX_Rentals_PickupTime",
                table: "Rentals");

            migrationBuilder.DropIndex(
                name: "IX_Rentals_Status",
                table: "Rentals");

            migrationBuilder.DropIndex(
                name: "IX_Rentals_UserId_Status",
                table: "Rentals");

            migrationBuilder.DropIndex(
                name: "IX_Payments_PaymentDate",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_Status",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_UserId_Status",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_BookingDate",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_StationId_Status",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_Status",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_UserId_Status",
                table: "Bookings");
        }
    }
}
