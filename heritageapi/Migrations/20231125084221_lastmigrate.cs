using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeritageApi.Migrations
{
    /// <inheritdoc />
    public partial class lastmigrate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Exhibits",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Exhibits");
        }
    }
}
