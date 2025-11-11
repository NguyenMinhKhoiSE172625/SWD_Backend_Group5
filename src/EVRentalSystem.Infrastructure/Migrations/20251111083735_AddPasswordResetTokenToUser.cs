using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVRentalSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordResetTokenToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "OdometerReading",
                table: "VehicleInspections",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RenterSignature",
                table: "VehicleInspections",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StaffSignature",
                table: "VehicleInspections",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordResetToken",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetTokenExpiry",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rentals_PickupStaffId",
                table: "Rentals",
                column: "PickupStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Rentals_ReturnStaffId",
                table: "Rentals",
                column: "ReturnStaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rentals_Users_PickupStaffId",
                table: "Rentals",
                column: "PickupStaffId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Rentals_Users_ReturnStaffId",
                table: "Rentals",
                column: "ReturnStaffId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleInspections_Users_InspectorId",
                table: "VehicleInspections",
                column: "InspectorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rentals_Users_PickupStaffId",
                table: "Rentals");

            migrationBuilder.DropForeignKey(
                name: "FK_Rentals_Users_ReturnStaffId",
                table: "Rentals");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleInspections_Users_InspectorId",
                table: "VehicleInspections");

            migrationBuilder.DropIndex(
                name: "IX_Rentals_PickupStaffId",
                table: "Rentals");

            migrationBuilder.DropIndex(
                name: "IX_Rentals_ReturnStaffId",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "OdometerReading",
                table: "VehicleInspections");

            migrationBuilder.DropColumn(
                name: "RenterSignature",
                table: "VehicleInspections");

            migrationBuilder.DropColumn(
                name: "StaffSignature",
                table: "VehicleInspections");

            migrationBuilder.DropColumn(
                name: "PasswordResetToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordResetTokenExpiry",
                table: "Users");
        }
    }
}
