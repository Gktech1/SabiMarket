using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Application.DTOs.Requests
{
    // CreateTeamMemberRequestDto.cs
    public class CreateTeamMemberRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }
    }

    // UpdateTeamMemberRequestDto.cs
    public class UpdateTeamMemberRequestDto
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }

        public string? EmailAddress { get; set; }
    }

    // TeamMemberFilterRequestDto.cs
    public class TeamMemberFilterRequestDto
    {
        public string SearchTerm { get; set; }
    }
}
