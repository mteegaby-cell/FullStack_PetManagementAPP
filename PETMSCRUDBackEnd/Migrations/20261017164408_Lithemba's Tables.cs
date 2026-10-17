using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PETMSCRUD.Migrations
{
    /// <inheritdoc />
    public partial class LithembasTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GroomingRecords_AppointmentId",
                table: "GroomingRecords");

            migrationBuilder.AlterColumn<int>(
                name: "AppointmentId",
                table: "GroomingRecords",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "ServicePerformed",
                table: "GroomingRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_GroomingRecords_AppointmentId",
                table: "GroomingRecords",
                column: "AppointmentId",
                unique: true,
                filter: "[AppointmentId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GroomingRecords_AppointmentId",
                table: "GroomingRecords");

            migrationBuilder.DropColumn(
                name: "ServicePerformed",
                table: "GroomingRecords");

            migrationBuilder.AlterColumn<int>(
                name: "AppointmentId",
                table: "GroomingRecords",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroomingRecords_AppointmentId",
                table: "GroomingRecords",
                column: "AppointmentId",
                unique: true);
        }
    }
}
