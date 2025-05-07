
using SabiMarket.Domain.Enum;
using System.ComponentModel.DataAnnotations;

public class CreateTraderRequestDto
{
    [Required]
    public string TraderName { get; set; }

    [Required]
    public string PhoneNumber { get; set; }

    public string? Email { get; set; }

    [Required]
    public string BusinessName { get; set; }

    [Required]
    public string BusinessType { get; set; }

    public string? TIN { get; set; }

    [Required]
    public string MarketId { get; set; }

    public string? SectionId { get; set; }

  /*  [Required]
    public string CaretakerId { get; set; }*/

    [Required]
    public MarketTypeEnum TraderOccupancy { get; set; }

    public string? Gender { get; set; }

    public string? ProfileImage { get; set; }
}

public class UpdateTraderRequestDto
{
    public string? TraderName { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public string? BusinessName { get; set; }

    public string? BusinessType { get; set; }

    public string? TIN { get; set; }

    public string? MarketId { get; set; }

    public string? SectionId { get; set; }

    public string? CaretakerId { get; set; }

    public MarketTypeEnum? TraderOccupancy { get; set; }

    public string? Gender { get; set; }

    public string? ProfileImage { get; set; }
}

// Response DTO
/*public class TraderResponseDto
{
    public string Id { get; set; }

    public string TraderName { get; set; }

    public string PhoneNumber { get; set; }

    public string Email { get; set; }

    public string BusinessName { get; set; }

    public string BusinessType { get; set; }

    public string TIN { get; set; }

    public string QRCode { get; set; }

    public MarketTypeEnum TraderOccupancy { get; set; }

    public string MarketId { get; set; }

    public string MarketName { get; set; }

    public string? SectionId { get; set; }

    public string? SectionName { get; set; }

    public string CaretakerId { get; set; }

    public string CaretakerName { get; set; }

    public string DefaultPassword { get; set; }

    public string Gender { get; set; }

    public string ProfileImageUrl { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}*/