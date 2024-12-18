using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using SabiMarket.Domain.Enum;
using SabiMarket.Domain.Entities.WaiveMarketModule;

namespace SabiMarket.Domain.Entities.AdvertisementModule
{
    [Table("Advertisements")]
    public class Advertisement : BaseEntity
    {
        public Guid VendorId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public string ImageUrl { get; set; }
        public string TargetUrl { get; set; }
        public AdvertStatusEnum Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public virtual Vendor Vendor { get; set; }
        public virtual ICollection<AdvertisementView> Views { get; set; }
    }

}
