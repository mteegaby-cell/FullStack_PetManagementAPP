using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PETMSCRUD.Migrations
{
    /// <inheritdoc />
    public partial class _2ndattempt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groomers_AspNetUsers_UserId",
                table: "Groomers");

            migrationBuilder.DropForeignKey(
                name: "FK_PetOwners_AspNetUsers_UserId",
                table: "PetOwners");

            migrationBuilder.AddForeignKey(
                name: "FK_Groomers_AspNetUsers_UserId",
                table: "Groomers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PetOwners_AspNetUsers_UserId",
                table: "PetOwners",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groomers_AspNetUsers_UserId",
                table: "Groomers");

            migrationBuilder.DropForeignKey(
                name: "FK_PetOwners_AspNetUsers_UserId",
                table: "PetOwners");

            migrationBuilder.AddForeignKey(
                name: "FK_Groomers_AspNetUsers_UserId",
                table: "Groomers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PetOwners_AspNetUsers_UserId",
                table: "PetOwners",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
