﻿using SabiMarket.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Application.DTOs.Requests
{
    public class CreateGoodBoyRequestDto
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string CaretakerId { get; set; }
        public string MarketId { get; set; }
    }

    public class UpdateGoodBoyProfileDto
    {
        public string PhoneNumber { get; set; }
    }

    public class GoodBoyFilterRequestDto
    {
        public string MarketId { get; set; }
        public string CaretakerId { get; set; }
        public StatusEnum? Status { get; set; }
    }

    public class ProcessLevyPaymentDto
    {
        public decimal Amount { get; set; }
    }
}
