using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SabiMarket.Domain.Entities;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;
using SabiMarket.Domain.Entities.MarketParticipants;

[Table("MarketSections")]
public class MarketSection : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; }
    public string Description { get; set; }
    public int Capacity { get; set; }
    public bool IsActive { get; set; }

    // Add this property
    public string MarketId { get; set; }  // Foreign key for Market

    public virtual Market Market { get; set; }
    public virtual ICollection<Trader> Traders { get; set; }
}