using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudBackend.Migrations
{
    /// <inheritdoc />
    public partial class addStudentsOnEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Students_StudentId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_StudentId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "Events");

            migrationBuilder.CreateTable(
                name: "EventStudent",
                columns: table => new
                {
                    EventsId = table.Column<int>(type: "integer", nullable: false),
                    StudentsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventStudent", x => new { x.EventsId, x.StudentsId });
                    table.ForeignKey(
                        name: "FK_EventStudent_Events_EventsId",
                        column: x => x.EventsId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventStudent_Students_StudentsId",
                        column: x => x.StudentsId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventStudent_StudentsId",
                table: "EventStudent",
                column: "StudentsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventStudent");

            migrationBuilder.AddColumn<int>(
                name: "StudentId",
                table: "Events",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_StudentId",
                table: "Events",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Students_StudentId",
                table: "Events",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id");
        }
    }
}
