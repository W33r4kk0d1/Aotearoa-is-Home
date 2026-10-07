using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aotearoa_is_Home.Migrations
{
    /// <inheritdoc />
    public partial class addwnw : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "ChecklistItems");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "ChecklistItems");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "ChecklistItems");

            migrationBuilder.AddColumn<int>(
                name: "ChecklistTaskId",
                table: "ChecklistItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ChecklistTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SettlementPageId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChecklistTasks_SettlementPages_SettlementPageId",
                        column: x => x.SettlementPageId,
                        principalTable: "SettlementPages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistItems_ChecklistTaskId",
                table: "ChecklistItems",
                column: "ChecklistTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistTasks_SettlementPageId",
                table: "ChecklistTasks",
                column: "SettlementPageId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChecklistItems_ChecklistTasks_ChecklistTaskId",
                table: "ChecklistItems",
                column: "ChecklistTaskId",
                principalTable: "ChecklistTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChecklistItems_ChecklistTasks_ChecklistTaskId",
                table: "ChecklistItems");

            migrationBuilder.DropTable(
                name: "ChecklistTasks");

            migrationBuilder.DropIndex(
                name: "IX_ChecklistItems_ChecklistTaskId",
                table: "ChecklistItems");

            migrationBuilder.DropColumn(
                name: "ChecklistTaskId",
                table: "ChecklistItems");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "ChecklistItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ChecklistItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "ChecklistItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
