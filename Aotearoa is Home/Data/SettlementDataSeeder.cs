using System.IO;
using Aotearoa_is_Home.Models;
using Microsoft.EntityFrameworkCore;

namespace Aotearoa_is_Home.Data
{
    /// <summary>
    /// Seeds the initial settlement pages and content blocks.
    /// Keep this separate from ApplicationDbContext so the context remains focused on database configuration and entity relationships.
    /// </summary>
    public static class SettlementDataSeeder
    {
        private sealed record SeedBlock(string Type, string Content, string Details = "");

        private sealed record SeedPage(string CategoryName, IReadOnlyList<SeedBlock> Blocks);

        public static async Task SeedAsync(ApplicationDbContext context, string webRootPath)
        {
            // Seed each category independently. Existing pages and their content are preserved; blocks are added only when that category currently has none.
            var pages = GetSeedPages();

            var categoryImages = new Dictionary<string, string>
            {
                ["Accomodation"] = "Accommodation.jpeg",
                ["Community"] = "Community.jpg",
                ["Language"] = "Language.jpg",
                ["Healthcare"] = "healthcare.jpg",
                ["Childcare"] = "Childcare.jpg",
                ["Emergency"] = "Emergency.jpg",
                ["Employment"] = "Employment.jpg",
                ["Family Support"] = "family-support.jpg",
                ["Education"] = "education.jpeg"
            };

            foreach (var seedPage in pages)
            {
                var page = await context.SettlementPages
                    .FirstOrDefaultAsync(p => p.CategoryName == seedPage.CategoryName);

                if (page == null)
                {
                    page = new SettlementPage
                    {
                        CategoryName = seedPage.CategoryName,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    context.SettlementPages.Add(page);
                    await context.SaveChangesAsync();
                }

                if (categoryImages.TryGetValue(
                        page.CategoryName,
                        out var imageFileName))
                {
                    page.BackgroundImage = LoadCategoryImage(
                        webRootPath,
                        imageFileName);

                    page.BackgroundImageContentType = "image/jpeg";
                    page.UpdatedAt = DateTime.UtcNow;
                }
                await context.SaveChangesAsync();

                var hasBlocks = await context.ContentBlocks
                    .AnyAsync(b => b.SettlementPageId == page.Id);

                if (hasBlocks || seedPage.Blocks.Count == 0)
                    continue;

                var blocks = seedPage.Blocks
                    .Select((block, index) => new ContentBlock
                    {
                        SettlementPageId = page.Id,
                        Type = block.Type,
                        Content = block.Content,
                        Details = block.Details,
                        DisplayOrder = index + 1
                    })
                    .ToList();

                context.ContentBlocks.AddRange(blocks);
                page.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }
        }


        private static byte[] LoadCategoryImage(
            string webRootPath,
            string imageFileName)
        {
            var imagePath = Path.Combine(
                webRootPath,
                "images",
                "Settlement",
                imageFileName);

            if (!File.Exists(imagePath))
            {
                throw new FileNotFoundException(
                    $"Settlement image not found: {imagePath}");
            }

            return File.ReadAllBytes(imagePath);
        }

        private static IReadOnlyList<SeedPage> GetSeedPages() =>
        [
            new("Accomodation",
            [
                // Add your approved Accommodation source content here.
                // The page is created, but no placeholder advice is inserted.
            ]),

            new("Community",
            [
                // Add your approved Community source content here.
            ]),

            new("Language",
            [
                // Add your approved Language source content here.
            ]),

            new("Healthcare",
            [
                new("heading", "Understanding NZ Healthcare"),
                new("subheading", "How the New Zealand Healthcare System Works"),
                new("paragraph", "New Zealand has a combination of publicly funded and privately provided healthcare services. The level of government funding and eligibility depends on a person's citizenship, residency or visa status. International students should check whether they are eligible for publicly funded healthcare and understand what costs they may need to pay themselves."),
                new("link", "Immigration New Zealand||https://www.immigration.govt.nz/live/setting-up-your-life-in-new-zealand/getting-health-care-and-finding-a-doctor/"),
                new("heading", "Public Healthcare"),
                new("paragraph", "Public healthcare services are funded or subsidised by the New Zealand Government for people who meet eligibility requirements. People who are not eligible can still use public healthcare services but will generally need to pay for the services they receive."),
                new("link", "Who can get public healthcare?||https://www.immigration.govt.nz/live/setting-up-your-life-in-new-zealand/getting-health-care-and-finding-a-doctor/who-can-get-public-health-care/"),
                new("heading", "Private Healthcare"),
                new("paragraph", "Private healthcare allows people to pay directly for healthcare services or use private health insurance. Private services may provide more choice over healthcare providers and appointment times, but costs depend on the service and insurance policy."),
                new("link", "Healthcare in New Zealand||https://www.immigration.govt.nz/live/setting-up-your-life-in-new-zealand/getting-health-care-and-finding-a-doctor/"),
                new("heading", "Primary Healthcare"),
                new("paragraph", "Primary healthcare is usually the first point of contact when you have a health problem. It includes services such as GPs, nurses, pharmacies and other community healthcare providers. If you are enrolled with a GP, they can provide ongoing healthcare and refer you to other healthcare professionals when necessary."),
                new("link", "General practices – Health New Zealand||https://www.healthnz.govt.nz/hospitals-services/services-support/general-practices"),
                new("heading", "Secondary Healthcare"),
                new("paragraph", "Secondary healthcare generally refers to specialist and hospital services. A GP or another healthcare professional may refer you to a specialist or hospital when more specialised assessment or treatment is required."),
                new("link", "Hospitals and services – Health New Zealand||https://www.healthnz.govt.nz/hospitals-services"),
                new("heading", "Emergency Healthcare"),
                new("paragraph", "Emergency healthcare is for serious or life-threatening illness and injuries. In a life-threatening emergency, call 111 and ask for an ambulance. Emergency Departments are available for urgent medical care."),
                new("link", "Emergency medical help – Health New Zealand||https://www.healthnz.govt.nz/health-topics/tests-and-treatments/emergencies-and-first-aid/emergency-medical-help"),
                new("heading", "Understanding Healthcare Costs"),
                new("paragraph", "Healthcare costs depend on your eligibility, the type of service, the healthcare provider and whether you have insurance. International students should not assume that healthcare will be free and should check their eligibility and insurance policy before receiving non-emergency treatment."),
                new("link", "Healthcare and eligibility – Immigration New Zealand||https://www.immigration.govt.nz/live/setting-up-your-life-in-new-zealand/getting-health-care-and-finding-a-doctor/"),
                new("heading", "Healthcare for International Students"),
                new("paragraph", "Many international students are required to have medical and travel insurance as a condition of their student visa. Fee-paying student visa holders must meet the applicable insurance requirements for their visa and education provider."),
                new("link", "Fee Paying Student Visa – Immigration New Zealand||https://www.immigration.govt.nz/visas/fee-paying-student-visa/"),
                new("heading", "Medical Insurance"),
                new("subheading", "Understanding Your Insurance Policy"),
                new("paragraph", "Before using healthcare services, understand your policy's benefits, exclusions, excesses, claim requirements and approved-provider conditions. Your education provider or insurance company can explain the specific policy you have."),
                new("link", "Insurance for international learners – NZQA||https://www2.nzqa.govt.nz/tertiary/the-code/the-code-for-education-providers/code-resources-for-school-signatories/insurance-for-international-learners/"),
                new("subheading", "Making an Insurance Claim"),
                new("paragraph", "Follow the claim process provided by your insurance company. Keep invoices, receipts, medical reports, prescriptions and other relevant documents because these may be required when making a claim."),
                new("subheading", "Keeping Medical Documents"),
                new("paragraph", "Keep copies of important medical documents, including prescriptions, invoices, receipts, referral letters, test results and insurance claim information. Store them securely so they can be accessed when needed."),
                new("heading", "GP and Medical Centres"),
                new("subheading", "Finding a GP"),
                new("paragraph", "You can contact a medical centre to ask whether it is accepting new patients. If you enrol with a GP practice, you may receive subsidised healthcare where you are eligible."),
                new("link", "Find healthcare services – Healthpoint||https://www.healthpoint.co.nz/"),
                new("subheading", "After-Hours Healthcare"),
                new("paragraph", "If you need medical attention outside your GP's normal hours and the problem cannot wait, you can use an after-hours or urgent medical centre. If you are unsure what to do, call Healthline on 0800 611 116 for free health advice."),
                new("link", "Healthline – Health New Zealand||https://www.healthnz.govt.nz/online-phone-healthcare/healthline"),
                new("heading", "Pharmacy and Medication"),
                new("subheading", "Understanding Medication Instructions"),
                new("paragraph", "Always follow the instructions provided by your healthcare professional and pharmacist. Ask the pharmacist if you are unsure about the dosage, timing, side effects or interactions with other medicines."),
                new("heading", "Mental Wellbeing"),
                new("subheading", "Homesickness and Study Stress"),
                new("paragraph", "Moving to another country can involve changes in study, work, relationships, culture and daily life. Maintaining social connections, getting enough rest, staying active and seeking help when needed can support mental wellbeing."),
                new("subheading", "1737 Mental Health Support"),
                new("paragraph", "If you need to talk to someone about mental health or emotional distress, call or text 1737 to access free support."),
                new("link", "Mental health support – Health New Zealand||https://www.healthnz.govt.nz/"),
            ]),

            new("Childcare",
            [
                new("heading", "Understanding Childcare in New Zealand"),
                new("subheading", "What Is Early Childhood Education?"),
                new("paragraph", "Early Childhood Education (ECE) provides education and care for young children before they start school. New Zealand has different types of early learning services, including kindergartens, preschools, childcare centres and other licensed services."),
                new("link", "Early childhood education – Ministry of Education||https://www.education.govt.nz/our-work/about-us/education-new-zealand/our-education-system/early-childhood-education"),
                new("subheading", "Types of Childcare"),
                new("paragraph", "Families can choose from different early learning options depending on their child's age, location, availability, hours and family needs. Services can have different operating hours, programmes and fees."),
                new("subheading", "Finding a Childcare Centre"),
                new("paragraph", "Parents should research childcare services in their local area, compare services and contact centres directly about availability, fees and enrolment requirements."),
                new("link", "Enrolling and starting early learning||https://www.education.govt.nz/parents-and-caregivers/early-learning/starting-early-learning/enrolling-and-starting-your-child-early-learning"),
                new("heading", "Childcare Enrolment"),
                new("subheading", "How to Enrol Your Child"),
                new("paragraph", "Parents normally contact the early learning service directly and complete its enrolment process. Services may have their own requirements, fees, minimum hours or waiting lists."),
                new("subheading", "Documents You May Need"),
                new("paragraph", "A childcare service may ask for information such as the child's identity documents, passport or birth certificate, as well as medical information such as allergies, medication and medical conditions."),
                new("subheading", "Waiting Lists"),
                new("paragraph", "Popular childcare centres may not have immediate vacancies. Families may need to join a waiting list, so it is useful to contact centres before childcare is required."),
                new("heading", "Childcare Costs and Financial Support"),
                new("subheading", "Understanding Childcare Fees"),
                new("paragraph", "Childcare fees vary between services. Ask about enrolment fees, weekly fees, additional charges, minimum hours and what is included before enrolling."),
                new("subheading", "20 Hours ECE"),
                new("paragraph", "Eligible children aged 3, 4 and 5 may be able to receive up to 20 hours of funded early childhood education each week through participating services. Families should confirm the service's conditions and any charges for additional hours."),
                new("subheading", "Childcare Subsidy"),
                new("paragraph", "The Childcare Subsidy helps eligible families with the cost of approved preschool childcare. Eligibility depends on factors including the caregiver's circumstances, income, residency status and the child's age and childcare arrangements."),
                new("link", "Childcare assistance – Work and Income||https://www.workandincome.govt.nz/"),
                new("subheading", "OSCAR Subsidy"),
                new("paragraph", "OSCAR support may help eligible families with before-school, after-school and school-holiday care. Eligibility and payment amounts depend on individual circumstances."),
                new("subheading", "FamilyBoost"),
                new("paragraph", "FamilyBoost is a government payment that can help eligible families with licensed early childhood education costs. Families should check the current eligibility requirements before applying."),
                new("link", "Inland Revenue||https://www.ird.govt.nz/"),
                new("heading", "Child Health and Family Support"),
                new("subheading", "Plunket"),
                new("paragraph", "Plunket provides information and support for parents and caregivers, particularly around babies, young children, parenting and child development."),
                new("link", "Plunket New Zealand||https://www.plunket.org.nz/"),
                new("subheading", "Child Health Information"),
                new("paragraph", "KidsHealth provides New Zealand-focused information about children's health, illnesses, medicines, development and wellbeing."),
                new("link", "KidsHealth New Zealand||https://www.kidshealth.org.nz/"),
            ]),

            new("Emergency",
            [
                new("heading", "Emergency Services"),
                new("subheading", "Emergency Number – 111"),
                new("paragraph", "111 is New Zealand's emergency telephone number. Call 111 when someone needs urgent police, fire or ambulance assistance. For a medical emergency, ask for an ambulance."),
                new("link", "111 emergency help – Health New Zealand||https://www.healthnz.govt.nz/online-phone-healthcare/online-gp-care/i-need-111-emergency-help"),
                new("subheading", "When to Call 111"),
                new("paragraph", "Call 111 for situations such as severe difficulty breathing, chest pain or tightness, unconsciousness, severe bleeding, choking, stroke symptoms, severe allergic reaction, severe burns, severe pain, or a suicidal or mental health crisis."),
                new("subheading", "What Information to Provide"),
                new("paragraph", "Clearly provide your location, what happened, the person's condition, whether they are conscious and breathing, any serious symptoms, and your contact details if requested. Follow the emergency operator's instructions and do not hang up until you are told to do so."),
                new("heading", "Healthcare During an Emergency"),
                new("subheading", "Emergency Department"),
                new("paragraph", "Emergency Departments assess and treat urgent medical conditions and injuries. Patients are prioritised according to how serious their condition is, so the person who arrives first may not necessarily be treated first."),
                new("subheading", "Urgent Care"),
                new("paragraph", "Urgent care is appropriate for medical problems that need attention quickly but are not life-threatening. If you cannot wait until your GP is available, an after-hours or urgent medical centre may be appropriate."),
                new("subheading", "Healthline – When You Are Unsure"),
                new("paragraph", "If you are unsure whether you need a doctor, urgent care or another health service, you can call Healthline on 0800 611 116 for free health advice. Healthline operates 24 hours a day, 7 days a week."),
                new("link", "Healthline – Health New Zealand||https://www.healthnz.govt.nz/online-phone-healthcare/healthline"),
                new("subheading", "Ambulance Costs and Insurance"),
                new("paragraph", "Ambulance services and healthcare costs can depend on the circumstances and eligibility. International students should understand how emergency and ambulance costs are handled under their insurance policy."),
                new("subheading", "Mental Health Emergency"),
                new("paragraph", "A mental health crisis can be an emergency. If someone is in immediate danger or is experiencing a serious mental health crisis, call 111 or go to an Emergency Department. For non-immediate mental health support, call or text 1737."),
            ]),

            new("Employment",
            [
                new("heading", "Starting Employment"),
                new("paragraph", "Starting a new job is an exciting and challenging time. Employers need to exercise good practices in their hiring procedures and all parties need to adhere to minimum rights and responsibilities."),
                new("heading", "Employee Rights and Responsibilities"),
                new("subheading", "What Are Employee Rights?"),
                new("paragraph", "Employee rights are minimum rights mandated by law. They cover employment agreements, pay and wages, breaks, holidays and leave, safe and fair workplaces, flexible working, union membership, redundancy or restructuring, employment records and resigning."),
                new("subheading", "Employee Responsibilities"),
                new("paragraph", "Responsibilities at work include attendance, responding to lawful and reasonable requests, staying safe and helping others stay safe, using appropriate knowledge and judgement, behaving responsibly and in good faith, and protecting information."),
                new("subheading", "Employment Agreements"),
                new("paragraph", "An employer must provide a written employment agreement and keep it up to date. Employees should have an opportunity to get advice or support from someone they trust before signing."),
                new("subheading", "Pay and Wages"),
                new("paragraph", "Employers must meet applicable minimum-wage requirements, pay wages lawfully, and make deductions only where legally permitted. Employees should check their agreement and official guidance for their circumstances."),
                new("subheading", "Breaks, Holidays and Leave"),
                new("paragraph", "Employment law sets requirements for rest and meal breaks, annual holidays, sick leave, bereavement leave, family violence leave and public holidays. Entitlements depend on the applicable legal requirements and employment circumstances."),
                new("subheading", "Safe and Fair Workplace"),
                new("paragraph", "Employees are entitled to a safe and healthy workplace, appropriate training and equipment, and protection from bullying, harassment and unlawful discrimination."),
                new("heading", "Writing a CV"),
                new("subheading", "What Is a CV?"),
                new("paragraph", "A CV, sometimes called a resume, is a written document that shows your education, training, achievements, skills and work experience."),
                new("subheading", "Make Your CV Match the Job"),
                new("paragraph", "Update your CV for each job application using relevant information and keywords from the job advertisement. Highlight the skills and experience that match the role."),
                new("link", "CV tools and resources – Tahatū||https://tahatu.govt.nz/work/applying-for-a-job/how-to-write-a-cv"),
                new("heading", "Tips for Interviews"),
                new("subheading", "Prepare for Your Interview"),
                new("paragraph", "Research the organisation, prepare anything you need to bring, plan suitable clothing and your journey, and check whether the interview includes tests, presentations or group activities."),
                new("subheading", "Answering Interview Questions"),
                new("paragraph", "Review the job description, practise explaining your skills and experience, speak clearly, ask for clarification when needed, and provide concise examples of how you have applied your skills."),
            ]),

            new("Family Support",
            [
                new("heading", "Financial Help for Your Family"),
                new("subheading", "Financial Help When Raising a Family"),
                new("paragraph", "Work and Income provides an online tool to help people explore benefits they might be entitled to. The tool may ask about personal circumstances, earnings, health conditions, housing costs and assets. Check the official service for current eligibility and privacy information."),
                new("link", "Work and Income||https://www.workandincome.govt.nz/"),
                new("subheading", "Financial Help When Caring for Someone Else's Child"),
                new("paragraph", "Unsupported Child's Benefit is a weekly payment for eligible people over 18 caring for a child who is not their natural, adopted or step-child, is financially dependent on them, and cannot be cared for by their parents because of a family breakdown."),
                new("link", "Unsupported Child's Benefit||https://www.workandincome.govt.nz/products/a-z-benefits/unsupported-childs-benefit"),
                new("subheading", "Orphan's Benefit"),
                new("paragraph", "Orphan's Benefit is a weekly payment for eligible people over 18 caring for a child whose parents have died, cannot be found, or cannot care for them because of a long-term illness."),
                new("link", "Orphan's Benefit||https://www.workandincome.govt.nz/products/a-z-benefits/orphans-benefit"),
                new("subheading", "School and Year Start-up Payment"),
                new("paragraph", "People receiving Unsupported Child's Benefit or Orphan's Benefit may be able to get help with preschool and school-related costs, such as clothing, school fees and stationery."),
                new("link", "School and Year Start-up Payment||https://www.workandincome.govt.nz/products/a-z-benefits/school-and-year-start-up-payment"),
                new("subheading", "Away From Home Allowance"),
                new("paragraph", "Away From Home Allowance is a weekly payment that may help eligible carers with the living costs of 16- or 17-year-olds who live away from home while attending a tertiary or training course."),
                new("link", "Away From Home Allowance||https://www.workandincome.govt.nz/products/a-z-benefits/away-from-home-allowance"),
                new("heading", "Working for Families"),
                new("subheading", "Who May Be Eligible?"),
                new("paragraph", "Working for Families tax credits help with the costs of raising children. Eligibility and payment amounts depend on factors such as the child's circumstances, family income, work situation and tax-residency requirements. Check Inland Revenue's current rules before applying."),
                new("link", "Working for Families eligibility||https://www.ird.govt.nz/working-for-families/can-you-get-it"),
                new("subheading", "How Payments Are Calculated"),
                new("paragraph", "Payment amounts depend on factors including the number of dependent children, total family income, hours worked by caregivers and sources of income."),
                new("subheading", "Choosing When to Get Paid"),
                new("paragraph", "Depending on the payment and your circumstances, payments may be available weekly, fortnightly or as a lump sum after the tax year. Consider whether your income and family circumstances are likely to change, and check current Inland Revenue guidance."),
                new("subheading", "What to Do if Something Changes"),
                new("paragraph", "Changes to family circumstances or income can affect entitlement. Notify Inland Revenue promptly when relevant details change."),
            ]),

            new("Education",
            [
                new("heading", "Eligibility to Enrol in New Zealand Schools"),
                new("subheading", "Who Can Enrol at a New Zealand School"),
                new("paragraph", "Children aged 5 to 19 may attend state and state-integrated schools without paying tuition when they meet the requirements for domestic student status. Domestic status can depend on citizenship, residence and visa circumstances."),
                new("subheading", "Enrolling Students From Overseas"),
                new("paragraph", "A student enrolling in a New Zealand school must be eligible as either a domestic student or an international student. The material supplied also notes that schools may host student visitors for up to two weeks without enrolment."),
                new("subheading", "Temporary Domestic Student Status"),
                new("paragraph", "The supplied material describes eligibility conditions for temporary domestic student status, including age and residence-related conditions. These rules can be specific to a child's circumstances, so families should confirm current requirements with the Ministry of Education before applying."),
                new("subheading", "Domestic and International Students"),
                new("paragraph", "A child's student status may depend on citizenship, residence and visa conditions. A school may ask for evidence of the child's visa status."),
                new("heading", "Three Levels of the Education System"),
                new("subheading", "Early Childhood Education"),
                new("paragraph", "Early childhood education is for children from birth until school entry age and is not compulsory."),
                new("subheading", "Primary and Secondary Education"),
                new("paragraph", "Primary and secondary education generally covers ages 5 to 19. Schooling is compulsory from age 6 to 16."),
                new("subheading", "Tertiary Education and Training"),
                new("paragraph", "The tertiary sector includes private training establishments, institutes of technology and polytechnics, wānanga, universities and workplace training. Options can include vocational training, apprenticeships, on-the-job training and adult and community education."),
                new("link", "Tertiary education – Ministry of Education||https://www.education.govt.nz/our-work/about-us/education-new-zealand/our-education-system/tertiary-education"),
                new("link", "Explore study and training – Tahatū||https://tahatu.govt.nz/study-and-training/explore-study-and-training"),
                new("heading", "School Hours, Terms and Holidays"),
                new("paragraph", "The supplied material describes a typical school day as starting around 9:00 am and finishing by 3:30 pm, with four terms in the school year. Dates and hours vary by school, so check directly with the school."),
                new("heading", "Before and After School Care"),
                new("paragraph", "Before-school, after-school and holiday programmes may be available through private or community providers. Fees and hours vary. Children under 14 must not be left without reasonable care and supervision."),
                new("heading", "Choosing a School"),
                new("subheading", "School Zones and Enrolment Schemes"),
                new("paragraph", "State schools may use enrolment schemes or home zones. Children who live in a school's zone may have priority for enrolment. Families should check the school's current enrolment scheme and requirements."),
                new("link", "School zones and enrolment schemes – Ministry of Education||https://www.education.govt.nz/parents-and-caregivers/schools-year-0-13/starting-school/school-zones-and-enrolment-schemes"),
            ])
        ];
    }
}
