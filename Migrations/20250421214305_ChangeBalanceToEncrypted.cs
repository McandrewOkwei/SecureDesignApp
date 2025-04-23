using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalProject.Migrations
{
    /// <inheritdoc />
    public partial class ChangeBalanceToEncrypted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Balance",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "EncBalance",
                table: "Users",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EncBalance",
                table: "Users");

            migrationBuilder.AddColumn<double>(
                name: "Balance",
                table: "Users",
                type: "double",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
