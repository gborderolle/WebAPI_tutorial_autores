using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WebAPI_tutorial_recursos.Migrations
{
    /// <inheritdoc />
    public partial class migracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Author",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Creation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Update = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Author", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Book",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Creation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Update = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Book", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuthorBook",
                columns: table => new
                {
                    AuthorId = table.Column<int>(type: "int", nullable: false),
                    BookId = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorBook", x => new { x.AuthorId, x.BookId });
                    table.ForeignKey(
                        name: "FK_AuthorBook_Author_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Author",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuthorBook_Book_BookId",
                        column: x => x.BookId,
                        principalTable: "Book",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Review",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BookId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Review", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Review_Book_BookId",
                        column: x => x.BookId,
                        principalTable: "Book",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Author",
                columns: new[] { "Id", "Creation", "Name", "Update" },
                values: new object[,]
                {
                    { 1, new DateTime(2023, 8, 19, 23, 42, 49, 660, DateTimeKind.Local).AddTicks(63), "Gonzalo", new DateTime(2023, 8, 19, 23, 42, 49, 660, DateTimeKind.Local).AddTicks(74) },
                    { 2, new DateTime(2023, 8, 19, 23, 42, 49, 660, DateTimeKind.Local).AddTicks(77), "Ramiro", new DateTime(2023, 8, 19, 23, 42, 49, 660, DateTimeKind.Local).AddTicks(77) },
                    { 3, new DateTime(2023, 8, 19, 23, 42, 49, 660, DateTimeKind.Local).AddTicks(78), "Daniel", new DateTime(2023, 8, 19, 23, 42, 49, 660, DateTimeKind.Local).AddTicks(79) },
                    { 4, new DateTime(2023, 8, 19, 23, 42, 49, 660, DateTimeKind.Local).AddTicks(80), "Gastón", new DateTime(2023, 8, 19, 23, 42, 49, 660, DateTimeKind.Local).AddTicks(80) },
                    { 5, new DateTime(2023, 8, 19, 23, 42, 49, 660, DateTimeKind.Local).AddTicks(81), "Martín", new DateTime(2023, 8, 19, 23, 42, 49, 660, DateTimeKind.Local).AddTicks(82) }
                });

            migrationBuilder.InsertData(
                table: "Book",
                columns: new[] { "Id", "Creation", "Title", "Update" },
                values: new object[,]
                {
                    { 1, new DateTime(2023, 8, 19, 23, 42, 49, 660, DateTimeKind.Local).AddTicks(200), "El libro de la selva", new DateTime(2023, 8, 19, 23, 42, 49, 660, DateTimeKind.Local).AddTicks(201) },
                    { 2, new DateTime(2023, 8, 19, 23, 42, 49, 660, DateTimeKind.Local).AddTicks(202), "La vida de Steve Jobs", new DateTime(2023, 8, 19, 23, 42, 49, 660, DateTimeKind.Local).AddTicks(203) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthorBook_BookId",
                table: "AuthorBook",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_Review_BookId",
                table: "Review",
                column: "BookId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthorBook");

            migrationBuilder.DropTable(
                name: "Review");

            migrationBuilder.DropTable(
                name: "Author");

            migrationBuilder.DropTable(
                name: "Book");
        }
    }
}
