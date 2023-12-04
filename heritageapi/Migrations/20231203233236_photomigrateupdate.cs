using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeritageApi.Migrations
{
    /// <inheritdoc />
    public partial class photomigrateupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageFileName",
                table: "Exhibits",
                newName: "PreviewImageFileName");

            migrationBuilder.AddColumn<string>(
                name: "DetailImageFileName",
                table: "Exhibits",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DetailImageFileName",
                table: "Exhibits");

            migrationBuilder.RenameColumn(
                name: "PreviewImageFileName",
                table: "Exhibits",
                newName: "ImageFileName");
        }
    }
}
