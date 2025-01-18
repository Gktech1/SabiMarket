using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SabiMarket.Application.DTOs;
using SabiMarket.Domain.Entities.LevyManagement;

namespace SabiMarket.Application.IRepositories
{
    public interface ILevyPaymentRepository
    {
        void AddPayment(LevyPayment levyPayment);
        Task<IEnumerable<LevyPayment>> GetAllLevyPaymentForExport(bool trackChanges);
        Task<PaginatorDto<IEnumerable<LevyPayment>>> GetLevyPaymentsAsync(
        string chairmanId, PaginationFilter paginationFilter, bool trackChanges);
        Task<LevyPayment> GetPaymentById(string id, bool trackChanges);
        Task<PaginatorDto<IEnumerable<LevyPayment>>> GetPagedPayment(int? period, PaginationFilter paginationFilter);
        Task<PaginatorDto<IEnumerable<LevyPayment>>> SearchPayment(string searchString, PaginationFilter paginationFilter);
        Task<decimal> GetTotalLeviesAsync();
        void DeleteLevyPayment(LevyPayment levy);
    }
}
