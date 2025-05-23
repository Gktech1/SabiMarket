﻿using SabiMarket.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Application.DTOs.Responses
{
    public class TraderQRValidationResponseDto
    {
        public string TraderId { get; set; }
        public string TraderName { get; set; }
        public string TraderOccupancy { get; set; }
        public string TraderIdentityNumber { get; set; }
        public string PaymentFrequency { get; set; }
        public PaymentPeriodEnum PayementPeriod { get; set; } = PaymentPeriodEnum.Weekly;  
        public decimal Amount { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public string? UpdatePaymentUrl { get; set; }    
    }
}
