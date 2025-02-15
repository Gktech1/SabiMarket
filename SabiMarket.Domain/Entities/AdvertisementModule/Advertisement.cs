using SabiMarket.Domain.Entities;
using SabiMarket.Domain.Entities.Administration;
using SabiMarket.Domain.Entities.AdvertisementModule;
using SabiMarket.Domain.Entities.WaiveMarketModule;
using SabiMarket.Domain.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Advertisements")]
public class Advertisement : BaseEntity
{
    public string VendorId { get; set; }
    public string AdminId { get; set; }  // Add this foreign key

    [Required]
    public string Title { get; set; }

    [Required]
    public string Description { get; set; }

    public string ImageUrl { get; set; }
    public string TargetUrl { get; set; }

    // Status Management
    public AdvertStatusEnum Status { get; set; }

    // Timing Properties
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    // New Properties from Figma UI
    public string AdvertId { get; set; }  // For custom IDs like Ad101

    [Required]
    public string Language { get; set; }  // For multilingual support

    // Location and Placement
    [Required]
    public string Location { get; set; }  // All location, SowFood Linkup, LG Linkup, etc.

    [Required]
    public string AdvertPlacement { get; set; }  // Vendor, Customer, Chairman, etc.

    // Payment Tracking
    public string PaymentStatus { get; set; }
    public string PaymentProofUrl { get; set; }
    public string BankTransferReference { get; set; }

    // Navigation Properties
    public virtual Vendor Vendor { get; set; }
    public virtual Admin Admin { get; set; }  // Add this navigation property
    public virtual ICollection<AdvertisementView> Views { get; set; }
    public virtual ICollection<AdvertisementLanguage> Translations { get; set; }
    public virtual AdvertPayment Payment { get; set; }
}