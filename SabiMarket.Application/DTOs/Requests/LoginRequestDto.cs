﻿namespace SabiMarket.Application.DTOs.Requests
{
    public class LoginRequestDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; } = false;
    }
}
