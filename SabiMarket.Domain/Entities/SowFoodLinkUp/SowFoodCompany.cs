namespace SabiMarket.Domain.Entities.SowFoodLinkUp
{
    public class SowFoodCompany : BaseEntity
    {
        public string CompanyName { get; set; }
        public ICollection<SowFoodCompanyStaff> Staff { get; set; } = new List<SowFoodCompanyStaff>();
        public ICollection<SowFoodCompanyProductionItem> SowFoodProducts { get; set; } = new List<SowFoodCompanyProductionItem>();
        public ICollection<SowFoodCompanySalesRecord> SowFoodSalesRecords { get; set; } = new List<SowFoodCompanySalesRecord>();
        public ICollection<SowFoodCompanyShelfItem> SowFoodShelfItems { get; set; } = new List<SowFoodCompanyShelfItem>();
        public ICollection<SowFoodCompanyCustomer> SowFoodCustomers { get; set; } = new List<SowFoodCompanyCustomer>();
    }
}