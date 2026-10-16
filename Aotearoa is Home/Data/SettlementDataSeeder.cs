using System.IO;

using Aotearoa_is_Home.Models;

using Microsoft.EntityFrameworkCore;

namespace Aotearoa_is_Home.Data

{

public static class SettlementDataSeeder

    {

private sealed record SeedBlock(

            string Type,

            string Content,

            string Details = "");

private sealed record SeedPage(

            string CategoryName,

            IReadOnlyList<SeedBlock> Blocks);

private sealed record SeedTask(

            string Title,

            string Description);

public static async Task SeedAsync(

            ApplicationDbContext context,

            string webRootPath)

        {

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

                    .FirstOrDefaultAsync(

                        p => p.CategoryName == seedPage.CategoryName);

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

                if (!hasBlocks && seedPage.Blocks.Count > 0)

                {

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

                await SeedChecklistTasksAsync(context, page);

            }

        }

private static async Task SeedChecklistTasksAsync(

            ApplicationDbContext context,

            SettlementPage page)

        {

            var hasTasks = await context.ChecklistTasks

                .AnyAsync(t => t.SettlementPageId == page.Id);

            if (hasTasks)

                return;

            var tasks = GetChecklistTasks(page.CategoryName);

            if (tasks.Count == 0)

                return;

            var checklistTasks = tasks

                .Select((task, index) => new ChecklistTask

                {

                    SettlementPageId = page.Id,

                    Title = task.Title,

                    Description = task.Description,

                    DisplayOrder = index + 1

                })

                .ToList();

            context.ChecklistTasks.AddRange(checklistTasks);

            page.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

        }

private static IReadOnlyList<SeedTask> GetChecklistTasks(

            string categoryName)

