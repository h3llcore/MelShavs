using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace course_melshavs.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AgreePrivacyPolicy",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ConfirmPassword",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgreePrivacyPolicy",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ConfirmPassword",
                table: "Users");
        }
    }
}
