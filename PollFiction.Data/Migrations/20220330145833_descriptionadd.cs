using Microsoft.EntityFrameworkCore.Migrations;

namespace PollFiction.Data.Migrations
{
    public partial class descriptionadd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PollDescription",
                table: "Polls",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PollDescription",
                table: "Polls");
        }
    }
}
