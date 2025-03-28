﻿namespace SabiMarket.Application.DTOs.Responses
{
    public class LoginResponseDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }    
        public string TokenType { get; set; } = "Bearer";
        public DateTime ExpiresAt { get; set; }
        public UserClaimsDto UserInfo { get; set; }
    }

    public class UserClaimsDto
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public DateTime LastLoginAt { get; set; }
        public string ProfileImageUrl { get; set; }
        public string? ChairmanId { get; set; }
        public string? CaretakerId { get; set; }
        public string? GoodBoyId { get; set; }  
        public string? MarketId { get; set; }
        public string? TraderId { get; set; }
        public string? AssistOfficerId { get; set; }
        public IDictionary<string, object> AdditionalDetails { get; set; }
    }
}
