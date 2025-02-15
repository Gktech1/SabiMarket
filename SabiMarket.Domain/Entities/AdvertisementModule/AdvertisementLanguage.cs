using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SabiMarket.Domain.Entities.AdvertisementModule
{
    [Table("AdvertisementLanguages")]
    public class AdvertisementLanguage : BaseEntity
    {
        public string AdvertisementId { get; set; }

        [Required]
        public string Language { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public virtual Advertisement Advertisement { get; set; }
    }
}
