using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hotel_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRegularPriceMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RegularPrice",
                table: "Cabins",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegularPrice",
                table: "Cabins");
        }
    }
}
