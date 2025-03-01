using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.Interfaces;
using SabiMarket.Application.IRepositories;
using SabiMarket.Domain.Entities.OrdersAndFeedback;
using SabiMarket.Domain.Entities.Supporting;
using SabiMarket.Domain.Entities.WaiveMarketModule;
using SabiMarket.Domain.Exceptions;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Utilities;
using Serilog;

namespace SabiMarket.Infrastructure.Services;


public class WaivedProductService : IWaivedProductService
{
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly IRepositoryManager _repositoryManager;
    private readonly IHttpContextAccessor _contextAccessor;
    public WaivedProductService(IRepositoryManager repositoryManager, IHttpContextAccessor contextAccessor, ApplicationDbContext applicationDbContext)
    {
        _repositoryManager = repositoryManager;
        _contextAccessor = contextAccessor;
        _applicationDbContext = applicationDbContext;
    }

    public async Task<BaseResponse<string>> CreateWaivedProduct(CreateWaivedProductDto dto)
    {
        try
        {
            var loggedInUser = Helper.GetUserDetails(_contextAccessor);

            var verifyVendor = ValiateVendor(loggedInUser.Id);
            if (!verifyVendor.Item1)
            {
                return ResponseFactory.Fail<string>(verifyVendor.Item2);

            }
            var validateCategory = await _applicationDbContext.ProductCategories.AnyAsync(x => x.Id.ToLower() == dto.CategoryId.ToLower());
            if (!validateCategory)
            {
                return ResponseFactory.Fail<string>("Product category is not valid.");

            }
            var waivedProd = new WaivedProduct
            {
                IsAvailbleForUrgentPurchase = dto.IsAvailbleForUrgentPurchase,
                ImageUrl = dto.ImageUrl,
                ProductName = dto.ProductName,
                Price = dto.Price,
                ProductCategoryId = dto.CategoryId,
                CurrencyType = dto.CurrencyType,
                VendorId = verifyVendor.Item2,
                IsActive = true,
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
    private Tuple<bool, string> ValiateVendor(string userId)
    {
        var getVendorDetails = _applicationDbContext.Vendors.FirstOrDefault(x => x.UserId == userId);
        if (getVendorDetails is null || getVendorDetails.Id is null)
        {
            return Tuple.Create(false, "Vendor not found.");

        }
        if (!getVendorDetails.IsActive)
        {
            return Tuple.Create(false, "Vendor is disabled.");

        }
        if (!getVendorDetails.IsSubscriptionActive)
        {
            return Tuple.Create(false, "Vendor does not have an active subscription.");

        }
        if (!getVendorDetails.IsVerified)
        {
            return Tuple.Create(false, "Vendor is not verified.");

        }
        return Tuple.Create(true, getVendorDetails.Id);
    }

    /* public async Task<BaseResponse<PaginatorDto<IEnumerable<WaivedProduct>>>> GetAllWaivedProducts(
      string category,
      PaginationFilter paginationFilter)
     {
         var correlationId = Guid.NewGuid().ToString();
         try
         {
             var query = _applicationDbContext.WaivedProducts.AsQueryable();

             if (!string.IsNullOrEmpty(category))
             {
                 query = query.Where(p => p.ProductCategoryId == category);
             }

             // Use the Paginate extension method
             var paginatedResult = await query.Paginate(paginationFilter);
            *//* // Create paginated response directly from entity
             var paginatedResult = new PaginatorDto<IEnumerable<WaivedProduct>>
             {
                 PageItems = result.PageItems,
                 PageSize = result.PageSize,
                 CurrentPage = result.CurrentPage,
                 NumberOfPages = result.NumberOfPages
             };
 *//*

             return ResponseFactory.Success(paginatedResult, "Waived products retrieved successfully");
         }
         catch (Exception ex)
         {

             //_logger.LogError(ex, "Error retrieving waived products: {ErrorMessage}", ex.Message);

             return ResponseFactory.Fail<PaginatorDto<IEnumerable<WaivedProduct>>>(
                 ex, "An unexpected error occurred while retrieving waived products"
             );
         }
     }*/

    public async Task<BaseResponse<PaginatorDto<IEnumerable<WaivedProduct>>>> GetAllWaivedProducts(
    string category,
    PaginationFilter paginationFilter)
    {
        var correlationId = Guid.NewGuid().ToString();
        try
        {
            // Fetch the waived products using pagination
            var result = await GetWaivedProductsAsync(category, paginationFilter);

            // Map WaivedProduct to WaivedProductDto
            var waivedProductDtos = result.PageItems.Select(p => new WaivedProduct
            {
                Id = p.Id,
                ProductName = p.ProductName,
                ImageUrl = p.ImageUrl,
                Price = p.Price,
                IsAvailbleForUrgentPurchase = p.IsAvailbleForUrgentPurchase,
                CurrencyType = p.CurrencyType,
                VendorId = p.VendorId,
                ProductCategoryId = p.ProductCategoryId
            });

            var paginatedResult = new PaginatorDto<IEnumerable<WaivedProduct>>
            {
                PageItems = waivedProductDtos,
                PageSize = result.PageSize,
                CurrentPage = result.CurrentPage,
                NumberOfPages = result.NumberOfPages
            };


            return ResponseFactory.Success(paginatedResult, "Waived products retrieved successfully");
        }
        catch (Exception ex)
        {
            return ResponseFactory.Fail<PaginatorDto<IEnumerable<WaivedProduct>>>(ex,
                "An unexpected error occurred while retrieving waived products");
        }
    }

    private async Task<PaginatorDto<IEnumerable<WaivedProduct>>> GetWaivedProductsAsync(
   string category,
   PaginationFilter paginationFilter)
    {
        // Base query
        var query = _applicationDbContext.WaivedProducts.AsNoTracking();

        // Apply category filter if provided
        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(p => p.ProductCategoryId == category);
        }

        // Order by product name
        query = query.OrderBy(p => p.ProductName);

        // Apply pagination
        return await query.Paginate(paginationFilter);
    }

    /*public async Task<BaseResponse<PaginatorDto<IEnumerable<WaivedProduct>>>> GetAllWaivedProducts(string? category, PaginationFilter filter)
    {
        var waivedProducts = await _applicationDbContext.WaivedProducts.Paginate(fil);
        if (waivedProducts == null)
        {
            return ResponseFactory.Fail<PaginatorDto<IEnumerable<WaivedProduct>>>(new NotFoundException("No Record Found."), "Record not found.");
        }

        return ResponseFactory.Success(waivedProducts);
    }*/
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
        product.ProductCategoryId = dto.CategoryId;

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
    public async Task<BaseResponse<string>> RegisterCustomerPurchase(CustomerPurchaseDto dto)
    {
        try
        {
            var loggedInUser = Helper.GetUserDetails(_contextAccessor);

            var purchase = new CustomerPurchase
            {
                IsActive = true,
                ProofOfPayment = dto.ProofOfPayment,
                WaivedProductId = dto.WaivedProductId,
                DeliveryInfo = dto.DeliveryInfo,
            };
            _applicationDbContext.CustomerPurchases.Add(purchase);
            await _repositoryManager.SaveChangesAsync();
            return ResponseFactory.Success("Success", "Customer purchase created successfully.");

        }
        catch (Exception ex)
        {
            Log.Error("Exception occured while creating purchase record" + ex.Message);
            return ResponseFactory.Fail<string>("An Error Occurred. Try again later");


        }
    }
    public async Task<BaseResponse<string>> ConfirmCustomerPurchase(string id)
    {
        try
        {
            var loggedInUser = Helper.GetUserDetails(_contextAccessor);

            var purchase = _applicationDbContext.CustomerPurchases.Find(id);
            if (purchase == null)
            {
                return ResponseFactory.Fail<string>($"Purchase with the Id: {id} could not be found");
            }
            purchase.IsPaymentConfirmed = true;
            await _repositoryManager.SaveChangesAsync();
            return ResponseFactory.Success("Success", "Customer purchase created successfully.");

        }
        catch (Exception ex)
        {
            Log.Error("Exception occured while confirming purchase record" + ex.Message);
            return ResponseFactory.Fail<string>("An Error Occurred. Try again later");
        }
    }

    public async Task<BaseResponse<string>> CreateProductCategory(string categoryName, string description)
    {
        try
        {
            var loggedInUser = Helper.GetUserDetails(_contextAccessor);

            var category = new ProductCategory
            {
                IsActive = true,
                Name = categoryName,
                Description = description
            };
            _applicationDbContext.ProductCategories.Add(category);
            await _repositoryManager.SaveChangesAsync();
            return ResponseFactory.Success("Success", "Category created successfully.");

        }
        catch (Exception ex)
        {
            Log.Error("Exception occured while creating product category" + ex.Message);
            return ResponseFactory.Fail<string>("An Error Occurred. Try again later");
        }
    }

    public async Task<BaseResponse<List<ProductCategoryDto>>> GetAllProductCategories()
    {

        var productCategory = await _applicationDbContext.ProductCategories.Where(x => x.IsActive).Select(x => new ProductCategoryDto
        {
            CategoryId = x.Id,
            CategoryName = x.Name,
        }).ToListAsync();

        if (productCategory == null)
        {
            return ResponseFactory.Fail<List<ProductCategoryDto>>(new NotFoundException("No Record Found."), "Record not found.");
        }

        return ResponseFactory.Success(productCategory);
    }

    ///
    public async Task<BaseResponse<string>> CreateComplaint(string vendorId, string compalaint, string? imageUrl)
    {
        try
        {
            var loggedInUser = Helper.GetUserDetails(_contextAccessor);

            var vendor = _applicationDbContext.Vendors.FirstOrDefault(v => v.Id == vendorId);
            if (vendor == null)
            {
                return ResponseFactory.Fail<string>(new NotFoundException($"Vendor with Id: {vendorId} not Found."), "Vendor not found.");
            }
            var complaint = new CustomerFeedback
            {
                Comment = compalaint,
                CustomerId = loggedInUser.Id,
                Rating = 0,
                ImageUrl = imageUrl,
                VendorCode = vendor.VendorCode,
                VendorId = vendorId,
                IsActive = true
            };
            _applicationDbContext.CustomerFeedbacks.Add(complaint);
            await _repositoryManager.SaveChangesAsync();
            return ResponseFactory.Success("Success", "Feedback Created Successfully.");
        }
        catch (Exception)
        {

            return ResponseFactory.Fail<string>("An Error Occurred. Try again later");

        }
    }

    public async Task<BaseResponse<string>> UpdateComplaint(string complaintId, string vendorId, string complaintMsg, string? imageUrl)
    {
        var complaint = _applicationDbContext.CustomerFeedbacks.Find(complaintId);
        if (complaint == null)
        {
            return ResponseFactory.Fail<string>(new NotFoundException("No Record Found."), "Record not found.");
        }
        complaint.VendorId = vendorId;
        complaint.Comment = complaintMsg;
        complaint.ImageUrl = imageUrl;
        await _repositoryManager.SaveChangesAsync();
        return ResponseFactory.Success("Success");
    }
    public async Task<BaseResponse<string>> DeleteComplaint(string complaintId, string vendorId, string complaintMsg, string imageUrl)
    {
        var complaint = _applicationDbContext.CustomerFeedbacks.Find(complaintId);
        if (complaint == null)
        {
            return ResponseFactory.Fail<string>(new NotFoundException("No Record Found."), "Record not found.");
        }
        _applicationDbContext.CustomerFeedbacks.Remove(complaint);
        await _repositoryManager.SaveChangesAsync();
        return ResponseFactory.Success("Success");
    }

    public async Task<BaseResponse<PaginatorDto<IEnumerable<CustomerFeedback>>>> GetAllComplaint(PaginationFilter filter)
    {
        var cusComplaint = await _applicationDbContext.CustomerFeedbacks.Paginate(filter);
        if (cusComplaint == null)
        {
            return ResponseFactory.Fail<PaginatorDto<IEnumerable<CustomerFeedback>>>(new NotFoundException("No Record Found."), "Record not found.");
        }

        return ResponseFactory.Success(cusComplaint);
    }
    public async Task<BaseResponse<CustomerFeedback>> GetCustomerFeedbackById(string Id)
    {
        var cusComplaint = _applicationDbContext.CustomerFeedbacks.Find(Id);
        if (cusComplaint == null)
        {
            return ResponseFactory.Fail<CustomerFeedback>(new NotFoundException("No Record Found."), "Record not found.");
        }

        return ResponseFactory.Success(cusComplaint);
    }

}
