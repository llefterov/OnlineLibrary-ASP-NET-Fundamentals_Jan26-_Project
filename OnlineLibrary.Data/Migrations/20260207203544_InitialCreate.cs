using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OnlineLibrary.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Authors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Publishers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Publishers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Genre = table.Column<int>(type: "int", maxLength: 100, nullable: false),
                    isRead = table.Column<bool>(type: "bit", nullable: false),
                    DateRead = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    CoverUrl = table.Column<string>(type: "nvarchar(2083)", maxLength: 2083, nullable: false),
                    AddedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PublisherId = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Books_AspNetUsers_AddedByUserId",
                        column: x => x.AddedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Books_Publishers_PublisherId",
                        column: x => x.PublisherId,
                        principalTable: "Publishers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BooksAuthors",
                columns: table => new
                {
                    BookId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuthorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BooksAuthors", x => new { x.BookId, x.AuthorId });
                    table.ForeignKey(
                        name: "FK_BooksAuthors_Authors_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Authors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BooksAuthors_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Authors",
                columns: new[] { "Id", "FullName" },
                values: new object[,]
                {
                    { 1, "Jane Austen" },
                    { 2, "George Orwell" },
                    { 3, "Isaac Asimov" },
                    { 4, "Agatha Christie" },
                    { 5, "J.K. Rowling" }
                });

            migrationBuilder.InsertData(
                table: "Publishers",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Apress" },
                    { 2, "Manning Publications" },
                    { 3, "O'Reilly Media" },
                    { 4, "Packt Publishing" },
                    { 5, "Addison-Wesley" }
                });

            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "Id", "AddedByUserId", "CoverUrl", "DateAdded", "DateRead", "Description", "Genre", "IsDeleted", "PublisherId", "Rating", "Title", "isRead" },
                values: new object[,]
                {
                    { new Guid("1411eab8-b839-441d-a72d-2bb3cf7aa218"), null, "https://www.blackcat-cideb.com/uploads/2020/02/COVER_Murder_on_the_orient_express_Agatha-Christie_f2a379ae1e65e577f341258edaba4148.jpg", new DateTime(2022, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Classic mystery featuring detective Hercule Poirot.", 2, false, 5, 0, "Murder on the Orient Express", false },
                    { new Guid("23c5dbca-dba7-46ff-ae96-7b233a8ca88c"), null, "https://resizing.flixster.com/-XZAfHZM39UwaGJIFWKAE8fS0ak=/v3/t/assets/p9458059_p_v10_ac.jpg", new DateTime(2018, 10, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2018, 11, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Fantasy adventure preceding the events of The Lord of the Rings.", 3, false, 4, 5, "The Hobbit", true },
                    { new Guid("2dc0a369-6c0d-44a7-a7b0-41959009d322"), null, "https://m.media-amazon.com/images/I/612ADI+BVlL._AC_UF1000,1000_QL80_.jpg", new DateTime(2019, 6, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2019, 8, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dystopian novel about surveillance and totalitarianism.", 4, false, 2, 5, "1984", true },
                    { new Guid("c697d648-8fc0-41cb-9fb1-105792262850"), null, "https://cdn.mos.cms.futurecdn.net/oFCCtndaa9gxNqmJDY6Rp8.jpg", new DateTime(2021, 3, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Epic science fiction series about the fall and rise of galactic empires.", 4, false, 3, 0, "Foundation", false },
                    { new Guid("f0c604df-a030-437f-9028-0ada33e35b85"), null, "https://upload.wikimedia.org/wikipedia/en/0/03/Prideandprejudiceposter.jpg", new DateTime(2020, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2020, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "A classic novel about love and society in early 19th-century England.", 0, false, 1, 5, "Pride and Prejudice", true }
                });

            migrationBuilder.InsertData(
                table: "BooksAuthors",
                columns: new[] { "AuthorId", "BookId" },
                values: new object[,]
                {
                    { 5, new Guid("1411eab8-b839-441d-a72d-2bb3cf7aa218") },
                    { 4, new Guid("23c5dbca-dba7-46ff-ae96-7b233a8ca88c") },
                    { 2, new Guid("2dc0a369-6c0d-44a7-a7b0-41959009d322") },
                    { 3, new Guid("c697d648-8fc0-41cb-9fb1-105792262850") },
                    { 1, new Guid("f0c604df-a030-437f-9028-0ada33e35b85") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Books_AddedByUserId",
                table: "Books",
                column: "AddedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Books_PublisherId",
                table: "Books",
                column: "PublisherId");

            migrationBuilder.CreateIndex(
                name: "IX_BooksAuthors_AuthorId",
                table: "BooksAuthors",
                column: "AuthorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BooksAuthors");

            migrationBuilder.DropTable(
                name: "Authors");

            migrationBuilder.DropTable(
                name: "Books");

            migrationBuilder.DropTable(
                name: "Publishers");
        }
    }
}
