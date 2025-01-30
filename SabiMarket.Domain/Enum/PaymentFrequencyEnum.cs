using System.ComponentModel.DataAnnotations;

namespace SabiMarket.Domain.Enum
{
    public enum PaymentFrequencyEnum
    {
        [Display(Name = "2 days")]
        TwoDays = 2,

        [Display(Name = "7 days")]
        Weekly = 7,

        [Display(Name = "30 days")]
        Monthly = 30
    }

    public enum GenderEnum
    {
        Male = 1,
        Female,
        Others
    }
}
