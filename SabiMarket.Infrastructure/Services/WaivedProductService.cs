using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.Interfaces;
using SabiMarket.Application.IRepositories;
using SabiMarket.Domain.Entities.WaiveMarketModule;
using SabiMarket.Domain.Exceptions;

namespace SabiMarket.Infrastructure.Services;


public class WaivedProductService : IWaivedProductService
{
    private readonly IRepositoryManager _repositoryManager;
    public WaivedProductService(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
    }

    public async Task<BaseResponse<string>> CreateWaivedProduct(CreateWaivedProductDto dto)
    {
        var waivedProd = new WaivedProduct
        {
            IsAvailbleForUrgentPurchase = dto.IsAvailbleForUrgentPurchase,
            ImageUrl = dto.ImageUrl,
            Name = dto.Name,
            StockQuantity = dto.StockQuantity,
            VendorId = dto.VendorId,
            WaivedPrice = dto.WaivedPrice,
            OriginalPrice = dto.OriginalPrice,
            Description = dto.Description,
            ProductCode = dto.Name + DateTime.Now.Ticks.ToString().Substring(-0, 5)
        };

        _repositoryManager.WaivedProductRepository.AddWaivedProduct(waivedProd);
        await _repositoryManager.SaveChangesAsync();
        return ResponseFactory.Success("Success", "Waive Product Created Successfully.");
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
        product.StockQuantity = dto.StockQuantity;
        product.OriginalPrice = dto.OriginalPrice;
        product.WaivedPrice = dto.WaivedPrice;
        product.Description = dto.Description;
        product.IsAvailbleForUrgentPurchase = dto.IsAvailbleForUrgentPurchase;
        product.Name = dto.Name;
        product.ImageUrl = dto.ImageUrl;

        await _repositoryManager.SaveChangesAsync();
        return ResponseFactory.Success("Success");

    }

}