        {

            return categoryName switch

            {

                "Accomodation" =>

                [

                    new("Research accommodation options",

                        "Compare rental properties, locations and prices."),

                    new("Understand tenancy requirements",

                        "Learn about tenancy agreements, bond payments and your responsibilities as a tenant."),

                    new("Prepare identification documents",

                        "Gather identification and any documents landlords may request."),

                    new("Arrange a place to stay",

                        "Confirm your accommodation and move-in arrangements."),

                    new("Set up utilities and internet",

                        "Check electricity, water, internet and other services required at your accommodation."),

                    new("Record important tenancy information",

                        "Keep a copy of your tenancy agreement, payment records and landlord contact details.")

                ],

                "Community" =>

                [

                    new("Explore your local area",

                        "Find nearby shops, libraries, parks, public facilities and community centres."),

                    new("Find local community groups",

                        "Research community groups and activities that match your interests."),

                    new("Learn how to use public transport",

                        "Find local bus, train or other public transport options and check how to pay for fares."),

                    new("Find useful local services",

                        "Identify nearby supermarkets, pharmacies, libraries and other services you may need."),

                    new("Connect with your community",

                        "Attend a community activity, event or group to meet people and build connections.")

                ],

                "Language" =>

                [

                    new("Assess your English language needs",

                        "Identify the English language skills you want to improve."),

                    new("Find English language learning resources",

                        "Research classes, online resources and language support services."),

                    new("Practise everyday English",

                        "Practise conversations used in shops, workplaces, healthcare services and everyday situations."),

                    new("Learn useful New Zealand expressions",

                        "Become familiar with common local expressions and communication styles."),

                    new("Explore English language support",

                        "Find out whether local organisations or education providers offer language assistance.")

                ],

                "Healthcare" =>

                [

                    new("Check your healthcare eligibility",

                        "Confirm which publicly funded healthcare services you may be eligible to access."),

                    new("Review your medical insurance",

                        "Understand your insurance coverage, exclusions, excesses and claim requirements."),

                    new("Find a local GP or medical centre",

                        "Research nearby general practices and check whether they are accepting new patients."),

                    new("Save important healthcare contacts",

                        "Keep the contact details for your GP, pharmacy and Healthline accessible."),

                    new("Understand prescription medication",

                        "Learn how to obtain prescriptions and ask a pharmacist about medication instructions."),

                    new("Learn how to access mental health support",

                        "Find available wellbeing services and remember that you can call or text 1737 for support.")

                ],

                "Childcare" =>

                [

                    new("Research childcare options",

                        "Compare early childhood education services and childcare options in your area."),

                    new("Check childcare availability",

                        "Contact services to ask about vacancies, waiting lists and enrolment timelines."),

                    new("Compare childcare fees",

                        "Ask about fees, minimum hours, additional charges and available financial assistance."),

                    new("Prepare enrolment documents",

                        "Gather the identity and other information required by your chosen childcare service."),

                    new("Check available childcare assistance",

                        "Research current eligibility requirements for relevant childcare subsidies and support."),

                    new("Confirm childcare arrangements",

                        "Confirm the start date, hours, contact details and drop-off and pick-up arrangements.")

                ],

                "Emergency" =>

                [

                    new("Learn New Zealand's emergency number",

                        "Call 111 for urgent police, fire or ambulance assistance."),

                    new("Save important emergency contacts",

                        "Keep emergency contacts and relevant support numbers accessible."),

                    new("Learn when to call 111",

                        "Understand the difference between an emergency and a non-urgent situation."),

                    new("Know where to find urgent healthcare",

                        "Identify your nearest emergency department and suitable urgent care services."),

                    new("Prepare an emergency plan",

                        "Consider how you would contact family, leave an unsafe location and get help during an emergency."),

                    new("Keep important documents accessible",

                        "Store essential identification, insurance information and emergency contact details securely.")

                ],

                "Employment" =>

                [

                    new("Prepare or update your CV",

                        "Include your relevant skills, education, experience and contact details."),

                    new("Check your work eligibility",

                        "Confirm the work conditions and restrictions that apply to your visa or employment situation."),

                    new("Research job opportunities",

                        "Find vacancies that match your skills, experience and availability."),

                    new("Prepare for job interviews",

                        "Practise answering common interview questions and prepare examples of your experience."),

                    new("Understand your employment agreement",

                        "Review your pay, hours, duties, leave entitlements and other employment conditions."),

                    new("Learn your employment rights",

                        "Understand minimum employment rights, workplace safety and how to seek help if needed.")

                ],

                "Family Support" =>

                [

                    new("Identify your family's support needs",

                        "Consider your family's financial, childcare, education, housing and wellbeing needs."),

                    new("Research family support services",

                        "Find official organisations that provide relevant information and assistance."),

                    new("Check eligibility for financial assistance",

                        "Review current eligibility requirements for any benefits, subsidies or family support payments."),

                    new("Find parenting and child wellbeing resources",

                        "Explore reliable services and resources that support children's health and development."),

                    new("Save useful family support contacts",

                        "Keep the contact details of relevant support organisations and service providers."),

                    new("Contact a relevant support service",

                        "Ask a suitable organisation about available assistance and the application process.")

                ],

                "Education" =>

                [

                    new("Research education options",

                        "Compare schools, tertiary programmes, training providers or other learning opportunities."),

                    new("Check enrolment eligibility",

                        "Confirm the relevant entry requirements and whether your circumstances affect eligibility."),

                    new("Prepare enrolment documents",

                        "Gather identification, academic records and other documents required by the education provider."),

                    new("Understand education costs",

                        "Check tuition fees, learning materials, transport costs and any available financial support."),

                    new("Confirm enrolment dates and deadlines",

                        "Check application deadlines, term dates, orientation dates and other important dates."),

                    new("Find student support services",

                        "Research academic support, wellbeing services, student advisers and other available assistance.")

                ],

_ => Array.Empty<SeedTask>()

            };

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
                new("heading", "Finding Accommodation in New Zealand"),
                new("subheading", "Choosing Where to Live"),
                new("paragraph", "Before choosing a place to live, consider your budget, travel time to campus or work, access to public transport, nearby shops and services, and whether the property suits your household. Options may include a room in a shared house, a flat, a rental property or temporary accommodation while you look for a longer-term home."),
                new("subheading", "Where to Look"),
                new("paragraph", "Use established rental websites, property managers, university or tertiary-provider accommodation services, and trusted community networks. View a property or arrange a reliable viewing before paying money. Be cautious of listings that pressure you to pay immediately, refuse reasonable checks, or offer a price that seems unusually low."),
                new("heading", "Renting a Property"),
                new("subheading", "Tenancy Agreements"),
                new("paragraph", "Read the tenancy agreement before signing and make sure you understand the rent, payment frequency, people named on the agreement, start date, notice requirements and responsibilities for utilities and property care. Keep a signed copy and ask for clarification before agreeing to terms you do not understand."),
                new("subheading", "Bond and Rent in Advance"),
                new("paragraph", "A general tenancy bond can be up to four weeks’ rent, and a landlord cannot ask for more than two weeks’ rent in advance. If you pay the bond to the landlord, they must lodge it with Tenancy Services within 23 working days. Ask for a receipt and keep records of payments. Check the official guidance because different rules can apply in some situations."),
                new("link", "Tenancy Services – Information for new tenants||https://www.tenancy.govt.nz/starting-a-tenancy/new-to-tenancy/information-for-new-tenants/"),
                new("subheading", "Inspecting the Property"),
                new("paragraph", "Before moving in, record the condition of the property, take dated photographs where appropriate, and report existing damage in writing. Check locks, smoke alarms, heating, ventilation, water, appliances and any agreed repairs. Keep copies of messages with the landlord or property manager."),
                new("heading", "Bills, Utilities and Staying Safe"),
                new("subheading", "Electricity, Internet and Other Bills"),
                new("paragraph", "Ask which services are included in the rent and which you must arrange or pay for separately. Compare electricity, internet and mobile plans, and clarify how shared household bills will be divided. Keep a simple record of due dates and payments."),
                new("subheading", "Flatting and Shared Accommodation"),
                new("paragraph", "Agree with housemates how rent, bills, cleaning, shared items, guests and household responsibilities will be managed. Understand whether you are a tenant, a named tenant on a joint agreement, or a flatmate, because the rights and responsibilities can differ."),
                new("subheading", "Getting Help"),
                new("paragraph", "If you have questions about tenancy rights, bond, repairs, rent increases or ending a tenancy, check official Tenancy Services guidance or seek appropriate advice. Aotearoa Is Home provides general information and does not replace legal advice."),
                new("link", "Tenancy Services||https://www.tenancy.govt.nz/")
            ]),

