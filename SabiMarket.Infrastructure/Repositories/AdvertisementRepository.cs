using Microsoft.EntityFrameworkCore;
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

