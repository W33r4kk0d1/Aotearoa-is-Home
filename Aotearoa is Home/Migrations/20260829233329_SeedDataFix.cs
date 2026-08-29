using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Aotearoa_is_Home.Migrations
{
    /// <inheritdoc />
    public partial class SeedDataFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "SettlementInformation",
                columns: new[] { "Id", "Description", "Title", "Topic" },
                values: new object[,]
                {
                    { 1, "Information regarding homestays, student accommodation, flatting, and tenant rights.", "Finding Accommodation", "Accommodation" },
                    { 2, "Details about international student support, academic integrity, and university resources.", "Educational & Tertiary Information", "Education" },
                    { 3, "Guidance on finding a GP, registering with a medical centre, and health insurance.", "Health Care and Wellbeing", "Healthcare" },
                    { 4, "Everyday Kiwi words, slang, communication card prompts, and family language support.", "Language Support Resources", "Language Assistance" },
                    { 5, "Information on work rights, CV/cover letter creation, interview etiquette, and workplace culture.", "Employment Rights & Job Hunting", "Employment" },
                    { 6, "Opening a bank account, everyday transaction accounts, tracking weekly budgets, and IRD tax profiles.", "Banking & Financial Management", "Banking & Finance" },
                    { 7, "Bus, train, and ferry logistics alongside NZ road rules, licensing setup, and vehicle ownership.", "Public Transport & Driving", "Transport" },
                    { 8, "Making friends, university student clubs, local community centers, and cultural activities.", "Social Networks & Community", "Community Support" },
                    { 9, "Daycare options, school zone applications, children's health checks, and local parenting culture.", "Childcare & School Enrolment", "Childcare & Family" },
                    { 10, "How to reach services via 111, dealing with natural disasters, and household safety guidelines.", "Emergency Support & Personal Safety", "Emergency & Safety" },
                    { 11, "Details regarding student visa conditions, renewal steps, and staying compliant.", "Immigration & Visa Compliance", "Immigration & Visa" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SettlementInformation",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "SettlementInformation",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "SettlementInformation",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "SettlementInformation",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "SettlementInformation",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "SettlementInformation",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "SettlementInformation",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "SettlementInformation",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "SettlementInformation",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "SettlementInformation",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "SettlementInformation",
                keyColumn: "Id",
                keyValue: 11);
        }
    }
}
