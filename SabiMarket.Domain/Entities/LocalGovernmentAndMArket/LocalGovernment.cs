
using SabiMarket.Domain.Entities.WaiveMarketModule;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using SabiMarket.Domain.Entities.UserManagement;

namespace SabiMarket.Domain.Entities.LocalGovernmentAndMArket
{
    [Table("LocalGovernments")]
    public class LocalGovernment : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public string State { get; set; }

        public string Address { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentRevenue { get; set; }

        public virtual ICollection<ApplicationUser> Users { get; set; }
        public virtual ICollection<Market> Markets { get; set; }
        public virtual ICollection<Vendor> Vendors { get; set; }
        public virtual ICollection<Customer> Customers { get; set; }
    }

}