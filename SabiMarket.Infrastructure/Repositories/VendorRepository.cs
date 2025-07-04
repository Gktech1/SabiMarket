using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.IRepositories;
using SabiMarket.Domain.Entities.WaiveMarketModule;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Utilities;

namespace SabiMarket.Infrastructure.Repositories
{
    public class VendorRepository : GeneralRepository<Vendor>, IVendorRepository
    {
        private readonly ApplicationDbContext _repositoryContext;
        public VendorRepository(ApplicationDbContext repositoryContext)
            : base(repositoryContext)
        {
            _repositoryContext = repositoryContext;
        }

        public async Task<Vendor> GetVendorById(string id, bool trackChanges) =>
            await FindByCondition(v => v.Id == id, trackChanges)
                .FirstOrDefaultAsync();

        public async Task<Vendor> GetVendorWithApplicationUser(string id, bool trackChanges) =>
            await FindByCondition(v => v.Id == id, trackChanges)
                .Include(v => v.User)
                .FirstOrDefaultAsync();

        public async Task<Vendor> GetVendorByUserId(string userId, bool trackChanges) =>
            await FindByCondition(v => v.UserId == userId, trackChanges).Include(x => x.User).ThenInclude(l => l.LocalGovernment)
                .FirstOrDefaultAsync();


        public async Task<Vendor> GetVendorDetails(string userId)
        {
            var vendor = await FindByCondition(v => v.UserId == userId, trackChanges: false)
                .Include(v => v.User)  // Include the related User entity
                .Include(v => v.LocalGovernment)  // Include the related LocalGovernment entity (if necessary)
                .Include(v => v.Products)  // Include the related Products (if needed)
                .Include(v => v.Orders)  // Include Orders (if needed)
                .Include(v => v.Feedbacks)  // Include Feedbacks (if needed)
                .Include(v => v.Advertisements)  // Include Advertisements (if needed)
                .FirstOrDefaultAsync();

            return vendor;
        }


        public async Task<PaginatorDto<IEnumerable<Vendor>>> GetVendorsWithPagination(PaginationFilter paginationFilter, bool trackChanges, string searchString, string filterString)
        {
            var query = _repositoryContext.Vendors
                .Include(p => p.Products)
                .Include(p => p.User)
                .Include(l => l.LocalGovernment)
                    .ThenInclude(x => x.Markets).AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var lowerSearch = searchString.ToLower();

                query = query.Where(v =>
                    v.BusinessName.ToLower().Contains(lowerSearch) ||
                    v.BusinessAddress.ToLower().Contains(lowerSearch) ||
                    v.User.FirstName.ToLower().Contains(lowerSearch) ||
                    v.User.LastName.ToLower().Contains(lowerSearch) ||
                    v.User.Email.ToLower().Contains(lowerSearch) ||
                    v.User.PhoneNumber.ToLower().Contains(lowerSearch));
            }
            if (!string.IsNullOrWhiteSpace(filterString))
            {
                var lowerSearch = filterString.ToLower();

                query = query.Where(v =>
                    v.LocalGovernment.Name.ToLower().Contains(lowerSearch));
            }

            query = query.OrderBy(v => v.User.FirstName);

            return await query.Paginate(paginationFilter);
        }

        public async Task<bool> VendorExists(string userId) =>
            await FindByCondition(v => v.UserId == userId, trackChanges: false)
                .AnyAsync();

        public void CreateVendor(Vendor vendor) =>
            Create(vendor);

        public void UpdateVendor(Vendor vendor) =>
            Update(vendor);

        public void DeleteVendor(Vendor vendor) =>
            Delete(vendor);
    }

}
