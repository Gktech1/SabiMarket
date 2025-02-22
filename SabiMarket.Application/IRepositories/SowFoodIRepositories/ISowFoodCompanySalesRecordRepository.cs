using SabiMarket.Application.DTOs;
using SabiMarket.Domain.Entities.SowFoodLinkUp;

namespace SabiMarket.Application.IRepositories.SowFoodIRepositories
{
    public interface ISowFoodCompanySalesRecordRepository
    {
        void AddCompanySalesRecord(SowFoodCompanySalesRecord salesRecord);
        void UpdateCompanySalesRecord(SowFoodCompanySalesRecord salesRecord);
        void DeleteCompanySalesRecord(string companyId, string salesRecordId);
        Task<SowFoodCompanySalesRecord> GetCompanySalesRecordById(string id, string companyId, bool trackChanges);
        Task<PaginatorDto<IEnumerable<SowFoodCompanySalesRecord>>> GetPagedCompanySalesRecord(string companyId, PaginationFilter paginationFilter);
        Task<PaginatorDto<IEnumerable<SowFoodCompanySalesRecord>>> SearchCompanySalesRecord(string searchString, string companyId, PaginationFilter paginationFilter);
        Task<IEnumerable<SowFoodCompanySalesRecord>> GetAllCompanySalesRecordForExport(string companyId, bool trackChanges);
    }
}
