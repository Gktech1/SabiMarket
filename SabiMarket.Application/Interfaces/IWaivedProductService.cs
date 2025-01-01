using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Domain.Entities.WaiveMarketModule;

namespace SabiMarket.Application.Interfaces
{
    public interface IWaivedProductService
    {
        Task<Result> CreateWaivedProduct(CreateWaivedProductDto dto);
        Task<Result<PaginatorDto<IEnumerable<WaivedProduct>>>> GetAllWaivedProducts(string? category, PaginationFilter filter);
        Task<Result<WaivedProduct>> GetWaivedProductById(string Id);
        Task<Result> UpdateProduct(UpdateWaivedProductDto dto);
    }
}
