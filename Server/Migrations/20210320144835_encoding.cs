using Microsoft.EntityFrameworkCore.Migrations;

namespace VideoCdn.Web.Server.Migrations
{
    public partial class encoding : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FileName",
                table: "Catalog",
                newName: "FileId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FileId",
                table: "Catalog",
                newName: "FileName");
        }
    }
}
