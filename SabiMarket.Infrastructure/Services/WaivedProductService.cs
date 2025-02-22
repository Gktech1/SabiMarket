using Microsoft.AspNetCore.Http;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.Interfaces;
using SabiMarket.Application.IRepositories;
using SabiMarket.Domain.Entities.WaiveMarketModule;
using SabiMarket.Domain.Exceptions;
using SabiMarket.Infrastructure.Utilities;
using Serilog;

namespace SabiMarket.Infrastructure.Services;


public class WaivedProductService : IWaivedProductService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IHttpContextAccessor _contextAccessor;
    public WaivedProductService(IRepositoryManager repositoryManager, IHttpContextAccessor contextAccessor)
    {
        _repositoryManager = repositoryManager;
        _contextAccessor = contextAccessor;
    }

    public async Task<BaseResponse<string>> CreateWaivedProduct(CreateWaivedProductDto dto)
    {
        try
        {
            var loggedInUser = Helper.GetUserDetails(_contextAccessor);
            var waivedProd = new WaivedProduct
            {
                IsAvailbleForUrgentPurchase = dto.IsAvailbleForUrgentPurchase,
                ImageUrl = dto.ImageUrl,
                ProductName = dto.ProductName,
                Price = dto.Price,
                Category = dto.Category,
                CurrencyType = dto.CurrencyType,
                VendorId = loggedInUser.Id
                //OriginalPrice = dto.OriginalPrice,
                //StockQuantity = dto.StockQuantity,
                //WaivedPrice = dto.WaivedPrice,
                //ProductCode = dto.Name + DateTime.Now.Ticks.ToString().Substring(-0, 5)
            };
            _repositoryManager.WaivedProductRepository.AddWaivedProduct(waivedProd);
            await _repositoryManager.SaveChangesAsync();
            return ResponseFactory.Success("Success", "Waive Product Created Successfully.");
        }
        catch (Exception)
        {

            return ResponseFactory.Fail<string>("An Error Occurred. Try again later");

        }
    }

    public async Task<BaseResponse<PaginatorDto<IEnumerable<WaivedProduct>>>> GetAllWaivedProducts(string? category, PaginationFilter filter)
    {
        var waivedProducts = await _repositoryManager.WaivedProductRepository.GetPagedWaivedProduct(category, filter);
        if (waivedProducts == null)
        {
            return ResponseFactory.Fail<PaginatorDto<IEnumerable<WaivedProduct>>>(new NotFoundException("No Record Found."), "Record not found.");
        }

        return ResponseFactory.Success<PaginatorDto<IEnumerable<WaivedProduct>>>(waivedProducts);
    }
    public async Task<BaseResponse<WaivedProduct>> GetWaivedProductById(string Id)
    {
        var waivedProduct = await _repositoryManager.WaivedProductRepository.GetWaivedProductById(Id, false);
        if (waivedProduct == null)
        {
            return ResponseFactory.Fail<WaivedProduct>(new NotFoundException("No Record Found."), "Record not found.");
        }

        return ResponseFactory.Success(waivedProduct);
    }

    public async Task<BaseResponse<string>> UpdateProduct(UpdateWaivedProductDto dto)
    {
        var product = await _repositoryManager.WaivedProductRepository.GetWaivedProductById(dto.ProductId, true);
        if (product == null)
        {
            return ResponseFactory.Fail<string>(new NotFoundException("No Record Found."), "Record not found.");
        }
        product.IsAvailbleForUrgentPurchase = dto.IsAvailbleForUrgentPurchase;
        product.ProductName = dto.ProductName;
        product.ImageUrl = dto.ImageUrl;
        product.Price = dto.Price;
        product.CurrencyType = dto.CurrencyType;
        product.UpdatedAt = DateTime.UtcNow.AddHours(1);
        product.Category = dto.Category;

        //product.StockQuantity = dto.StockQuantity;
        //product.OriginalPrice = dto.OriginalPrice;
        //product.WaivedPrice = dto.WaivedPrice;
        //product.Description = dto.Description;
        await _repositoryManager.SaveChangesAsync();
        return ResponseFactory.Success("Success");

    }

    public async Task<BaseResponse<PaginatorDto<IEnumerable<Vendor>>>> GetVendorAndProducts(PaginationFilter filter)
    {
        try
        {
            var vendors = await _repositoryManager.VendorRepository.GetVendorsWithPagination(filter, false);
            if (vendors == null)
            {
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<Vendor>>>(new NotFoundException("No Record Found."), "Record not found.");
            }

            return ResponseFactory.Success(vendors);

        }
        catch (Exception ex)
        {
            Log.Error("Exception occured while getting vendors" + ex.Message);
            return ResponseFactory.Fail<PaginatorDto<IEnumerable<Vendor>>>(new Exception("An Error occured."), "Try again later.");

        }
    }

}
