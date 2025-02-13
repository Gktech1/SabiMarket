namespace SabiMarket.Domain.Entities.SowFoodLinkUp
{
    public class SowFoodCompanyCustomer : BaseEntity
    {
        public string SowFoodCompanyId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string RegisteredBy { get; set; }
        public ICollection<SowFoodCompanySalesRecord> SowFoodCompanySalesRecords { get; set; }
        public ICollection<SowFoodCompany> SowFoodCompany { get; set; }
    }
}