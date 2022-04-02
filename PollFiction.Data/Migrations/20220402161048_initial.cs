using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PollFiction.Data.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Guests",
                columns: table => new
                {
                    GuestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GuestMail = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guests", x => x.GuestId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserPseudo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserMail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserUuidCookie = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserPwd = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Polls",
                columns: table => new
                {
                    PollId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PollTitle = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PollDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Polldate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PollMultiple = table.Column<bool>(type: "bit", nullable: false),
                    PollDisable = table.Column<bool>(type: "bit", nullable: false),
                    PollLinkAccess = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PollLinkDisable = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PollLinkStat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Polls", x => x.PollId);
                    table.ForeignKey(
                        name: "FK_Polls_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Choices",
                columns: table => new
                {
                    ChoiceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChoiceText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PollId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Choices", x => x.ChoiceId);
                    table.ForeignKey(
                        name: "FK_Choices_Polls_PollId",
                        column: x => x.PollId,
                        principalTable: "Polls",
                        principalColumn: "PollId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PollGuests",
                columns: table => new
                {
                    PollGuestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PollId = table.Column<int>(type: "int", nullable: false),
                    GuestId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PollGuests", x => x.PollGuestId);
                    table.ForeignKey(
                        name: "FK_PollGuests_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "GuestId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PollGuests_Polls_PollId",
                        column: x => x.PollId,
                        principalTable: "Polls",
                        principalColumn: "PollId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GuestChoices",
                columns: table => new
                {
                    GuestChoiceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChoiceId = table.Column<int>(type: "int", nullable: false),
                    NumberVote = table.Column<int>(type: "int", nullable: false),
                    GuestId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuestChoices", x => x.GuestChoiceId);
                    table.ForeignKey(
                        name: "FK_GuestChoices_Choices_ChoiceId",
                        column: x => x.ChoiceId,
                        principalTable: "Choices",
                        principalColumn: "ChoiceId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GuestChoices_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "GuestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Choices_PollId",
                table: "Choices",
                column: "PollId");

            migrationBuilder.CreateIndex(
                name: "IX_GuestChoices_ChoiceId",
                table: "GuestChoices",
                column: "ChoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_GuestChoices_GuestId",
                table: "GuestChoices",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_PollGuests_GuestId",
                table: "PollGuests",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_PollGuests_PollId",
                table: "PollGuests",
                column: "PollId");

            migrationBuilder.CreateIndex(
                name: "IX_Polls_UserId",
                table: "Polls",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuestChoices");

            migrationBuilder.DropTable(
                name: "PollGuests");

            migrationBuilder.DropTable(
                name: "Choices");

            migrationBuilder.DropTable(
                name: "Guests");

            migrationBuilder.DropTable(
                name: "Polls");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
