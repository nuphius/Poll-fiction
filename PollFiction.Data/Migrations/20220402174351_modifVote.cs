using Microsoft.EntityFrameworkCore.Migrations;

namespace PollFiction.Data.Migrations
{
    public partial class modifVote : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumberVote",
                table: "GuestChoices");

            migrationBuilder.AddColumn<int>(
                name: "NumberVote",
                table: "Choices",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumberVote",
                table: "Choices");

            migrationBuilder.AddColumn<int>(
                name: "NumberVote",
                table: "GuestChoices",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
