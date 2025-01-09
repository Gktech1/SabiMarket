using System.ComponentModel.DataAnnotations;

namespace SabiMarket.Application.DTOs.Requests
{
    public class UpdateProfileDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
    }

}