            new("Community",
            [
                new("heading", "Connecting with Your Community"),
                new("subheading", "Why Community Connections Matter"),
                new("paragraph", "Moving to a new country can feel unfamiliar, especially when you are separated from family, friends and familiar routines. Building social connections can help you learn about local life, share experiences, practise communication and develop a sense of belonging."),
                new("heading", "Campus and Student Life"),
                new("subheading", "Whitireia and WelTec Activities"),
                new("paragraph", "Check your campus communications and student support channels for orientation activities, workshops, clubs, cultural celebrations, wellbeing sessions and other events. Event details can change, so confirm the date, venue, registration requirements and any costs with the organiser."),
                new("subheading", "Meet Other Students"),
                new("paragraph", "Join a student club, attend a campus event, take part in a group activity or invite classmates to study together. You do not need to attend every event; start with activities that match your interests and comfort level."),
                new("heading", "Local Community Resources"),
                new("subheading", "Libraries and Community Centres"),
                new("paragraph", "Public libraries and community centres can offer study spaces, community information, activities, workshops and opportunities to meet people. Check the local council or library website for current opening hours, programmes and membership information."),
                new("subheading", "Volunteering and Interest Groups"),
                new("paragraph", "Volunteering, sports, arts, faith-based groups where relevant, hobby groups and neighbourhood activities can help you meet people with shared interests. Check the organiser's expectations, costs, location and safety arrangements before joining."),
                new("heading", "Understanding Aotearoa New Zealand"),
                new("subheading", "Respecting Māori Culture"),
                new("paragraph", "Māori are tangata whenua, the Indigenous people of Aotearoa New Zealand. You may encounter te reo Māori, tikanga (customs and practices), and concepts such as manaakitanga (care and hospitality) in community and educational settings. Be open to learning, listen respectfully, and follow the guidance of hosts or organisers."),
                new("subheading", "Staying Connected and Safe"),
                new("paragraph", "When meeting new people, choose public places for first meetings, tell someone you trust where you are going when appropriate, and protect your personal and financial information. You can take your time when deciding which groups and relationships feel right for you."),
                new("heading", "Finding Events and Services"),
                new("paragraph", "Use official campus channels, local council websites, libraries and established community organisations to find current events and services. Aotearoa Is Home may display community and campus opportunities, but users should confirm details with the event organiser."),
                new("link", "Wellington City Council||https://wellington.govt.nz/"),
                new("link", "Hutt City Council||https://www.huttcity.govt.nz/")
            ]),

