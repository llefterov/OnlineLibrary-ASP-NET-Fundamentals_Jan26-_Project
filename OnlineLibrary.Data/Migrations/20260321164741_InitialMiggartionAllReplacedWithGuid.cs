using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OnlineLibrary.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialMiggartionAllReplacedWithGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Authors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Publishers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Genre = table.Column<int>(type: "int", maxLength: 100, nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    DateRead = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    CoverUrl = table.Column<string>(type: "nvarchar(2083)", maxLength: 2083, nullable: true),
                    AddedByUserId = table.Column<Guid>(type: "uniqueidentifier", maxLength: 450, nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PublisherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Books_AspNetUsers_AddedByUserId",
                        column: x => x.AddedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    AuthorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "UsersBooks",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersBooks", x => new { x.UserId, x.BookId });
                    table.ForeignKey(
                        name: "FK_UsersBooks_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_UsersBooks_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { new Guid("d6f5de2f-4be6-4f7a-a3b5-c7c0d72ac8f1"), 0, "8a242145-60f2-4e35-8e5b-693a7d7bb6f4", "admin@onlinelibrary.com", true, false, null, "ADMIN@ONLINELIBRARY.COM", "ADMIN@ONLINELIBRARY.COM", "AQAAAAIAAYagAAAAEHIaJlSfhU4K8tmJOH0vNC1Seaj11r0efwD90KrvKI+D5PDGPPUoibcXFsK6Gfh1Yg==", null, false, "3f5e9b60-7f08-4a84-9a2c-ec9d32f1cc7b", false, "admin@onlinelibrary.com" });

            migrationBuilder.InsertData(
                table: "Authors",
                columns: new[] { "Id", "FullName" },
                values: new object[,]
                {
                    { new Guid("2c5f78aa-7fca-46f7-a112-4db9d4c0d093"), "Isaac Asimov" },
                    { new Guid("9a17ee9f-f2c6-44f4-9247-ec5fcbec4f92"), "George Orwell" },
                    { new Guid("abebfdd2-7a9e-4aa7-84b8-454b6ac74f1d"), "Jane Austen" },
                    { new Guid("f625bc9b-1b34-44fd-b88b-cbec15d529c6"), "Agatha Christie" },
                    { new Guid("fb1ac6db-3c36-4f35-bdce-8dcd4a61156e"), "R.R. Tolkien" }
                });

            migrationBuilder.InsertData(
                table: "Publishers",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("2da9d7ca-8f3f-4f0f-ab28-e84fc351338e"), "O'Reilly Media" },
                    { new Guid("67ad1f17-8e47-47ce-be56-9d7bc2c09736"), "Manning Publications" },
                    { new Guid("7b1cb8a1-cce4-4fe8-b258-a03e3468b761"), "Apress" },
                    { new Guid("e66f13fa-3f9b-4304-a450-7d0b907f22ec"), "Addison-Wesley" },
                    { new Guid("f2f4bc8f-b080-4624-8ba5-f64f0d006778"), "Packt Publishing" }
                });

            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "Id", "AddedByUserId", "CoverUrl", "DateAdded", "DateRead", "Description", "Genre", "IsDeleted", "IsRead", "PublisherId", "Rating", "Title" },
                values: new object[,]
                {
                    { new Guid("0eea95e2-33be-4bf1-a851-84a040d4432a"), new Guid("d6f5de2f-4be6-4f7a-a3b5-c7c0d72ac8f1"), "https://m.media-amazon.com/images/I/612ADI+BVlL._AC_UF1000,1000_QL80_.jpg", new DateTime(2019, 6, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2019, 8, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dystopian novel about surveillance and totalitarianism.", 4, false, true, new Guid("67ad1f17-8e47-47ce-be56-9d7bc2c09736"), 5, "1984" },
                    { new Guid("3a1508af-90c6-4eb0-bec6-f7b5ea096d2d"), new Guid("d6f5de2f-4be6-4f7a-a3b5-c7c0d72ac8f1"), "https://www.blackcat-cideb.com/uploads/2020/02/COVER_Murder_on_the_orient_express_Agatha-Christie_f2a379ae1e65e577f341258edaba4148.jpg", new DateTime(2022, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Classic mystery featuring detective Hercule Poirot.", 2, false, false, new Guid("e66f13fa-3f9b-4304-a450-7d0b907f22ec"), 0, "Murder on the Orient Express" },
                    { new Guid("6e916262-b412-4232-8e4d-5f822f9da185"), new Guid("d6f5de2f-4be6-4f7a-a3b5-c7c0d72ac8f1"), "https://resizing.flixster.com/-XZAfHZM39UwaGJIFWKAE8fS0ak=/v3/t/assets/p9458059_p_v10_ac.jpg", new DateTime(2018, 10, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2018, 11, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Fantasy adventure preceding the events of The Lord of the Rings.", 3, false, true, new Guid("f2f4bc8f-b080-4624-8ba5-f64f0d006778"), 5, "The Hobbit" },
                    { new Guid("9f5ce95b-c0cd-4f1f-bff3-c3571d003319"), new Guid("d6f5de2f-4be6-4f7a-a3b5-c7c0d72ac8f1"), "https://cdn.mos.cms.futurecdn.net/oFCCtndaa9gxNqmJDY6Rp8.jpg", new DateTime(2021, 3, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Epic science fiction series about the fall and rise of galactic empires.", 4, false, false, new Guid("2da9d7ca-8f3f-4f0f-ab28-e84fc351338e"), 0, "Foundation" },
                    { new Guid("e6f42e6f-6c1b-4b8f-bb2f-e87eb4fd8ecf"), new Guid("d6f5de2f-4be6-4f7a-a3b5-c7c0d72ac8f1"), "https://upload.wikimedia.org/wikipedia/en/0/03/Prideandprejudiceposter.jpg", new DateTime(2020, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2020, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "A classic novel about love and society in early 19th-century England.", 0, false, true, new Guid("7b1cb8a1-cce4-4fe8-b258-a03e3468b761"), 5, "Pride and Prejudice" }
                });

            migrationBuilder.InsertData(
                table: "BooksAuthors",
                columns: new[] { "AuthorId", "BookId" },
                values: new object[,]
                {
                    { new Guid("9a17ee9f-f2c6-44f4-9247-ec5fcbec4f92"), new Guid("0eea95e2-33be-4bf1-a851-84a040d4432a") },
                    { new Guid("f625bc9b-1b34-44fd-b88b-cbec15d529c6"), new Guid("3a1508af-90c6-4eb0-bec6-f7b5ea096d2d") },
                    { new Guid("fb1ac6db-3c36-4f35-bdce-8dcd4a61156e"), new Guid("6e916262-b412-4232-8e4d-5f822f9da185") },
                    { new Guid("2c5f78aa-7fca-46f7-a112-4db9d4c0d093"), new Guid("9f5ce95b-c0cd-4f1f-bff3-c3571d003319") },
                    { new Guid("abebfdd2-7a9e-4aa7-84b8-454b6ac74f1d"), new Guid("e6f42e6f-6c1b-4b8f-bb2f-e87eb4fd8ecf") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

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

            migrationBuilder.CreateIndex(
                name: "IX_UsersBooks_BookId",
                table: "UsersBooks",
                column: "BookId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "BooksAuthors");

            migrationBuilder.DropTable(
                name: "UsersBooks");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Authors");

            migrationBuilder.DropTable(
                name: "Books");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Publishers");
        }
    }
}
