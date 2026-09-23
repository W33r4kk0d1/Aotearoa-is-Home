using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aotearoa_is_Home.Migrations.University
{
    /// <inheritdoc />
    public partial class InitialUniversityDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UniversityStudents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StudentEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ApplicationEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsCurrentStudent = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UniversityStudents", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UniversityStudents");
        }
    }
}
