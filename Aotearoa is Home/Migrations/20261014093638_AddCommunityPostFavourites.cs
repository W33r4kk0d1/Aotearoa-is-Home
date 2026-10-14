using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aotearoa_is_Home.Migrations
{
    /// <inheritdoc />
    public partial class AddCommunityPostFavourites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommunityPostFavourites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommunityPostId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityPostFavourites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommunityPostFavourites_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommunityPostFavourites_CommunityPosts_CommunityPostId",
                        column: x => x.CommunityPostId,
                        principalTable: "CommunityPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommunityPostFavourites_CommunityPostId_UserId",
                table: "CommunityPostFavourites",
                columns: new[] { "CommunityPostId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommunityPostFavourites_UserId",
                table: "CommunityPostFavourites",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommunityPostFavourites");
        }
    }
}
