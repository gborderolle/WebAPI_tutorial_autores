using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPI_tutorial_recursos.Migrations
{
    /// <inheritdoc />
    public partial class comentariousuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Review",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Author",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Creation", "Update" },
                values: new object[] { new DateTime(2023, 8, 20, 15, 2, 31, 348, DateTimeKind.Local).AddTicks(6391), new DateTime(2023, 8, 20, 15, 2, 31, 348, DateTimeKind.Local).AddTicks(6399) });

            migrationBuilder.UpdateData(
                table: "Author",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Creation", "Update" },
                values: new object[] { new DateTime(2023, 8, 20, 15, 2, 31, 348, DateTimeKind.Local).AddTicks(6403), new DateTime(2023, 8, 20, 15, 2, 31, 348, DateTimeKind.Local).AddTicks(6403) });

            migrationBuilder.UpdateData(
                table: "Author",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Creation", "Update" },
                values: new object[] { new DateTime(2023, 8, 20, 15, 2, 31, 348, DateTimeKind.Local).AddTicks(6404), new DateTime(2023, 8, 20, 15, 2, 31, 348, DateTimeKind.Local).AddTicks(6404) });

            migrationBuilder.UpdateData(
                table: "Author",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Creation", "Update" },
                values: new object[] { new DateTime(2023, 8, 20, 15, 2, 31, 348, DateTimeKind.Local).AddTicks(6405), new DateTime(2023, 8, 20, 15, 2, 31, 348, DateTimeKind.Local).AddTicks(6405) });

            migrationBuilder.UpdateData(
                table: "Author",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Creation", "Update" },
                values: new object[] { new DateTime(2023, 8, 20, 15, 2, 31, 348, DateTimeKind.Local).AddTicks(6406), new DateTime(2023, 8, 20, 15, 2, 31, 348, DateTimeKind.Local).AddTicks(6406) });

            migrationBuilder.UpdateData(
                table: "Book",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Creation", "Update" },
                values: new object[] { new DateTime(2023, 8, 20, 15, 2, 31, 348, DateTimeKind.Local).AddTicks(6540), new DateTime(2023, 8, 20, 15, 2, 31, 348, DateTimeKind.Local).AddTicks(6541) });

            migrationBuilder.UpdateData(
                table: "Book",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Creation", "Update" },
                values: new object[] { new DateTime(2023, 8, 20, 15, 2, 31, 348, DateTimeKind.Local).AddTicks(6543), new DateTime(2023, 8, 20, 15, 2, 31, 348, DateTimeKind.Local).AddTicks(6544) });

            migrationBuilder.CreateIndex(
                name: "IX_Review_UserId",
                table: "Review",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Review_AspNetUsers_UserId",
                table: "Review",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Review_AspNetUsers_UserId",
                table: "Review");

            migrationBuilder.DropIndex(
                name: "IX_Review_UserId",
                table: "Review");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Review");

            migrationBuilder.UpdateData(
                table: "Author",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Creation", "Update" },
                values: new object[] { new DateTime(2023, 8, 20, 13, 18, 8, 346, DateTimeKind.Local).AddTicks(3974), new DateTime(2023, 8, 20, 13, 18, 8, 346, DateTimeKind.Local).AddTicks(3984) });

            migrationBuilder.UpdateData(
                table: "Author",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Creation", "Update" },
                values: new object[] { new DateTime(2023, 8, 20, 13, 18, 8, 346, DateTimeKind.Local).AddTicks(3988), new DateTime(2023, 8, 20, 13, 18, 8, 346, DateTimeKind.Local).AddTicks(3988) });

            migrationBuilder.UpdateData(
                table: "Author",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Creation", "Update" },
                values: new object[] { new DateTime(2023, 8, 20, 13, 18, 8, 346, DateTimeKind.Local).AddTicks(3989), new DateTime(2023, 8, 20, 13, 18, 8, 346, DateTimeKind.Local).AddTicks(3989) });

            migrationBuilder.UpdateData(
                table: "Author",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Creation", "Update" },
                values: new object[] { new DateTime(2023, 8, 20, 13, 18, 8, 346, DateTimeKind.Local).AddTicks(3990), new DateTime(2023, 8, 20, 13, 18, 8, 346, DateTimeKind.Local).AddTicks(3990) });

            migrationBuilder.UpdateData(
                table: "Author",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Creation", "Update" },
                values: new object[] { new DateTime(2023, 8, 20, 13, 18, 8, 346, DateTimeKind.Local).AddTicks(3992), new DateTime(2023, 8, 20, 13, 18, 8, 346, DateTimeKind.Local).AddTicks(3992) });

            migrationBuilder.UpdateData(
                table: "Book",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Creation", "Update" },
                values: new object[] { new DateTime(2023, 8, 20, 13, 18, 8, 346, DateTimeKind.Local).AddTicks(4104), new DateTime(2023, 8, 20, 13, 18, 8, 346, DateTimeKind.Local).AddTicks(4104) });

            migrationBuilder.UpdateData(
                table: "Book",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Creation", "Update" },
                values: new object[] { new DateTime(2023, 8, 20, 13, 18, 8, 346, DateTimeKind.Local).AddTicks(4106), new DateTime(2023, 8, 20, 13, 18, 8, 346, DateTimeKind.Local).AddTicks(4106) });
        }
    }
}
