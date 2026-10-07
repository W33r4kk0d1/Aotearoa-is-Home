using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aotearoa_is_Home.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentChecklistJourneys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudentChecklists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentChecklists", x => x.Id);

                    table.ForeignKey(
                        name: "FK_StudentChecklists_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.AddColumn<int>(
                name: "StudentChecklistId",
                table: "ChecklistItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentChecklists_UserId",
                table: "StudentChecklists",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistItems_StudentChecklistId",
                table: "ChecklistItems",
                column: "StudentChecklistId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChecklistItems_StudentChecklists_StudentChecklistId",
                table: "ChecklistItems",
                column: "StudentChecklistId",
                principalTable: "StudentChecklists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChecklistItems_StudentChecklists_StudentChecklistId",
                table: "ChecklistItems");

            migrationBuilder.DropTable(
                name: "StudentChecklists");

            migrationBuilder.DropColumn(
                name: "StudentChecklistId",
                table: "ChecklistItems");
        }
    }
}
