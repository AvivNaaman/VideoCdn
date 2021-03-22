using Microsoft.EntityFrameworkCore.Migrations;

namespace VideoCdn.Web.Server.Migrations
{
    public partial class available_resolutions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvailableResolutions",
                table: "Catalog",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableResolutions",
                table: "Catalog");
        }
    }
}
