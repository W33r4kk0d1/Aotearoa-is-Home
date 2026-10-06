using System.ComponentModel.DataAnnotations;

namespace Aotearoa_is_Home.Models
{
    public class Event : IValidatableObject
    {
        public int Id { get; set; }


        // BASIC EVENT INFORMATION

        [Required(ErrorMessage = "Event title is required.")]
        [StringLength(200, ErrorMessage = "Event title cannot exceed 200 characters.")]
        public string Title { get; set; } = string.Empty;


        [Required(ErrorMessage = "Event description is required.")]
        public string Description { get; set; } = string.Empty;


        [Required(ErrorMessage = "Please select an event category.")]
        [StringLength(100)]
        public string? Category { get; set; }


        // EVENT IMAGE
        public byte[]? ImageData { get; set; }

        public string? ImageContentType { get; set; }


        // SERVICE PROVIDER
        public int EventProviderProfileId { get; set; }

        public EventProviderProfile? EventProviderProfile { get; set; }


        // LOCATION
        [Required(ErrorMessage = "Location is required.")]
        [StringLength(
            150,
            ErrorMessage = "Location cannot exceed 150 characters."
        )]
        public string Location { get; set; } = string.Empty;


        [Required(ErrorMessage = "Please select a region.")]
        [StringLength(100)]
        public string? Region { get; set; } = string.Empty;


        // DATE AND TIME
        [Required(ErrorMessage = "Start date and time are required.")]
        public DateTime StartDate { get; set; }


        [Required(ErrorMessage = "End date and time are required.")]
        public DateTime EndDate { get; set; }



        // OFFICIAL EVENT INFORMATION
        [Url(ErrorMessage = "Please enter a valid URL.")]
        [StringLength(
            500,
            ErrorMessage = "Official event link cannot exceed 500 characters."
        )]
        public string? OfficialEventLink { get; set; }


        // EVENT RESPONSE / RSVP

        /// <summary>
        /// Allows visitors to express interest in the event.
        /// </summary>
        public bool EnableResponses { get; set; }


        /// <summary>
        /// Determines whether responses should be stored.
        /// </summary>
        public bool RecordResponses { get; set; }


        /// <summary>
        /// Determines whether the public can see the
        /// number of people interested.
        /// </summary>
        public bool ShowResponseCount { get; set; }


        /// <summary>
        /// Sends an email to the provider when someone
        /// expresses interest.
        /// </summary>
        public bool EmailOnResponse { get; set; }


        // EVENT STATUS

        [StringLength(50)]
        public string Status { get; set; } = "Pending Approval";


        // CREATED / POSTED DATE
        public DateTime CreatedAt { get; set; }


        // CUSTOM VALIDATION
        public IEnumerable<ValidationResult> Validate(
            ValidationContext validationContext)
        {
            // Get current New Zealand time

            var newZealandTimeZone =
                TimeZoneInfo.FindSystemTimeZoneById(
                    "Pacific/Auckland"
                );

            var currentNewZealandTime =
                TimeZoneInfo.ConvertTimeFromUtc(
                    DateTime.UtcNow,
                    newZealandTimeZone
                );


            // Start date must be in the future
            if (StartDate <= currentNewZealandTime)
            {
                yield return new ValidationResult(
                    "Event start date and time must be in the future.",
                    new[]
                    {
                        nameof(StartDate)
                    }
                );
            }


            // End date must be after start date
            if (EndDate <= StartDate)
            {
                yield return new ValidationResult(
                    "Event end date and time must be after the start date and time.",
                    new[]
                    {
                        nameof(EndDate)
                    }
                );
            }


            // Official event link validation

            if (!string.IsNullOrWhiteSpace(OfficialEventLink))
            {
                if (!Uri.TryCreate(
                        OfficialEventLink,
                        UriKind.Absolute,
                        out var uri)
                    ||
                    (uri.Scheme != Uri.UriSchemeHttp &&
                     uri.Scheme != Uri.UriSchemeHttps))
                {
                    yield return new ValidationResult(
                        "Official event link must be a valid HTTP or HTTPS URL.",
                        new[]
                        {
                            nameof(OfficialEventLink)
                        }
                    );
                }
            }


            // Response settings validation

            if (!EnableResponses)
            {
                if (RecordResponses ||
                    ShowResponseCount ||
                    EmailOnResponse)
                {
                    yield return new ValidationResult(
                        "Response settings cannot be enabled when Event Response is turned off."
                    );
                }
            }
        }
    }
}