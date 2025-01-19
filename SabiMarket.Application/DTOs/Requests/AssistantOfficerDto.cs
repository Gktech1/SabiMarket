namespace SabiMarket.Application.DTOs
{
    public class AssistantOfficerDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Level { get; set; }
        public string MarketId { get; set; }
    }

    public class CreateAssistantOfficerRequestDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Level { get; set; }
        public string MarketId { get; set; }
    }

    public class UpdateAssistantOfficerRequestDto
    {
        public string PhoneNumber { get; set; }
        public string Level { get; set; }
    }

    public class BlockAssistantOfficerDto
    {
        public string AssistantOfficerId { get; set; }
        public bool IsBlocked { get; set; }
    }
}
