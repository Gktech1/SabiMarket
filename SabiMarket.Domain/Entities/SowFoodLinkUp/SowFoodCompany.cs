namespace SabiMarket.Domain.Entities.SowFoodLinkUp
{
    public class SowFoodCompany : BaseEntity
    {
        public string CompanyName { get; set; }
        public virtual ICollection<SowFoodCompanyStaff> Staff { get; set; } = new List<SowFoodCompanyStaff>();
        public virtual ICollection<SowFoodCompanyProductionItem> SowFoodProducts { get; set; } = new List<SowFoodCompanyProductionItem>();
        public virtual ICollection<SowFoodCompanySalesRecord> SowFoodSalesRecords { get; set; } = new List<SowFoodCompanySalesRecord>();
        public virtual ICollection<SowFoodCompanyShelfItem> SowFoodShelfItems { get; set; } = new List<SowFoodCompanyShelfItem>();
        public virtual ICollection<SowFoodCompanyCustomer> Customers { get; set; } = new List<SowFoodCompanyCustomer>();
    }
}