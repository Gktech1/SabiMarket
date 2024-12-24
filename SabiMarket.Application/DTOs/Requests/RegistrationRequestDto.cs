namespace SabiMarket.Application.DTOs.Requests
{
    public class RegistrationRequestDto
    {
        // Common fields for all registrations
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }  // VENDOR, CUSTOMER, ADVERTISER
        public string Address { get; set; }

        // Vendor specific fields
        public VendorDetailsDto VendorDetails { get; set; }

        // Customer specific fields
        public CustomerDetailsDto CustomerDetails { get; set; }

        // Advertiser specific fields
        public AdvertiserDetailsDto AdvertiserDetails { get; set; }
    }

    public class VendorDetailsDto
    {
        public string BusinessName { get; set; }
        public string BusinessType { get; set; }
        public string BusinessDescription { get; set; }
        public int LocalGovernmentId { get; set; }
    }

    public class CustomerDetailsDto
    {
        public string PreferredMarket { get; set; }
        public int LocalGovernmentId { get; set; }
    }

    public class AdvertiserDetailsDto
    {
        public string CompanyName { get; set; }
        public string BusinessType { get; set; }
        public string Website { get; set; }
    }

}
