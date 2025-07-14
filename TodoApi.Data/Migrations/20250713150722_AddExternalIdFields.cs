using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalIdFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "TodoList",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "TodoItem",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "TodoList");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "TodoItem");
        }
    }
}
