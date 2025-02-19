using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Utilities;

namespace SabiMarket.Infrastructure.Repositories
{
    public class LocalGovernmentRepository : GeneralRepository<LocalGovernment>, ILocalGovernmentRepository
    {
        private readonly ApplicationDbContext _repositoryContext;

        public LocalGovernmentRepository(ApplicationDbContext repositoryContext)
            : base(repositoryContext)
        {
            _repositoryContext = repositoryContext;
        }

        public async Task<LocalGovernment> GetLocalGovernmentById(string id, bool trackChanges) =>
            await FindByCondition(lg => lg.Id == id, trackChanges)
                .FirstOrDefaultAsync();

        public async Task<LocalGovernment> GetLocalGovernmentWithUsers(string id, bool trackChanges) =>
            await FindByCondition(lg => lg.Id == id, trackChanges)
                .Include(lg => lg.Users)
                .FirstOrDefaultAsync();

        public async Task<LocalGovernment> GetLocalGovernmentWithMarkets(string id, bool trackChanges) =>
            await FindByCondition(lg => lg.Id == id, trackChanges)
                .Include(lg => lg.Markets)
                .FirstOrDefaultAsync();

        public async Task<PaginatorDto<IEnumerable<LocalGovernment>>> GetLocalGovernmentsWithPagination(
            PaginationFilter paginationFilter, bool trackChanges)
        {
            var query = FindAll(trackChanges)
                .Include(lg => lg.Markets)
                .OrderBy(lg => lg.Name);

            return await query.Paginate(paginationFilter);
        }

        public async Task<LocalGovernment> GetLocalGovernmentByName(
            string name, string state, bool trackChanges) =>
            await FindByCondition(
                lg => lg.Name.ToLower() == name.ToLower() &&
                      lg.State.ToLower() == state.ToLower(),
                trackChanges)
            .FirstOrDefaultAsync();

        public async Task<decimal> GetTotalRevenue(string localGovernmentId)
        {
            var localGovernment = await FindByCondition(
                lg => lg.Id == localGovernmentId, false)
                .FirstOrDefaultAsync();

            return localGovernment?.CurrentRevenue ?? 0;
        }

        public async Task<bool> LocalGovernmentExists(string name, string state) =>
            await FindByCondition(
                lg => lg.Name.ToLower() == name.ToLower() &&
                      lg.State.ToLower() == state.ToLower(),
                trackChanges: false)
            .AnyAsync();

        public async Task<bool> LocalGovernmentExist(string localgovernmentId) =>
            await FindByCondition(
                lg => lg.Id == localgovernmentId,
                trackChanges: false)
            .AnyAsync();
        public void CreateLocalGovernment(LocalGovernment localGovernment) =>
            Create(localGovernment);

        public void UpdateLocalGovernment(LocalGovernment localGovernment) =>
            Update(localGovernment);

        public void DeleteLocalGovernment(LocalGovernment localGovernment) =>
            Delete(localGovernment);

        public async Task<PaginatorDto<IEnumerable<LocalGovernment>>> GetLocalGovernmentAreas(
     string searchTerm,
     string officerName,  // Changed from chairmanName to officerName
     bool? isActive,
     string state,
     string orderBy,
     bool? isDescending,
     PaginationFilter paginationFilter)
        {
            var predicate = PredicateBuilder.New<LocalGovernment>(true);

            // Apply search term filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                predicate = predicate.And(lg =>
                    lg.Name.ToLower().Contains(term) ||
                    lg.Address.ToLower().Contains(term) ||
                    (lg.LGA != null && lg.LGA.ToLower().Contains(term)));
            }

            // Apply officer name filter
            if (!string.IsNullOrWhiteSpace(officerName))
            {
                var name = officerName.ToLower();
                predicate = predicate.And(lg =>
                    lg.AssistCenterOfficers.Any(officer =>
                        officer.User.FirstName.ToLower().Contains(name) ||
                        officer.User.LastName.ToLower().Contains(name)));
            }

            // Apply active status filter
            if (isActive.HasValue)
            {
                predicate = predicate.And(lg => lg.IsActive == isActive.Value);
            }

            // Apply state filter
            if (!string.IsNullOrWhiteSpace(state))
            {
                var stateValue = state.ToLower();
                predicate = predicate.And(lg => lg.State.ToLower().Contains(stateValue));
            }

            // Start with base query including AssistCenterOfficers and their User information
            var query = FindAll(trackChanges: false)
                .Include(lg => lg.AssistCenterOfficers)
                    .ThenInclude(aco => aco.User)
                .Where(predicate);

            // Apply ordering
            query = ApplyLocalGovernmentOrdering(query, orderBy, isDescending ?? false);

