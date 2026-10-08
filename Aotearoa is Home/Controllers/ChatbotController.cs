using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace Aotearoa_is_Home.Controllers
{
    // Only Students and Family Members can use the chatbot.
    [Authorize(Roles = "Student,Family Member")]
    public class ChatbotController : Controller
    {
        // Opens the chatbot page.
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // Receives the user's question and returns a controlled FAQ response.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GetResponse(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return Json(new
                {
                    response = "Please enter a question so I can help you."
                });
            }

            // Normalise the user's input.
            // This makes matching more reliable for short questions,
            // punctuation, capital letters, etc.
            var question = Normalize(message);

            string response;

            // =========================================================
            // 1. EMERGENCY & SAFETY
            // =========================================================

            if (question.Contains("emergency number") ||
                question == "emergency" ||
                question.Contains("emergency") ||
                question.Contains("111"))
            {
                response =
                    "Call 111 for Police, Fire or Ambulance when there is an emergency " +
                    "or an immediate risk to life or property.";
            }

            else if (question.Contains("when should i call 111") ||
                     question.Contains("should i call 111"))
            {
                response =
                    "Call 111 when someone is in danger, seriously injured, there is a " +
                    "serious risk to life or property, or an emergency is happening now.";
            }

            else if (question.Contains("police non emergency") ||
                     question.Contains("police non-emergency") ||
                     question == "105" ||
                     question.Contains("105"))
            {
                response =
                    "Call 105 for Police matters that do not need an immediate Police response.";
            }

            else if (question.Contains("not sure if it is an emergency") ||
                     question.Contains("not sure if this is an emergency"))
            {
                response =
                    "If you are worried and unsure whether it is a real emergency, " +
                    "call 111 and explain the situation.";
            }

            else if (question.Contains("medical emergency"))
            {
                response =
                    "Call 111 and ask for an ambulance. For urgent but non-life-threatening " +
                    "health advice, call Healthline on 0800 611 116.";
            }

            else if (question.Contains("feel unsafe") ||
                     question.Contains("unsafe"))
            {
                response =
                    "If you are in immediate danger, call 111. If there is no immediate " +
                    "danger, contact Police on 105 or an appropriate support service.";
            }

            // =========================================================
            // 2. HEALTHCARE
            // =========================================================

            else if (question.Contains("healthline") ||
                     question.Contains("health line"))
            {
                response =
                    "Healthline is a free 24/7 health advice service. " +
                    "Call 0800 611 116. Interpreter support is available.";
            }

            else if (question == "gp" ||
                     question.Contains("find a doctor") ||
                     question.Contains("find doctor") ||
                     question.Contains("see a doctor"))
            {
                response =
                    "Find a local GP (General Practitioner) and ask about enrolment " +
                    "and appointments. You can also call Healthline on 0800 611 116.";
            }

            else if (question.Contains("what is a gp") ||
                     question.Contains("what does gp mean"))
            {
                response =
                    "A GP is a General Practitioner and is usually the first healthcare " +
                    "professional to contact for common health problems and ongoing care.";
            }

            else if (question.Contains("no gp") ||
                     question.Contains("do not have a gp") ||
                     question.Contains("don't have a gp"))
            {
                response =
                    "Call Healthline free on 0800 611 116, 24 hours a day, 7 days a week.";
            }

            else if (question.Contains("medical help today") ||
                     question.Contains("need medical help"))
            {
                response =
                    "For life-threatening emergencies, call 111. If it is urgent but " +
                    "not life-threatening, contact your GP, an urgent medical centre, or Healthline.";
            }

            else if (question.Contains("pharmacy") ||
                     question.Contains("pharmacist"))
            {
                response =
                    "Pharmacists can advise about medicines and some common health problems. " +
                    "Ask your local pharmacy what services it provides.";
            }

            // =========================================================
            // 3. WELLINGTON PUBLIC TRANSPORT
            // =========================================================

            else if (question.Contains("public transport") ||
                     question.Contains("transport in wellington") ||
                     question.Contains("transportation in wellington"))
            {
                response =
                    "Wellington's Metlink network includes buses and trains, with ferry " +
                    "services also available. Check Metlink for routes, timetables, fares and updates.";
            }

            else if (question == "metlink" ||
                     question.Contains("what is metlink"))
            {
                response =
                    "Metlink is the Wellington region's public transport network. " +
                    "Use its website or app to plan trips and check service information.";
            }

            else if (question == "snapper")
            {
                response =
                    "Snapper is a reusable transport card used on many Wellington public transport " +
                    "services. It can be topped up and used for eligible fares and passes.";
            }

            else if (question.Contains("how do i pay for") &&
                     (question.Contains("bus") ||
                      question.Contains("train") ||
                      question.Contains("transport")))
            {
                response =
                    "Metlink accepts contactless EFTPOS, debit and credit cards on applicable " +
                    "services, and Snapper is also used. Check current payment rules before travelling.";
            }

            else if (question.Contains("plan a bus") ||
                     question.Contains("plan a train") ||
                     question.Contains("bus route") ||
                     question.Contains("train route"))
            {
                response =
                    "Use Metlink to search your route, timetable, stops and service alerts before travelling.";
            }

            else if (question.Contains("bank card") &&
                     (question.Contains("transport") ||
                      question.Contains("bus") ||
                      question.Contains("train")))
            {
                response =
                    "Contactless EFTPOS, debit and credit cards are accepted on applicable Metlink " +
                    "services. Check current Metlink information before travelling.";
            }

            else if (question.Contains("transport fare") ||
                     question.Contains("transport fares") ||
                     question.Contains("bus fare") ||
                     question.Contains("train fare"))
            {
                response =
                    "Fares can vary by zones, peak/off-peak times, concessions and payment method. " +
                    "Check current Metlink fares.";
            }

            else if (question.Contains("cash") &&
                     (question.Contains("bus") ||
                      question.Contains("train") ||
                      question.Contains("transport")))
            {
                response =
                    "Cash is being phased out across Metlink services. Check the latest Metlink " +
                    "payment information before travelling.";
            }

            // =========================================================
            // 4. MOBILE PHONE & SIM
            // =========================================================

            else if (question.Contains("new zealand phone number") ||
                     question.Contains("nz phone number") ||
                     question.Contains("phone number"))
            {
                response =
                    "Buy a New Zealand SIM or eSIM from a mobile provider and choose a prepaid " +
                    "or monthly plan.";
            }

            else if (question.Contains("need a new zealand sim") ||
                     question.Contains("need a nz sim") ||
                     question.Contains("sim card"))
            {
                response =
                    "A New Zealand SIM or eSIM is useful for connecting to local mobile networks " +
                    "and getting a New Zealand phone number.";
            }

            else if (question.Contains("prepaid") ||
                     question.Contains("monthly mobile plan"))
            {
                response =
                    "Prepaid is flexible when you are new to New Zealand. A monthly plan may suit " +
                    "regular users. Compare data, calls, contract terms and total cost.";
            }

            else if (question.Contains("mobile plan") ||
                     question.Contains("phone plan"))
            {
                response =
                    "Check coverage, data allowance, call/text allowance, price, contract length, " +
                    "extra charges and early termination conditions.";
            }

            else if (question.Contains("overseas phone") ||
                     question.Contains("foreign phone"))
            {
                response =
                    "Your overseas phone will usually work if it is compatible with New Zealand " +
                    "networks and is not locked to another provider. Check with the provider.";
            }

            else if (question.Contains("where can i buy a sim") ||
                     question.Contains("buy a sim") ||
                     question.Contains("where to buy sim"))
            {
                response =
                    "SIM cards and eSIMs are available from mobile providers and many retail outlets. " +
                    "Compare plans and coverage before choosing one.";
            }

            // =========================================================
            // 5. BANKING & MONEY
            // =========================================================

            else if (question.Contains("open a bank account") ||
                     question.Contains("bank account") ||
                     question.Contains("open bank account"))
            {
                response =
                    "Choose a New Zealand bank and apply. You will usually need identity information " +
                    "and may need proof of address and other documents.";
            }

            else if (question.Contains("documents") &&
                     question.Contains("bank"))
            {
                response =
                    "You may be asked for your legal name and date of birth, photo ID such as a " +
                    "passport or driver's licence, and proof of address.";
            }

            else if (question.Contains("ird number") ||
                     question == "ird")
            {
                response =
                    "An IRD number is your New Zealand tax number. If you work or earn taxable " +
                    "income in New Zealand, you generally need one. Check Inland Revenue for your circumstances.";
            }

            else if (question.Contains("pay for things") ||
                     question.Contains("pay in new zealand") ||
                     question.Contains("payment"))
            {
                response =
                    "Cards and electronic payments are widely used in New Zealand. Keep a small " +
                    "amount of cash for situations where electronic payment is unavailable.";
            }

            else if (question.Contains("bank scam") ||
                     question.Contains("bank scams") ||
                     question.Contains("scam"))
            {
                response =
                    "Never share passwords, PINs or one-time security codes. Be cautious of " +
                    "unexpected messages asking for money or banking details.";
            }

            else if (question.Contains("lost bank card") ||
                     question.Contains("lost my bank card"))
            {
                response =
                    "Contact your bank immediately through its official phone number or app " +
                    "and ask them to block or replace the card.";
            }

            // =========================================================
            // 6. ACCOMMODATION & UTILITIES
            // =========================================================

            else if (question.Contains("what should i do first") ||
                     question.Contains("first when i arrive") ||
                     question.Contains("just arrived"))
            {
                response =
                    "Secure accommodation, set up a local phone, arrange essential utilities, " +
                    "open a bank account, identify healthcare support and learn local transport.";
            }

            else if (question.Contains("find a place to rent") ||
                     question.Contains("find a rental") ||
                     question.Contains("rent a house") ||
                     question.Contains("rental property"))
            {
                response =
                    "Compare rental listings, check the location and costs, understand the tenancy " +
                    "terms, and use Tenancy Services information to understand your rights and responsibilities.";
            }

            else if (question.Contains("rental bond") ||
                     question.Contains("what is a bond") ||
                     question == "bond")
            {
                response =
                    "A bond is a security deposit for a rental property. A landlord can generally " +
                    "charge up to four weeks' rent as bond.";
            }

            else if (question.Contains("electricity") ||
                     question.Contains("set up electricity"))
            {
                response =
                    "Choose an electricity provider, compare plans and arrange connection for your address. " +
                    "Ask your landlord or property manager what is already connected.";
            }

            else if (question.Contains("internet at home") ||
                     question.Contains("home internet") ||
                     question.Contains("wifi"))
            {
                response =
                    "Home internet is useful for study, work and communication. Compare plans available at your address.";
            }

            else if (question.Contains("water") ||
                     question.Contains("water service"))
            {
                response =
                    "Water services depend on your location and property. Check local council or water-service " +
                    "information and ask your landlord or property manager about your responsibilities.";
            }

            // =========================================================
            // 7. WASTE & RECYCLING - WELLINGTON
            // =========================================================

            else if (question.Contains("dispose of rubbish") ||
                     question.Contains("rubbish in wellington") ||
                     question.Contains("throw away rubbish") ||
                     question.Contains("garbage"))
            {
                response =
                    "Waste collection depends on your address and service. Check Wellington City Council's " +
                    "current rubbish and recycling information for your collection day and rules.";
            }

            else if (question.Contains("how do i recycle") ||
                     question.Contains("recycling in wellington") ||
                     question.Contains("recycle in wellington"))
            {
                response =
                    "Follow Wellington City Council's current recycling rules for your service. " +
                    "Items should be clean and prepared correctly.";
            }

            else if (question.Contains("what can i put in recycling") ||
                     question.Contains("what can i recycle"))
            {
                response =
                    "Common accepted items include clean plastic containers marked 1, 2 or 5, glass " +
                    "bottles and jars, tins and cans, and paper/cardboard. Check the Council's latest guide.";
            }

            else if (question.Contains("what should not go in recycling") ||
                     question.Contains("not recyclable") ||
                     question.Contains("cannot recycle"))
            {
                response =
                    "Do not put batteries, hazardous materials, broken glass, food-contaminated items, " +
                    "soft plastics or other non-accepted items into kerbside recycling.";
            }

            else if (question.Contains("not sure") &&
                     (question.Contains("recycl") ||
                      question.Contains("recycling")))
            {
                response =
                    "Check Wellington City Council's waste search or recycling guide rather than guessing.";
            }

            else if (question.Contains("food scraps") ||
                     question.Contains("food waste") ||
                     question.Contains("compost"))
            {
                response =
                    "Options depend on your property and current local service. Check Wellington City Council " +
                    "information for the food-scrap and composting options available to your address.";
            }

            else if (question.Contains("battery") ||
                     question.Contains("batteries"))
            {
                response =
                    "Do not put batteries in kerbside recycling. Use an appropriate battery collection or disposal service.";
            }

            else if (question.Contains("electronic waste") ||
                     question.Contains("e-waste") ||
                     question.Contains("ewaste"))
            {
                response =
                    "Do not put e-waste in ordinary recycling unless the service specifically accepts it. " +
                    "Use an appropriate e-waste recycling or disposal service.";
            }

            // =========================================================
            // 8. SHOPPING FOR NEW ARRIVALS
            // =========================================================

            else if (question.Contains("what should i buy first") ||
                     question.Contains("buy first") ||
                     question.Contains("first things to buy"))
            {
                response =
                    "Start with food, drinking water, toiletries, basic medicines, bedding, towels, basic " +
                    "kitchen items, cleaning supplies, rubbish bags, phone/SIM access and work/study essentials.";
            }

            else if (question.Contains("where can i buy groceries") ||
                     question.Contains("buy groceries") ||
                     question.Contains("food shopping") ||
                     question.Contains("supermarket"))
            {
                response =
                    "Supermarkets are the main place for groceries. Dairies and smaller stores are useful " +
                    "for quick purchases but may cost more.";
            }

            else if (question.Contains("new home") ||
                     question.Contains("household items") ||
                     question.Contains("new house"))
            {
                response =
                    "Useful basics include bedding, towels, kitchen utensils, plates, cups, cutlery, cookware, " +
                    "cleaning products, rubbish bags, laundry supplies and basic storage.";
            }

            else if (question.Contains("affordable household") ||
                     question.Contains("cheap household") ||
                     question.Contains("cheap furniture"))
            {
                response =
                    "Compare supermarkets, discount/home-goods stores, second-hand shops and local online " +
                    "marketplaces. Check quality and return policies.";
            }

            else if (question.Contains("emergency supplies") ||
                     question.Contains("emergency at home") ||
                     question.Contains("emergency kit"))
            {
                response =
                    "Keep drinking water, long-lasting food, essential medicines, a torch, batteries, " +
                    "a first-aid kit, a phone charger or power bank and copies of important documents.";
            }

            // =========================================================
            // 9. DRIVING & GETTING AROUND
            // =========================================================

            else if (question.Contains("overseas licence") ||
                     question.Contains("overseas license") ||
                     question.Contains("foreign licence") ||
                     question.Contains("foreign license"))
            {
                response =
                    "If your overseas licence is current and valid and you meet NZTA requirements, " +
                    "you may be able to drive for a limited period. Check NZTA before driving.";
            }

            else if (question.Contains("how long") &&
                     (question.Contains("overseas licence") ||
                      question.Contains("overseas license")))
            {
                response =
                    "From 1 November 2026, the maximum period for a qualifying overseas car licence " +
                    "will be 12 months from your last arrival in New Zealand. Check NZTA for current rules.";
            }

            else if (question.Contains("licence not in english") ||
                     question.Contains("license not in english") ||
                     question.Contains("international driving permit"))
            {
                response =
                    "You need an accurate English translation or an acceptable international driving permit, " +
                    "and you must carry your current overseas licence.";
            }

            else if (question.Contains("convert overseas licence") ||
                     question.Contains("convert overseas license") ||
                     question.Contains("convert my licence") ||
                     question.Contains("convert my license"))
            {
                response =
                    "The process depends on the country that issued your licence. Check NZTA's overseas " +
                    "licence conversion requirements.";
            }

            else if (question.Contains("which side of the road") ||
                     question.Contains("what side of the road"))
            {
                response =
                    "New Zealand drives on the left-hand side of the road.";
            }

            else if (question.Contains("car insurance") ||
                     question.Contains("vehicle insurance"))
            {
                response =
                    "Third-party insurance is recommended at a minimum. If you finance a vehicle, " +
                    "your lender may require comprehensive insurance.";
            }

            // =========================================================
            // 10. EMPLOYMENT, TAX & EVERYDAY SERVICES
            // =========================================================

            else if (question.Contains("find a job") ||
                     question.Contains("find work") ||
                     question.Contains("employment"))
            {
                response =
                    "Use trusted job websites and government career services, prepare a New Zealand-style CV, " +
                    "and use professional and community networks.";
            }

            else if (question.Contains("employment help") ||
                     question.Contains("help finding a job") ||
                     question.Contains("work and income"))
            {
                response =
                    "Work and Income and other employment services may provide support depending on your circumstances. " +
                    "Check official government information.";
            }

            else if (question.Contains("public services") ||
                     question.Contains("government services"))
            {
                response =
                    "Eligibility depends on your visa and circumstances. Check Immigration New Zealand and the relevant government agency.";
            }

            else if (question.Contains("improve my english") ||
                     question.Contains("learn english") ||
                     question.Contains("english classes"))
            {
                response =
                    "Look for English-language courses and community learning options through local education " +
                    "providers and community organisations.";
            }

            else if (question.Contains("which government service") ||
                     question.Contains("don't know which service") ||
                     question.Contains("do not know which service"))
            {
                response =
                    "Start with Immigration New Zealand's 'Setting up your life in New Zealand' information " +
                    "or the relevant government agency.";
            }

            // =========================================================
            // 11. FAMILY, CHILDREN & COMMUNITY
            // =========================================================

            else if (question.Contains("enrol my child") ||
                     question.Contains("enroll my child") ||
                     question.Contains("child school") ||
                     question.Contains("school enrolment") ||
                     question.Contains("school enrollment"))
            {
                response =
                    "School enrolment depends on age, location and eligibility. Contact local schools " +
                    "and check Ministry of Education information.";
            }

            else if (question.Contains("childcare") ||
                     question.Contains("child care") ||
                     question.Contains("early childhood"))
            {
                response =
                    "Search for licensed early childhood education services and compare location, hours, fees and availability.";
            }

            else if (question.Contains("meet people") ||
                     question.Contains("make friends") ||
                     question.Contains("community"))
            {
                response =
                    "Try community groups, libraries, local events, sports clubs, volunteering, cultural " +
                    "organisations and newcomer support groups.";
            }

            else if (question.Contains("language support") ||
                     question.Contains("translation support") ||
                     question.Contains("interpreter"))
            {
                response =
                    "Check the relevant government or community service. Many public services can provide " +
                    "interpreter support, and Healthline offers interpreter assistance.";
            }

            // =========================================================
            // 12. FIRST-WEEK CHECKLIST
            // =========================================================

            else if (question.Contains("first week") ||
                     question.Contains("first-week checklist") ||
                     question.Contains("first week in new zealand"))
            {
                response =
                    "Set up your phone, accommodation and utilities; open a bank account; apply for an IRD " +
                    "number if needed; identify healthcare support; learn local transport; and find nearby " +
                    "supermarkets, pharmacies and emergency services.";
            }

            else if (question.Contains("important documents") ||
                     question.Contains("documents should i keep") ||
                     question.Contains("keep safe"))
            {
                response =
                    "Keep your passport, visa or immigration documents, identification, tenancy documents, " +
                    "bank information, insurance documents, licences and important contact details secure.";
            }

            else if (question.Contains("learn about my local area") ||
                     question.Contains("local area"))
            {
                response =
                    "Learn your nearest supermarket, pharmacy, GP or medical centre, public transport stop, " +
                    "emergency services, waste collection arrangements and important community facilities.";
            }

            else if (question.Contains("help settling in") ||
                     question.Contains("settling in") ||
                     question.Contains("settle in"))
            {
                response =
                    "Use trusted government, council, education-provider and community services. " +
                    "Aotearoa Is Home can help you find relevant settlement information and services.";
            }

            // =========================================================
            // 13. SHORT / ONE-WORD CATEGORY QUESTIONS
            // =========================================================
            // These rules are intentionally near the end.
            // Detailed questions above get priority.
            // These rules handle inputs such as:
            // rubbish, bus, bank, SIM, shopping, driving, etc.

            else if (IsAny(question, "rubbish", "waste", "garbage", "trash"))
            {
                response =
                    "Waste collection depends on your address and service. Check Wellington City Council's " +
                    "current rubbish and recycling information for your collection day and rules.";
            }

            else if (IsAny(question, "recycling", "recycle"))
            {
                response =
                    "Follow Wellington City Council's current recycling rules for your service. " +
                    "Items should be clean and prepared correctly.";
            }

            else if (IsAny(question, "bus", "buses", "train", "trains", "transport", "transportation"))
            {
                response =
                    "Wellington's Metlink network includes buses and trains, with ferry services also available. " +
                    "Check Metlink for routes, timetables, fares and service updates.";
            }

            else if (IsAny(question, "metlink"))
            {
                response =
                    "Metlink is the Wellington region's public transport network. Use its website or app " +
                    "to plan trips and check service information.";
            }

            else if (IsAny(question, "snapper"))
            {
                response =
                    "Snapper is a reusable transport card used on many Wellington public transport services. " +
                    "It can be topped up and used for eligible fares and passes.";
            }

            else if (IsAny(question, "bank", "banking", "credit card", "debit card"))
            {
                response =
                    "For banking in New Zealand, you can open a bank account, use cards and electronic payments, " +
                    "and keep a small amount of cash for situations where electronic payment is unavailable.";
            }

            else if (IsAny(question, "sim", "sim card", "esim", "phone", "mobile"))
            {
                response =
                    "A New Zealand SIM or eSIM is useful for connecting to local mobile networks and getting " +
                    "a New Zealand phone number. Compare providers, coverage, data and plan costs.";
            }

            else if (IsAny(question, "shopping", "shop", "groceries", "grocery", "supermarket"))
            {
                response =
                    "Supermarkets are the main place for groceries. Dairies and smaller stores are useful " +
                    "for quick purchases but may cost more.";
            }

            else if (IsAny(question, "driving", "drive", "licence", "license", "nzta"))
            {
                response =
                    "If your overseas licence is current and valid and you meet NZTA requirements, you may " +
                    "be able to drive for a limited period. Check NZTA before driving.";
            }

            else if (IsAny(question, "job", "jobs", "work", "employment", "career"))
            {
                response =
                    "Use trusted job websites and government career services, prepare a New Zealand-style CV, " +
                    "and use professional and community networks.";
            }

            else if (IsAny(question, "childcare", "child", "school", "education"))
            {
                response =
                    "School enrolment depends on age, location and eligibility. For childcare, search for " +
                    "licensed early childhood education services and compare location, hours, fees and availability.";
            }

            else if (IsAny(question, "doctor", "gp", "health", "healthcare", "pharmacy", "pharmacist"))
            {
                response =
                    "Find a local GP and ask about enrolment and appointments. You can also call Healthline " +
                    "free on 0800 611 116, 24 hours a day, 7 days a week.";
            }

            else if (IsAny(question, "housing", "house", "accommodation", "rent", "rental"))
            {
                response =
                    "Compare rental listings, check the location and costs, understand the tenancy terms, " +
                    "and use Tenancy Services information to understand your rights and responsibilities.";
            }

            else if (IsAny(question, "electricity", "power", "water", "internet", "wifi"))
            {
                response =
                    "For utilities, arrange electricity and other essential services for your address. " +
                    "Check with your landlord or property manager about what is already connected.";
            }

            else if (IsAny(question, "community", "friends", "friend", "social"))
            {
                response =
                    "Try community groups, libraries, local events, sports clubs, volunteering, cultural " +
                    "organisations and newcomer support groups.";
            }

            else if (IsAny(question, "emergency", "help"))
            {
                response =
                    "If you are in immediate danger or there is an emergency, call 111 for Police, Fire or Ambulance.";
            }

            // =========================================================
            // 14. GREETING
            // =========================================================

            else if (IsAny(question, "hi", "hello", "kia ora", "hey"))
            {
                response =
                    "Kia ora! I can provide quick answers about emergencies, healthcare, transport, " +
                    "banking, phones, accommodation, waste, shopping, employment and other settlement needs.";
            }

            // =========================================================
            // 15. FALLBACK
            // =========================================================

            else
            {
                response =
                    "Sorry, I don't have a specific answer for that yet. " +
                    "Try asking about emergency help, healthcare, transport, banking, phones, " +
                    "accommodation, waste, shopping, driving, employment or family services.";
            }

            return Json(new
            {
                response = response
            });
        }

        // =============================================================
        // HELPER METHODS
        // =============================================================

        private static string Normalize(string input)
        {
            input = input.Trim().ToLowerInvariant();

            // Replace punctuation with spaces.
            input = Regex.Replace(input, @"[^\p{L}\p{N}\s]", " ");

            // Remove multiple spaces.
            input = Regex.Replace(input, @"\s+", " ");

            return input.Trim();
        }

        private static bool IsAny(string question, params string[] keywords)
        {
            foreach (var keyword in keywords)
            {
                if (question == keyword ||
                    question.Contains(" " + keyword + " "))
                {
                    return true;
                }
            }

            return false;
        }
    }
}