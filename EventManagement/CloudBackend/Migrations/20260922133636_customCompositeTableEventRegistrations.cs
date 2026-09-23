using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudBackend.Migrations
{
    /// <inheritdoc />
    public partial class customCompositeTableEventRegistrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventStudent");

            migrationBuilder.CreateTable(
                name: "EventRegistrations",
                columns: table => new
                {
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    EventId = table.Column<int>(type: "integer", nullable: false),
                    CheckedIn = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventRegistrations", x => new { x.StudentId, x.EventId });
                    table.ForeignKey(
                        name: "FK_EventRegistrations_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventRegistrations_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventRegistrations_EventId",
                table: "EventRegistrations",
                column: "EventId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventRegistrations");

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
    }
}
