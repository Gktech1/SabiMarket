using System.ComponentModel.DataAnnotations.Schema;
using SabiMarket.Domain.Entities;
using SabiMarket.Domain.Entities.LevyManagement;
using SabiMarket.Domain.Entities.MarketParticipants;
using SabiMarket.Domain.Entities.UserManagement;

[Table("GoodBoys")]
public class GoodBoy : BaseEntity
{
    public string UserId { get; set; }
    public string CaretakerId { get; set; }
    public virtual ApplicationUser User { get; set; }
    public virtual Caretaker Caretaker { get; set; }
    public virtual ICollection<LevyPayment> LevyPayments { get; set; }
}

