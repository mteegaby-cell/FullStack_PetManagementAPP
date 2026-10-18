using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PETMSCRUD.Migrations
{
    /// <inheritdoc />
    public partial class OvayosTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PetOwners_UserId",
                table: "PetOwners");

            migrationBuilder.DropIndex(
                name: "IX_Groomers_UserId",
                table: "Groomers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "GroomingServices");

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "Pets",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "PetOwners",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "GroomingServices",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "AppointmentId",
                table: "GroomingRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GroomerId1",
                table: "GroomingRecords",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GroomingServiceId1",
                table: "GroomingRecords",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Groomers",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Appointments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_PetOwners_Email",
                table: "PetOwners",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PetOwners_UserId",
                table: "PetOwners",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_GroomingRecords_AppointmentId",
                table: "GroomingRecords",
                column: "AppointmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroomingRecords_GroomerId1",
                table: "GroomingRecords",
                column: "GroomerId1");

            migrationBuilder.CreateIndex(
                name: "IX_GroomingRecords_GroomingServiceId1",
                table: "GroomingRecords",
                column: "GroomingServiceId1");

            migrationBuilder.CreateIndex(
                name: "IX_Groomers_Email",
                table: "Groomers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Groomers_UserId",
                table: "Groomers",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_GroomingRecords_Appointments_AppointmentId",
                table: "GroomingRecords",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroomingRecords_Groomers_GroomerId1",
                table: "GroomingRecords",
                column: "GroomerId1",
                principalTable: "Groomers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GroomingRecords_GroomingServices_GroomingServiceId1",
                table: "GroomingRecords",
                column: "GroomingServiceId1",
                principalTable: "GroomingServices",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroomingRecords_Appointments_AppointmentId",
                table: "GroomingRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_GroomingRecords_Groomers_GroomerId1",
                table: "GroomingRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_GroomingRecords_GroomingServices_GroomingServiceId1",
                table: "GroomingRecords");

            migrationBuilder.DropIndex(
                name: "IX_PetOwners_Email",
                table: "PetOwners");

            migrationBuilder.DropIndex(
                name: "IX_PetOwners_UserId",
                table: "PetOwners");

            migrationBuilder.DropIndex(
                name: "IX_GroomingRecords_AppointmentId",
                table: "GroomingRecords");

            migrationBuilder.DropIndex(
                name: "IX_GroomingRecords_GroomerId1",
                table: "GroomingRecords");

            migrationBuilder.DropIndex(
                name: "IX_GroomingRecords_GroomingServiceId1",
                table: "GroomingRecords");

            migrationBuilder.DropIndex(
                name: "IX_Groomers_Email",
                table: "Groomers");

            migrationBuilder.DropIndex(
                name: "IX_Groomers_UserId",
                table: "Groomers");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "GroomingServices");

            migrationBuilder.DropColumn(
                name: "AppointmentId",
                table: "GroomingRecords");

            migrationBuilder.DropColumn(
                name: "GroomerId1",
                table: "GroomingRecords");

            migrationBuilder.DropColumn(
                name: "GroomingServiceId1",
                table: "GroomingRecords");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<int>(
                name: "Gender",
                table: "Pets",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "PetOwners",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "GroomingServices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Groomers",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Appointments",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.CreateIndex(
                name: "IX_PetOwners_UserId",
                table: "PetOwners",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Groomers_UserId",
                table: "Groomers",
                column: "UserId",
                unique: true);
        }
    }
}