            new("Language",
            [
                new("heading", "Language Support in New Zealand"),
                new("subheading", "Building Confidence in Everyday English"),
                new("paragraph", "English is widely used in education, workplaces and public services in New Zealand. You do not need to speak perfectly to begin communicating. Practising little and often can help you feel more confident when asking questions, making appointments, travelling and meeting people."),
                new("heading", "Everyday Communication"),
                new("subheading", "Useful Situations to Practise"),
                new("paragraph", "Practise introducing yourself, asking for directions, explaining a problem, booking an appointment, speaking to a landlord, using public transport and asking for help at a shop or service desk. If you do not understand something, it is acceptable to ask the person to repeat or explain it more slowly."),
                new("subheading", "New Zealand Expressions"),
                new("paragraph", "You may hear informal expressions such as 'Kia ora' (a greeting), 'cheers' (often used to say thanks), and 'sweet as' (an informal way of saying something is good or fine). Meaning depends on context, and you can ask someone politely if an expression is unfamiliar."),
                new("heading", "Learning and Practising English"),
                new("subheading", "Classes and Learning Resources"),
                new("paragraph", "Look for English classes, conversation groups, library learning resources and reputable online lessons. Before enrolling, check the course level, fees, timetable, location and whether it is suitable for your goals. Your education provider may also be able to direct you to student learning support."),
                new("subheading", "Practising with Others"),
                new("paragraph", "Conversation groups, classmates, community activities and volunteering can provide opportunities to practise. Set a manageable goal, such as learning a few useful words each week or practising one everyday conversation."),
                new("heading", "Language Assistance"),
                new("subheading", "When You Need Help Understanding Information"),
                new("paragraph", "For important conversations about health, legal matters, tenancy or official services, ask the organisation whether interpreting or translated information is available. Do not rely on an informal translation alone when a misunderstanding could have serious consequences."),
                new("heading", "Te Reo Māori and Cultural Respect"),
                new("paragraph", "Te reo Māori is an official language of New Zealand. Learning common greetings and place names can help you engage respectfully with the local context. Follow reliable learning resources and respect the pronunciation and cultural context of words."),
                new("link", "Te Taura Whiri i te Reo Māori – Māori Language Commission||https://www.tetaurawhiri.govt.nz/"),
                new("link", "English Language Partners New Zealand||https://www.englishlanguage.org.nz/")
            ]),

            new("Healthcare",

            [

                new("heading", "Understanding NZ Healthcare"),

                new("subheading", "How the New Zealand Healthcare System Works"),

                new("paragraph", "New Zealand has a combination of publicly funded and privately provided healthcare services. The level of government funding and eligibility depends on a person's citizenship, residency or visa status. International students should check whether they are eligible for publicly funded healthcare and understand what costs they may need to pay themselves."),

                new("link", "Immigration New Zealand||https://www.immigration.govt.nz/live/setting-up-your-life-in-new-zealand/getting-health-care-and-finding-a-doctor/"),

                new("heading", "Public Healthcare"),

                new("paragraph", "Public healthcare services are funded or subsidised by the New Zealand Government for people who meet eligibility requirements. People who are not eligible can still use public healthcare services but will generally need to pay for the services they receive."),

                new("link", "Who can get public healthcare?||https://www.immigration.govt.nz/process-to-apply/applying-for-a-visa/providing-evidence-and-documents-to-support-your-visa-application/health-requirements/who-can-get-public-health-care/"),

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

                new("link", "Insurance for international learners – NZQA||https://www2.nzqa.govt.nz/tertiary/the-code/the-code-for-education-providers/code-resources-for-tertiary-providers/insurance-for-international-learners/"),

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

                new("link", "Mental health support – Health New Zealand||https://www.healthnz.govt.nz/")

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

                new("paragraph", "Children aged 3, 4 and 5 are eligible for 20 Hours ECE at participating services, for up to 20 hours each week and no more than 6 hours a day. A service cannot charge fees for the hours a child receives as 20 Hours ECE, although fees or optional charges may apply to additional hours or other items. Confirm the arrangement directly with the service."),

                new("subheading", "Childcare Subsidy"),

                new("paragraph", "The Childcare Subsidy helps eligible families with the cost of approved preschool childcare. Current eligibility rules include requirements about the principal caregiver, income, the child and the caregiver’s immigration or residence status; Work and Income says the caregiver must be a New Zealand citizen or permanent resident. Many temporary-visa holders may not qualify, so check the current rules directly before relying on this assistance."),

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

                new("link", "KidsHealth New Zealand||https://www.kidshealth.org.nz/")

            ]),

