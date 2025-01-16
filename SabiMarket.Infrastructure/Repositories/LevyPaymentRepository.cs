using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.IRepositories;
using SabiMarket.Domain.Entities.LevyManagement;
using SabiMarket.Domain.Enum;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Utilities;

namespace SabiMarket.Infrastructure.Repositories
{
    public class LevyPaymentRepository : GeneralRepository<LevyPayment>, ILevyPaymentRepository
    {
        public LevyPaymentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public void AddPayment(LevyPayment levyPayment) => Create(levyPayment);

        public async Task<IEnumerable<LevyPayment>> GetAllLevyPaymentForExport(bool trackChanges) => await FindAll(trackChanges).ToListAsync();

        public async Task<LevyPayment> GetPaymentById(string id, bool trackChanges) => await FindByCondition(x => x.Id == id, trackChanges).FirstOrDefaultAsync();

        public async Task<PaginatorDto<IEnumerable<LevyPayment>>> GetPagedPayment(int? period, PaginationFilter paginationFilter)
        {
            if (period is not null)
            {
                return await FindAll(false)
                            .Where(l => Convert.ToInt32(l.Period) == period)
                            .Paginate(paginationFilter);
            }
            else
            {
                return await FindAll(false)
                           .Paginate(paginationFilter);
            }
        }

        public async Task<PaginatorDto<IEnumerable<LevyPayment>>> GetLevyPaymentsAsync(
        string chairmanId, PaginationFilter paginationFilter, bool trackChanges)
        {
            return await FindPagedByCondition(
                expression: lp => lp.ChairmanId == chairmanId, 
                paginationFilter: paginationFilter,
                trackChanges: trackChanges,
                orderBy: query => query.OrderByDescending(lp => lp.CreatedAt)
            );
        }

        public async Task<PaginatorDto<IEnumerable<LevyPayment>>> SearchPayment(string searchString, PaginationFilter paginationFilter)
        {
            return await FindAll(false)
                           .Where(a => a.TransactionReference.Contains(searchString) ||
                           a.Trader.BusinessName.Contains(searchString) ||
                           a.GoodBoy.User.LastName.Contains(searchString) ||
                           a.GoodBoy.User.FirstName.Contains(searchString))
                           .Paginate(paginationFilter);
        }

        public async Task<decimal> GetTotalLeviesAsync()
        {
            return await FindAll(trackChanges: false)
                         .SumAsync(lp => lp.Amount);
        }

    }
}
