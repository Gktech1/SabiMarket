using System.ComponentModel.DataAnnotations.Schema;
using SabiMarket.Domain.Entities;
using SabiMarket.Domain.Entities.LevyManagement;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;
using SabiMarket.Domain.Entities.MarketParticipants;
using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Domain.Enum;

[Table("GoodBoys")]
public class GoodBoy : BaseEntity
{
    public string UserId { get; set; }
    public string CaretakerId { get; set; }
    public string MarketId {  get; set; }    
    public StatusEnum Status { get; set; }
    public string? DeviceToken { get; set; } // For push notifications

    // For statistics
    public int TotalCollections { get; set; } = 0;  
    public decimal? TotalAmountCollected { get; set; } = decimal.Zero;
    public virtual Market Market { get; set; }
    public virtual ApplicationUser User { get; set; }
    public virtual Caretaker Caretaker { get; set; }
    public virtual ICollection<LevyPayment> LevyPayments { get; set; }
}

