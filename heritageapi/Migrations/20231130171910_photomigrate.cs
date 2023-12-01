using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeritageApi.Migrations
{
    /// <inheritdoc />
    public partial class photomigrate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Exhibits");

            migrationBuilder.AddColumn<byte[]>(
                name: "Image",
                table: "Exhibits",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "ImageFileName",
                table: "Exhibits",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Image",
                table: "Exhibits");

            migrationBuilder.DropColumn(
                name: "ImageFileName",
                table: "Exhibits");

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Exhibits",
                type: "text",
                nullable: true);
        }
    }
}
