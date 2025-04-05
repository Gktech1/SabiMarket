using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Advertisement;
using SabiMarket.Domain.Enum;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Utilities;

namespace SabiMarket.Infrastructure.Repositories
{
    public class AdvertisementRepository : GeneralRepository<Advertisement>, IAdvertisementRepository
    {
        private readonly ApplicationDbContext _repositoryContext;

        public AdvertisementRepository(ApplicationDbContext repositoryContext)
            : base(repositoryContext)
        {
            _repositoryContext = repositoryContext;
        }

        public async Task<Advertisement> GetAdvertisementById(string id, bool trackChanges) =>
            await FindByCondition(a => a.Id == id, trackChanges)
                .FirstOrDefaultAsync();

        public async Task<Advertisement> GetAdvertisementWithVendor(string id, bool trackChanges) =>
            await FindByCondition(a => a.Id == id, trackChanges)
                .Include(a => a.Vendor)
                .Include(a => a.Vendor.User)
                .FirstOrDefaultAsync();

        public async Task<Advertisement> GetAdvertisementByVendorId(string vendorId, bool trackChanges) =>
            await FindByCondition(a => a.VendorId == vendorId, trackChanges)
                .FirstOrDefaultAsync();

        public async Task<Advertisement> GetAdvertisementDetails(string id)
        {
            var advertisement = await FindByCondition(a => a.Id == id, trackChanges: false)
                .Include(a => a.Vendor)
                .Include(a => a.Admin)
                .Include(a => a.Views)
                .Include(a => a.Translations)
                .Include(a => a.Payment)
                .FirstOrDefaultAsync();

            return advertisement;
        }

        public async Task<PaginatorDto<IEnumerable<Advertisement>>> GetAdvertisementsWithPagination(
            PaginationFilter paginationFilter, bool trackChanges)
        {
            var query = FindAll(trackChanges)
                .Include(a => a.Vendor)
                .Include(a => a.Views)
                .OrderByDescending(a => a.CreatedAt);

            return await query.Paginate(paginationFilter);
        }

        public async Task<PaginatorDto<IEnumerable<Advertisement>>> GetFilteredAdvertisements(
            AdvertisementFilterRequestDto filterDto, string vendorId, PaginationFilter paginationFilter)
        {
            // Start with base query
            var query = FindAll(trackChanges: false)
                .Include(a => a.Vendor)
                .Include(a => a.Views)
                .AsQueryable();

            // Apply vendor filter if specified
            if (!string.IsNullOrEmpty(vendorId))
            {
                query = query.Where(a => a.VendorId == vendorId);
            }

            // Apply search term filter
            if (!string.IsNullOrEmpty(filterDto.SearchTerm))
            {
                var searchTerm = filterDto.SearchTerm.ToLower();
                query = query.Where(a =>
                    a.Title.ToLower().Contains(searchTerm) ||
                    a.Description.ToLower().Contains(searchTerm));
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(filterDto.Status))
            {
                if (Enum.TryParse<AdvertStatusEnum>(filterDto.Status, true, out var statusEnum))
                {
                    query = query.Where(a => a.Status == statusEnum);
                }
            }

            // Apply location filter
            if (!string.IsNullOrEmpty(filterDto.Location))
            {
                query = query.Where(a => a.Location == filterDto.Location);
            }

            // Apply language filter
            if (!string.IsNullOrEmpty(filterDto.Language))
            {
                query = query.Where(a => a.Language == filterDto.Language);
            }

            // Apply placement filter
            if (!string.IsNullOrEmpty(filterDto.AdvertPlacement))
            {
                query = query.Where(a => a.AdvertPlacement == filterDto.AdvertPlacement);
            }

            // Apply date filters
            if (filterDto.StartDateFrom.HasValue)
            {
                query = query.Where(a => a.StartDate >= filterDto.StartDateFrom.Value);
            }

            if (filterDto.StartDateTo.HasValue)
            {
                query = query.Where(a => a.StartDate <= filterDto.StartDateTo.Value);
            }

            if (filterDto.EndDateFrom.HasValue)
            {
                query = query.Where(a => a.EndDate >= filterDto.EndDateFrom.Value);
            }

            if (filterDto.EndDateTo.HasValue)
            {
                query = query.Where(a => a.EndDate <= filterDto.EndDateTo.Value);
            }

            // Order by creation date, newest first
            query = query.OrderByDescending(a => a.CreatedAt);

            // Execute pagination
            return await query.Paginate(paginationFilter);
        }

        public async Task<bool> AdvertisementExists(string id) =>
            await FindByCondition(a => a.Id == id, trackChanges: false)
                .AnyAsync();

        public void CreateAdvertisement(Advertisement advertisement) =>
            Create(advertisement);

        public void UpdateAdvertisement(Advertisement advertisement) =>
            Update(advertisement);

        public void DeleteAdvertisement(Advertisement advertisement) =>
            Delete(advertisement);
    }
}


/*using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Repositories;
using SabiMarket.Infrastructure.Utilities;

public class AdvertisementRepository : GeneralRepository<Advertisement>, IAdvertisementRepository
{
    private readonly ApplicationDbContext _repositoryContext;

    public AdvertisementRepository(ApplicationDbContext repositoryContext)
        : base(repositoryContext)
    {
        _repositoryContext = repositoryContext;
    }

    public async Task<Advertisement> GetAdvertisementById(string id, bool trackChanges) =>
        await FindByCondition(a => a.Id == id, trackChanges)
            .FirstOrDefaultAsync();

    public async Task<Advertisement> GetAdvertisementWithVendor(string id, bool trackChanges) =>
        await FindByCondition(a => a.Id == id, trackChanges)
            .Include(a => a.Vendor)
            .Include(a => a.Vendor.User)
            .FirstOrDefaultAsync();

    public async Task<Advertisement> GetAdvertisementByVendorId(string vendorId, bool trackChanges) =>
        await FindByCondition(a => a.VendorId == vendorId, trackChanges)
            .FirstOrDefaultAsync();

    public async Task<Advertisement> GetAdvertisementDetails(string id)
    {
        var advertisement = await FindByCondition(a => a.Id == id, trackChanges: false)
            .Include(a => a.Vendor)
            .Include(a => a.Admin)
            .Include(a => a.Views)
            .Include(a => a.Translations)
            .Include(a => a.Payment)
            .FirstOrDefaultAsync();
        return advertisement;
    }

    public async Task<PaginatorDto<IEnumerable<Advertisement>>> GetAdvertisementsWithPagination(
        PaginationFilter paginationFilter, bool trackChanges)
    {
        var query = FindAll(trackChanges)
            .Include(a => a.Vendor)
            .OrderByDescending(a => a.CreatedAt);
        return await query.Paginate(paginationFilter);
    }

    public async Task<bool> AdvertisementExists(string id) =>
        await FindByCondition(a => a.Id == id, trackChanges: false)
            .AnyAsync();

    public void CreateAdvertisement(Advertisement advertisement) =>
        Create(advertisement);

    public void UpdateAdvertisement(Advertisement advertisement) =>
        Update(advertisement);

    public void DeleteAdvertisement(Advertisement advertisement) =>
        Delete(advertisement);
}

*/