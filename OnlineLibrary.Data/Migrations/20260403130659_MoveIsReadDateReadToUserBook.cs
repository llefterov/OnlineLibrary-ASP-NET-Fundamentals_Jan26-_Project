using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineLibrary.Data.Migrations
{
    /// <inheritdoc />
    public partial class MoveIsReadDateReadToUserBook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateRead",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "IsRead",
                table: "Books");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateRead",
                table: "UsersBooks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                table: "UsersBooks",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateRead",
                table: "UsersBooks");

            migrationBuilder.DropColumn(
                name: "IsRead",
                table: "UsersBooks");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateRead",
                table: "Books",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                table: "Books",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: new Guid("0eea95e2-33be-4bf1-a851-84a040d4432a"),
                columns: new[] { "DateRead", "IsRead" },
                values: new object[] { new DateTime(2019, 8, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), true });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: new Guid("3a1508af-90c6-4eb0-bec6-f7b5ea096d2d"),
                columns: new[] { "DateRead", "IsRead" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: new Guid("6e916262-b412-4232-8e4d-5f822f9da185"),
                columns: new[] { "DateRead", "IsRead" },
                values: new object[] { new DateTime(2018, 11, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), true });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: new Guid("9f5ce95b-c0cd-4f1f-bff3-c3571d003319"),
                columns: new[] { "DateRead", "IsRead" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: new Guid("e6f42e6f-6c1b-4b8f-bb2f-e87eb4fd8ecf"),
                columns: new[] { "DateRead", "IsRead" },
                values: new object[] { new DateTime(2020, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true });
        }
    }
}
