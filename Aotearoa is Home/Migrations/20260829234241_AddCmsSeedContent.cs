using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Aotearoa_is_Home.Migrations
{
    /// <inheritdoc />
    public partial class AddCmsSeedContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "SettlementPages",
                columns: new[] { "Id", "CategoryName" },
                values: new object[,]
                {
                    { 1, "Accommodation" },
                    { 2, "Education" },
                    { 3, "Healthcare" },
                    { 4, "Language Assistance" },
                    { 5, "Employment" },
                    { 6, "Banking & Finance" },
                    { 7, "Transport" },
                    { 8, "Community Support" },
                    { 9, "Childcare & Family" },
                    { 10, "Emergency Support" }
                });

            migrationBuilder.InsertData(
                table: "ContentBlocks",
                columns: new[] { "Id", "Content", "DisplayOrder", "SettlementPageId", "Type" },
                values: new object[,]
                {
                    { 1, "Finding Accommodation in New Zealand", 1, 1, "Heading" },
                    { 2, "Options include Homestays, Student accommodation, Youth hostels, Shared apartments, and Rental houses.", 2, 1, "Paragraph" },
                    { 3, "Renting Guidelines", 3, 1, "Heading" },
                    { 4, "Ensure you understand your tenancy agreement, bond payments (usually 2-4 weeks rent), and tenant rights.", 4, 1, "Paragraph" },
                    { 5, "Tertiary Information & Support", 1, 2, "Heading" },
                    { 6, "Utilize campus international student support, academic learning hubs, library networks, and career advisory services.", 2, 2, "Paragraph" },
                    { 7, "Medical Centres & Insurance", 1, 3, "Heading" },
                    { 8, "Register with a local General Practitioner (GP). International students must maintain current medical insurance coverage.", 2, 3, "Paragraph" },
                    { 9, "NZ Workplace Rights", 1, 5, "Heading" },
                    { 10, "All workers are entitled to minimum wage, scheduled breaks, sick leave, and protection from workplace harassment.", 2, 5, "Paragraph" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ContentBlocks",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ContentBlocks",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ContentBlocks",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ContentBlocks",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ContentBlocks",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "ContentBlocks",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "ContentBlocks",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "ContentBlocks",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "ContentBlocks",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "ContentBlocks",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "SettlementPages",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "SettlementPages",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "SettlementPages",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "SettlementPages",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "SettlementPages",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "SettlementPages",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "SettlementPages",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "SettlementPages",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "SettlementPages",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "SettlementPages",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
