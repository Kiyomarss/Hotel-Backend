using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hotel_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NameOfMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Image",
                table: "Cabins");

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Cabins",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Cabins");

            migrationBuilder.AddColumn<byte[]>(
                name: "Image",
                table: "Cabins",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
