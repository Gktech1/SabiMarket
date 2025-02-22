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
        Task<BaseResponse<string>> CreateWaivedProduct(CreateWaivedProductDto dto);
        Task<BaseResponse<PaginatorDto<IEnumerable<WaivedProduct>>>> GetAllWaivedProducts(string? category, PaginationFilter filter);
        Task<BaseResponse<WaivedProduct>> GetWaivedProductById(string Id);
        Task<BaseResponse<string>> UpdateProduct(UpdateWaivedProductDto dto);
        Task<BaseResponse<PaginatorDto<IEnumerable<Vendor>>>> GetVendorAndProducts(PaginationFilter filter);
    }
}
