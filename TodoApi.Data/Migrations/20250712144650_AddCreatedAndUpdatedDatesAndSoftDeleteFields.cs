using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedAndUpdatedDatesAndSoftDeleteFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TodoItem_TodoList_ListId",
                table: "TodoItem");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "TodoList",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "TodoList",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "TodoList",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<long>(
                name: "ListId",
                table: "TodoItem",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "TodoItem",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "TodoItem",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "TodoItem",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "FK_TodoItem_TodoList_ListId",
                table: "TodoItem",
                column: "ListId",
                principalTable: "TodoList",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TodoItem_TodoList_ListId",
                table: "TodoItem");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "TodoList");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "TodoList");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TodoList");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "TodoItem");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "TodoItem");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TodoItem");

            migrationBuilder.AlterColumn<long>(
                name: "ListId",
                table: "TodoItem",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "FK_TodoItem_TodoList_ListId",
                table: "TodoItem",
                column: "ListId",
                principalTable: "TodoList",
                principalColumn: "Id");
        }
    }
}
