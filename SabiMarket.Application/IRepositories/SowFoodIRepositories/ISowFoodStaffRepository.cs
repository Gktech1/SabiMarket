using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SabiMarket.Application.DTOs;
using SabiMarket.Domain.Entities.SowFoodLinkUp;

namespace SabiMarket.Application.IRepositories.SowFoodIRepositories
{
    public interface ISowFoodStaffRepository
    {
        void AddStaff(SowFoodCompanyStaff product);
        void UpdateStaff(SowFoodCompanyStaff staff);
        void DeleteStaff(SowFoodCompanyStaff staff);
        Task<SowFoodCompanyStaff> GetStaffById(string id, bool trackChanges);
        Task<PaginatorDto<IEnumerable<SowFoodCompanyStaff>>> GetPagedStaff(PaginationFilter paginationFilter);
        Task<PaginatorDto<IEnumerable<SowFoodCompanyStaff>>> SearchStaff(string searchString, PaginationFilter paginationFilter);
        Task<IEnumerable<SowFoodCompanyStaff>> GetAllStaffForExport(bool trackChanges);
    }
    
    public interface ISowFoodStaffRepository
    {
        void AddStaff(SowFoodCompanyStaff product);
        void UpdateStaff(SowFoodCompanyStaff staff);
        void DeleteStaff(SowFoodCompanyStaff staff);
        Task<SowFoodCompanyStaff> GetStaffById(string id, bool trackChanges);
        Task<PaginatorDto<IEnumerable<SowFoodCompanyStaff>>> GetPagedStaff(PaginationFilter paginationFilter);
        Task<PaginatorDto<IEnumerable<SowFoodCompanyStaff>>> SearchStaff(string searchString, PaginationFilter paginationFilter);
        Task<IEnumerable<SowFoodCompanyStaff>> GetAllStaffForExport(bool trackChanges);
    }
}
