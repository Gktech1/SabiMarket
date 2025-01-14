using System.ComponentModel.DataAnnotations;

namespace SabiMarket.Application.DTOs.Requests
{
    public class UpdateProfileDto
    {
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string LocalGovernmentId { get; set; }
        public string ProfileImageUrl { get; set; }
    }


}
