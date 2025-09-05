using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIMoiFood.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToFood : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Foods",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Foods");
        }
    }
}
