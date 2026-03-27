using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineLibrary.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDeletedToBookAuthor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "BooksAuthors",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "BooksAuthors",
                keyColumns: new[] { "AuthorId", "BookId" },
                keyValues: new object[] { new Guid("9a17ee9f-f2c6-44f4-9247-ec5fcbec4f92"), new Guid("0eea95e2-33be-4bf1-a851-84a040d4432a") },
                column: "IsDeleted",
                value: false);

            migrationBuilder.UpdateData(
                table: "BooksAuthors",
                keyColumns: new[] { "AuthorId", "BookId" },
                keyValues: new object[] { new Guid("f625bc9b-1b34-44fd-b88b-cbec15d529c6"), new Guid("3a1508af-90c6-4eb0-bec6-f7b5ea096d2d") },
                column: "IsDeleted",
                value: false);

            migrationBuilder.UpdateData(
                table: "BooksAuthors",
                keyColumns: new[] { "AuthorId", "BookId" },
                keyValues: new object[] { new Guid("fb1ac6db-3c36-4f35-bdce-8dcd4a61156e"), new Guid("6e916262-b412-4232-8e4d-5f822f9da185") },
                column: "IsDeleted",
                value: false);

            migrationBuilder.UpdateData(
                table: "BooksAuthors",
                keyColumns: new[] { "AuthorId", "BookId" },
                keyValues: new object[] { new Guid("2c5f78aa-7fca-46f7-a112-4db9d4c0d093"), new Guid("9f5ce95b-c0cd-4f1f-bff3-c3571d003319") },
                column: "IsDeleted",
                value: false);

            migrationBuilder.UpdateData(
                table: "BooksAuthors",
                keyColumns: new[] { "AuthorId", "BookId" },
                keyValues: new object[] { new Guid("abebfdd2-7a9e-4aa7-84b8-454b6ac74f1d"), new Guid("e6f42e6f-6c1b-4b8f-bb2f-e87eb4fd8ecf") },
                column: "IsDeleted",
                value: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "BooksAuthors");
        }
    }
}