            // Apply pagination and return results
            return await query.Paginate(paginationFilter);
        }

        private static IQueryable<LocalGovernment> ApplyLocalGovernmentOrdering(
            IQueryable<LocalGovernment> query,
            string orderBy,
            bool isDescending)
        {
            return (orderBy?.ToLower(), isDescending) switch
            {
                ("name", true) => query.OrderByDescending(lg => lg.Name),
                ("name", false) => query.OrderBy(lg => lg.Name),
                ("state", true) => query.OrderByDescending(lg => lg.State),
                ("state", false) => query.OrderBy(lg => lg.State),
                ("revenue", true) => query.OrderByDescending(lg => lg.CurrentRevenue),
                ("revenue", false) => query.OrderBy(lg => lg.CurrentRevenue),
                ("createdat", true) => query.OrderByDescending(lg => lg.CreatedAt),
                ("createdat", false) => query.OrderBy(lg => lg.CreatedAt),
                ("lga", true) => query.OrderByDescending(lg => lg.LGA),
                ("lga", false) => query.OrderBy(lg => lg.LGA),
                _ => query.OrderBy(lg => lg.Name) // Default ordering
            };
        }
        public IQueryable<LocalGovernment> GetFilteredLGAsQuery(LGAFilterRequestDto filterDto)
        {
            // Start with a base query without unnecessary includes
            var query = FindAll(trackChanges: false);

            // Build the where clause using PredicateBuilder for dynamic filtering
            var predicate = PredicateBuilder.New<LocalGovernment>(true);

            // Apply filters only if they exist (using case-insensitive comparison)
            if (!string.IsNullOrWhiteSpace(filterDto.State))
            {
                var state = filterDto.State.ToLower();
                predicate = predicate.And(lg => lg.State.ToLower() == state);
            }

            if (!string.IsNullOrWhiteSpace(filterDto.LGA))
            {
                var lga = filterDto.LGA.ToLower();
                predicate = predicate.And(lg => lg.LGA.ToLower() == lga);
            }

            if (!string.IsNullOrWhiteSpace(filterDto.Name))
            {
                var name = filterDto.Name.ToLower();
                predicate = predicate.And(lg => lg.Name.ToLower().Contains(name));
            }

            if (filterDto.MinRevenue.HasValue)
            {
                predicate = predicate.And(lg => lg.CurrentRevenue >= filterDto.MinRevenue.Value);
            }

            if (filterDto.MaxRevenue.HasValue)
            {
                predicate = predicate.And(lg => lg.CurrentRevenue <= filterDto.MaxRevenue.Value);
            }

            // Apply the combined predicate
            query = query.Where(predicate);

            // Apply market activity filter separately to avoid unnecessary joins if not needed
            if (filterDto.HasActiveMarkets.HasValue)
            {
                query = filterDto.HasActiveMarkets.Value
                    ? query.Where(lg => lg.Markets.Any(m => m.IsActive))
                    : query.Where(lg => !lg.Markets.Any(m => m.IsActive));
            }

            // Apply sorting only once at the end
            var sortProperty = filterDto.SortBy?.ToLower();
            var isDescending = filterDto.SortOrder?.ToLower() == "desc";

            // Include only the necessary related entities based on sort criteria
            if (sortProperty == "markets" || filterDto.HasActiveMarkets.HasValue)
            {
                query = query.Include(lg => lg.Markets);
            }
            if (sortProperty == "vendors")
            {
                query = query.Include(lg => lg.Vendors);
            }
            if (sortProperty == "customers")
            {
                query = query.Include(lg => lg.Customers);
            }

            // Apply sorting using a more efficient approach
            query = ApplySorting(query, sortProperty, isDescending);

            return query;
        }

        private static IQueryable<LocalGovernment> ApplySorting(
            IQueryable<LocalGovernment> query,
            string sortProperty,
            bool isDescending)
        {
            return (sortProperty, isDescending) switch
            {
                ("name", true) => query.OrderByDescending(lg => lg.Name),
                ("name", false) => query.OrderBy(lg => lg.Name),
                ("revenue", true) => query.OrderByDescending(lg => lg.CurrentRevenue),
                ("revenue", false) => query.OrderBy(lg => lg.CurrentRevenue),
                ("markets", true) => query.OrderByDescending(lg => lg.Markets.Count()),
                ("markets", false) => query.OrderBy(lg => lg.Markets.Count()),
                ("vendors", true) => query.OrderByDescending(lg => lg.Vendors.Count()),
                ("vendors", false) => query.OrderBy(lg => lg.Vendors.Count()),
                ("customers", true) => query.OrderByDescending(lg => lg.Customers.Count()),
                ("customers", false) => query.OrderBy(lg => lg.Customers.Count()),
                ("createdat", true) => query.OrderByDescending(lg => lg.CreatedAt),
                ("createdat", false) => query.OrderBy(lg => lg.CreatedAt),
                _ => query.OrderBy(lg => lg.Name)
            };
        }
    }

    // DTOs
   /* public class LocalGovernmentDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
        public string Address { get; set; }
        public decimal CurrentRevenue { get; set; }
        public int TotalMarkets { get; set; }
        public int TotalUsers { get; set; }
    }*/

}
