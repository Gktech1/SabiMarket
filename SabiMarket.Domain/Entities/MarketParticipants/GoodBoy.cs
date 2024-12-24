using SabiMarket.Domain.Entities;
using SabiMarket.Domain.Entities.LevyManagement;
using SabiMarket.Domain.Entities.MarketParticipants;
using SabiMarket.Domain.Entities.UserManagement;
using System.ComponentModel.DataAnnotations.Schema;

[Table("GoodBoys")]
public class GoodBoy : BaseEntity
{
    public string UserId { get; set; }
    public Guid CaretakerId { get; set; }
    public virtual ApplicationUser User { get; set; }
    public virtual Caretaker Caretaker { get; set; }
    public virtual ICollection<LevyPayment> LevyPayments { get; set; }
}

