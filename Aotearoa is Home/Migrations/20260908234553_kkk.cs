using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Aotearoa_is_Home.Migrations
{
    /// <inheritdoc />
    public partial class kkk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.InsertData(
                table: "SettlementPages",
                columns: new[] { "Id", "BackgroundImage", "BackgroundImageContentType", "CategoryName" },
                values: new object[,]
                {
                    { 1, null, null, "Accommodation" },
                    { 2, null, null, "Education" },
                    { 3, null, null, "Healthcare" },
                    { 4, null, null, "Language Assistance" },
                    { 5, null, null, "Employment" },
                    { 6, null, null, "Banking & Finance" },
                    { 7, null, null, "Transport" },
                    { 8, null, null, "Community Support" },
                    { 9, null, null, "Childcare & Family" },
                    { 10, null, null, "Emergency Support" }
                });

            migrationBuilder.InsertData(
                table: "ContentBlocks",
                columns: new[] { "Id", "Content", "Details", "DisplayOrder", "ImageContentType", "ImageData", "SettlementPageId", "Type" },
                values: new object[,]
                {
                    { 1, "Finding Accommodation in New Zealand", null, 1, null, null, 1, "Heading" },
                    { 2, "Options include Homestays, Student accommodation, Youth hostels, Shared apartments, and Rental houses.", null, 2, null, null, 1, "Paragraph" },
                    { 3, "Renting Guidelines", null, 3, null, null, 1, "Heading" },
                    { 4, "Ensure you understand your tenancy agreement, bond payments, and tenant rights.", null, 4, null, null, 1, "Paragraph" },
                    { 5, "Tertiary Information & Support", null, 1, null, null, 2, "Heading" },
                    { 6, "Utilize campus international student support, academic learning hubs, library networks, and career advisory services.", null, 2, null, null, 2, "Paragraph" },
                    { 7, "Medical Centres & Insurance", null, 1, null, null, 3, "Heading" },
                    { 8, "Register with a local General Practitioner (GP). International students must maintain current medical insurance coverage.", null, 2, null, null, 3, "Paragraph" },
                    { 9, "NZ Workplace Rights", null, 1, null, null, 5, "Heading" },
                    { 10, "All workers are entitled to minimum wage, scheduled breaks, sick leave, and protection from workplace harassment.", null, 2, null, null, 5, "Paragraph" }
                });
        }
    }
}