            new("Emergency",

            [

                new("heading", "Emergency Services"),

                new("subheading", "Emergency Number – 111"),

                new("paragraph", "111 is New Zealand's emergency telephone number. Call 111 when someone needs urgent police, fire or ambulance assistance. For a medical emergency, ask for an ambulance."),

                new("link", "111 emergency help – Health New Zealand||https://www.healthnz.govt.nz/online-phone-healthcare/online-gp-care/i-need-111-emergency-help"),

                new("subheading", "Police Non-Emergencies – 105"),

                new("paragraph", "Use 105 to report a Police matter that does not require immediate attendance, such as a theft discovered after the event or property damage when there is no immediate danger. If a person is in danger, a serious incident is happening now, or there is an immediate risk to life or property, call 111."),

                new("link", "New Zealand Police – 105 non-emergency service||https://www.police.govt.nz/105support"),

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

                new("paragraph", "A mental health crisis can be an emergency. If someone is in immediate danger or is experiencing a serious mental health crisis, call 111 or go to an Emergency Department. For non-immediate mental health support, call or text 1737.")

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

                new("paragraph", "Before accepting work, check the conditions on your own visa. Work rights vary by visa type and individual conditions; do not rely on another student’s work conditions or on a general weekly-hours figure."),

                new("link", "Check or change visa conditions – Immigration New Zealand||https://www.immigration.govt.nz/process-to-apply/once-you-have-a-visa/manage-your-visa-and-passport/check-or-change-your-visa-conditions/"),

                new("heading", "Tips for Interviews"),

                new("subheading", "Prepare for Your Interview"),

                new("paragraph", "Research the organisation, prepare anything you need to bring, plan suitable clothing and your journey, and check whether the interview includes tests, presentations or group activities."),

                new("subheading", "Answering Interview Questions"),

                new("paragraph", "Review the job description, practise explaining your skills and experience, speak clearly, ask for clarification when needed, and provide concise examples of how you have applied your skills.")

            ]),

            new("Family Support",

            [

                new("heading", "Financial Help for Your Family"),

                new("paragraph", "Important: financial assistance is not available to every family living in New Zealand. Many benefits and tax credits have citizenship, residence, tax-residency, income or other eligibility requirements. International students and family members on temporary visas may not qualify for some payments. Confirm eligibility directly with Work and Income or Inland Revenue before relying on financial assistance."),

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

                new("paragraph", "Working for Families tax credits help eligible families with the costs of raising children. Eligibility includes specific residence and tax-residency rules for the caregiver or child, as well as other conditions. People who hold only a temporary visa are generally not eligible on that basis alone. Check Inland Revenue’s current rules before applying or including these payments in a budget."),

                new("link", "Working for Families eligibility||https://www.ird.govt.nz/working-for-families/can-you-get-it"),

                new("subheading", "How Payments Are Calculated"),

                new("paragraph", "Payment amounts depend on factors including the number of dependent children, total family income, hours worked by caregivers and sources of income."),

                new("subheading", "Choosing When to Get Paid"),

                new("paragraph", "Depending on the payment and your circumstances, payments may be available weekly, fortnightly or as a lump sum after the tax year. Consider whether your income and family circumstances are likely to change, and check current Inland Revenue guidance."),

                new("subheading", "What to Do if Something Changes"),

                new("paragraph", "Changes to family circumstances or income can affect entitlement. Notify Inland Revenue promptly when relevant details change.")

            ]),

            new("Education",

            [

                new("heading", "Eligibility to Enrol in New Zealand Schools"),

                new("subheading", "Who Can Enrol at a New Zealand School"),

                new("paragraph", "Children aged 5 to 19 may attend state and state-integrated schools without paying tuition when they meet the requirements for domestic student status. Domestic status can depend on citizenship, residence and visa circumstances."),

                new("subheading", "Enrolling Students From Overseas"),

                new("paragraph", "A child who does not qualify as a domestic student may be able to enrol as an international student, subject to the school and applicable international-learner requirements. The Ministry of Education states that a visitor visa holder may attend a school as a visitor for up to two weeks at the school’s discretion, without tuition fees or entry on ENROL. This is a limited visitor arrangement, not a substitute for checking enrolment and visa requirements for longer attendance. Contact the school before making arrangements."),

                new("subheading", "Temporary Domestic Student Status"),

                new("paragraph", "Some children who do not appear to meet the usual domestic-student criteria may qualify under specific categories in the current Education (Domestic Students) Notice. The rules depend on the child’s and family’s circumstances. Ask the school and check the Ministry of Education’s current eligibility guidance before assuming that a child qualifies for domestic-student treatment."),

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

                new("link", "School enrolment eligibility – Ministry of Education||https://www.education.govt.nz/our-work/publications/education-circulars/education-circular-202008")

            ])

        ];

    }

}
