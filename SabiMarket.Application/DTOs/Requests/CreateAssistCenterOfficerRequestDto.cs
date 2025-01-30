namespace SabiMarket.Application.DTOs.Requests
{
    public class CreateAssistCenterOfficerRequestDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string ChairmanId { get; set; }
        public string MarketId { get; set; }
        public string LocalGovernmentId { get; set; }
        public string UserLevel { get; set; }
        public string Password { get; set; }
    }

    public class UpdateAssistCenterOfficerProfileDto
    {
        public string PhoneNumber { get; set; }
        public string MarketId { get; set; }
        public string LocalGovernmentId { get; set; }
        public string UserLevel { get; set; }
        public string PhotoUrl { get; set; }
    }

    public class AssistCenterOfficerFilterRequestDto
    {
        public string MarketId { get; set; }
        public string LocalGovernmentId { get; set; }
        public string SearchTerm { get; set; }
        public bool? IsBlocked { get; set; }
        public string UserLevel { get; set; }
    }
}
