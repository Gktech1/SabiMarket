using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Domain.Entities.OrdersAndFeedback;
using SabiMarket.Domain.Entities.WaiveMarketModule;

namespace SabiMarket.Application.Interfaces
{
    public interface IWaivedProductService
    {
        Task<BaseResponse<string>> CreateWaivedProduct(CreateWaivedProductDto dto);
        Task<BaseResponse<PaginatorDto<IEnumerable<WaivedProduct>>>> GetAllWaivedProducts(string? category, PaginationFilter filter);
        Task<BaseResponse<WaivedProduct>> GetWaivedProductById(string Id);
        Task<BaseResponse<string>> UpdateProduct(UpdateWaivedProductDto dto);
        Task<BaseResponse<PaginatorDto<IEnumerable<VendorDto>>>> GetVendorAndProducts(PaginationFilter filter);
        Task<BaseResponse<string>> ConfirmCustomerPurchase(string id);
        Task<BaseResponse<string>> RegisterCustomerPurchase(CustomerPurchaseDto dto);
        Task<BaseResponse<List<ProductCategoryDto>>> GetAllProductCategories();
        Task<BaseResponse<string>> CreateProductCategory(string categoryName, string description);
        Task<BaseResponse<string>> DeleteProductCategory(string id);
        Task<BaseResponse<string>> CreateComplaint(string vendorId, string compalaint, string? imageUrl);
        Task<BaseResponse<string>> UpdateComplaint(string complaintId, string vendorId, string complaintMsg, string? imageUrl);
        Task<BaseResponse<string>> DeleteComplaint(string complaintId);
        Task<BaseResponse<PaginatorDto<IEnumerable<CustomerFeedback>>>> GetAllComplaint(PaginationFilter filter);
        Task<BaseResponse<CustomerFeedback>> GetCustomerFeedbackById(string Id);
        Task<BaseResponse<string>> DeleteProduct(string waiveProductId);
        Task<BaseResponse<string>> BlockOrUnblockVendor(string userId);
        Task<BaseResponse<List<ProductDetailsDto>>> GetUrgentPurchaseWaivedProduct(PaginationFilter filter);
        public Task<BaseResponse<int>> GetUrgentPurchaseWaivedProductCount();
    }
}
