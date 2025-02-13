namespace SabiMarket.Domain.Entities.SowFoodLinkUp
{
    public class SowFoodCompanyProductionItem : BaseEntity
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string ImageUrl { get; set; }
        public string SowFoodCompanyId { get; set; }
        public SowFoodCompany SowFoodCompany { get; set; }
    }
}