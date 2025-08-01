using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Domain.Enum;

namespace SabiMarket.Application.DTOs.Requests
{
    public class CreateSubscriptionDto
    {
        public decimal Amount { get; set; }
        //public string PaymentMethod { get; set; }
        public string ProofOfPayment { get; set; }
        public string SubscriberId { get; set; }
        public string SubscriberPlanId { get; set; }
        public string? PaymentRef { get; set; }
        public string SubscriberType { get; set; }
    }
    public class CreateSubscriptionPlanDto
    {
        public decimal Amount { get; set; }
        public string Frequency { get; set; }
        public string UserType { get; set; }

    }
    public class UpdateSubscriptionPlanDto
    {
        public decimal Amount { get; set; }
        public string Frequency { get; set; }
        public string UserType { get; set; }
        public string Id { get; set; }

    }
    public class GetSubScriptionDto
    {
        public DateTime SubscriptionDate { get; set; }
        public string Product { get; set; }
        public decimal Amount { get; set; }
    }
    public class GetSubScriptionPlanDto
    {
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Frequency { get; set; }
        public decimal Amount { get; set; }
        public int NumberOfSubscribers { get; set; }
        public int NumberOfActiveSubscribers { get; set; }
        public int NumberOfInActiveSubscribers { get; set; }
        public bool IsActive { get; set; }
    }

    public class GetSubscriptionDto
    {
        public string Id { get; set; }
        public string Frequency { get; set; }
        public int Currency { get; set; }
        public decimal Amount { get; set; }
        public string UserType { get; set; }
        public DateTime DateCreated { get; set; }
    }

    public class SubscribedUserDto
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; }
        public DateTime? SubscriptionStartDate { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }
    }

    public class GetSubscriptionUserDto
    {
        public string SubscriberId { get; set; }
        public string SubscriptionId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string ProofOfPayment { get; set; }
        public bool IsActive { get; set; }
        public DateTime? SubscriptionStartDate { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }
        public DateTime DateCreated { get; set; }
        public decimal Amount { get; set; }
    }

}
