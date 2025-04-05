using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Advertisement;

namespace SabiMarket.Infrastructure.Repositories
{
    public interface IAdvertisementRepository
    {
        Task<Advertisement> GetAdvertisementById(string id, bool trackChanges);
        Task<Advertisement> GetAdvertisementWithVendor(string id, bool trackChanges);
        Task<Advertisement> GetAdvertisementByVendorId(string vendorId, bool trackChanges);
        Task<Advertisement> GetAdvertisementDetails(string id);
        Task<PaginatorDto<IEnumerable<Advertisement>>> GetAdvertisementsWithPagination(
            PaginationFilter paginationFilter, bool trackChanges);
        Task<PaginatorDto<IEnumerable<Advertisement>>> GetFilteredAdvertisements(
           AdvertisementFilterRequestDto filterDto, string vendorId, PaginationFilter paginationFilter);
        Task<bool> AdvertisementExists(string id);
        void CreateAdvertisement(Advertisement advertisement);
        void UpdateAdvertisement(Advertisement advertisement);
        void DeleteAdvertisement(Advertisement advertisement);
    }
}
