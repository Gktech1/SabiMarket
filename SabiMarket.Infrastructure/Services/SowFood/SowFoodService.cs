using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.DTOs.SowFoodDto;
using SabiMarket.Application.IRepositories;
using SabiMarket.Domain.Entities.SowFoodLinkUp;
using SabiMarket.Domain.Entities.WaiveMarketModule;
using SabiMarket.Domain.Exceptions;
using SabiMarket.Infrastructure.Utilities;
using Serilog;

namespace SabiMarket.Infrastructure.Services.SowFood
{
    public class SowFoodService
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly CustomIdGenerator _customIdGenerator;

        public SowFoodService(IRepositoryManager repositoryManager, IHttpContextAccessor httpContextAccessor, CustomIdGenerator customIdGenerator)
        {
            _repositoryManager = repositoryManager;
            _httpContextAccessor = httpContextAccessor;
            _customIdGenerator = customIdGenerator;
        }
        #region Sow Food Customer
        public async Task<BaseResponse<string>> AddCompanyCustomer(CreateSowFoodCompanyCustomerDto dto)
        {
            try
            {
                var loggedInUser = Helper.GetUserDetails(_httpContextAccessor);
                var customer = new SowFoodCompanyCustomer
                {
                    SowFoodCompanyId = dto.SowFoodCompanyId,
                    FullName = dto.FullName,
                    PhoneNumber = dto.PhoneNumber,
                    EmailAddress = dto.EmailAddress,
                    RegisteredById = loggedInUser.Id
                };

                _repositoryManager.SowFoodCompanyCustomerRepository.AddCompanyCustomer(customer);
                await _repositoryManager.SaveChangesAsync();
                return ResponseFactory.Success("Success", "Customer added successfully.");
            }
            catch (Exception ex)
            {
                return ResponseFactory.Fail<string>(ex, "An error occurred while adding the customer.");
            }
        }
        public async Task<BaseResponse<string>> UpdateCompanyCustomer(UpdateSowFoodCompanyCustomerDto dto)
        {
            try
            {
                var customer = await _repositoryManager.SowFoodCompanyCustomerRepository.GetCompanyCustomerById(dto.CustomerId, dto.SowFoodCompanyId, true);
                if (customer == null)
                {
                    return ResponseFactory.Fail<string>(new NotFoundException("No Record Found."), "Record not found.");
                }

                var loggedInUser = Helper.GetUserDetails(_httpContextAccessor);
                customer.SowFoodCompanyId = dto.SowFoodCompanyId;
                customer.FullName = dto.FullName;
                customer.PhoneNumber = dto.PhoneNumber;
                customer.EmailAddress = dto.EmailAddress;
                customer.RegisteredById = loggedInUser.Id;

                await _repositoryManager.SaveChangesAsync();
                return ResponseFactory.Success("Success", "Customer updated successfully.");
            }
            catch (Exception ex)
            {
                Log.Warning($"Error from UpdateCompanyCustomer===> {ex.Message}");
                return ResponseFactory.Fail<string>(ex, "An error occurred while updating the customer.");
            }
        }
        public async Task<BaseResponse<string>> DeleteCompanyCustomer(string id, string companyId)
        {
            try
            {
                var customer = await _repositoryManager.SowFoodCompanyCustomerRepository.GetCompanyCustomerById(id, companyId, true);
                if (customer == null)
                {
                    return ResponseFactory.Fail<string>(new NotFoundException("No Record Found."), "Record not found.");
                }

                _repositoryManager.SowFoodCompanyCustomerRepository.DeleteCompanyCustomer(customer);
                await _repositoryManager.SaveChangesAsync();
                return ResponseFactory.Success("Success", "Customer deleted successfully.");
            }
            catch (Exception ex)
            {
                return ResponseFactory.Fail<string>(ex, "An error occurred while deleting the customer.");
            }
        }
        public async Task<BaseResponse<GetSowFoodCompanyCustomerDto>> GetCompanyCustomerById(string customerId, string companyId)
        {
            try
            {
                var customer = await _repositoryManager.SowFoodCompanyCustomerRepository.GetCompanyCustomerById(customerId, companyId, false);
                if (customer == null)
                {
                    return ResponseFactory.Fail<GetSowFoodCompanyCustomerDto>(new NotFoundException("No Record Found."), "Record not found.");
                }
                var customerDetails = new GetSowFoodCompanyCustomerDto
                {
                    EmailAddress = customer.EmailAddress,
                    Name = customer.FullName,
                    PhoneNumber = customer.PhoneNumber,
                    BoughtItems = customer?.SowFoodCompanySalesRecords?.Select(boughtItem => new BoughtItemDto
                    {
                        Amount = boughtItem.TotalPrice,
                        ItemName = boughtItem.SowFoodCompanyProductItem.Name,
                        PurchaseDate = boughtItem.CreatedAt
                    }).ToList()
                };
                return ResponseFactory.Success(customerDetails);
            }
            catch (Exception ex)
            {
                return ResponseFactory.Fail<GetSowFoodCompanyCustomerDto>(ex, "An error occurred while retrieving the customer.");
            }
        }
        public async Task<BaseResponse<PaginatorDto<IEnumerable<GetSowFoodCompanyCustomerDto>>>> GetPagedCompanyCustomer(PaginationFilter filter)
        {
            try
            {
                // Get paginated customers from the repository
                var customers = await _repositoryManager.SowFoodCompanyCustomerRepository.GetPagedCompanyCustomer(filter);
                if (customers == null || !customers.PageItems.Any())
                {
                    return ResponseFactory.Fail<PaginatorDto<IEnumerable<GetSowFoodCompanyCustomerDto>>>(new NotFoundException("No Record Found."), "Record not found.");
                }

                // Map customer details
                var customerDetails = customers.PageItems.Select(customer => new GetSowFoodCompanyCustomerDto
                {
                    EmailAddress = customer.EmailAddress,
                    Name = customer.FullName,
                    PhoneNumber = customer.PhoneNumber,
                    BoughtItems = customer.SowFoodCompanySalesRecords?.Select(boughtItem => new BoughtItemDto
                    {
                        Amount = boughtItem.TotalPrice,
                        ItemName = boughtItem.SowFoodCompanyProductItem.Name,
                        PurchaseDate = boughtItem.CreatedAt
                    }).ToList() ?? new List<BoughtItemDto>()
                }).ToList();

                // Return paginated response
                var response = new PaginatorDto<IEnumerable<GetSowFoodCompanyCustomerDto>>
                {
                    PageItems = customerDetails,
                    PageSize = customers.PageSize,
                    CurrentPage = customers.CurrentPage,
                    NumberOfPages = customers.NumberOfPages
                };

                return ResponseFactory.Success(response);
            }
            catch (Exception ex)
            {
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<GetSowFoodCompanyCustomerDto>>>(ex, "An error occurred while retrieving the paged customers.");
            }
        }

        public async Task<BaseResponse<PaginatorDto<IEnumerable<GetSowFoodCompanyCustomerDto>>>> SearchCompanyCustomer(string searchString, PaginationFilter filter)
        {
            try
            {
                // Get paginated search results from the repository
                var customers = await _repositoryManager.SowFoodCompanyCustomerRepository.SearchCompanyCustomer(searchString, filter);
                if (customers == null || !customers.PageItems.Any())
                {
                    return ResponseFactory.Fail<PaginatorDto<IEnumerable<GetSowFoodCompanyCustomerDto>>>(new NotFoundException("No Record Found."), "Record not found.");
                }

                // Map customer details
                var customerDetails = customers.PageItems.Select(customer => new GetSowFoodCompanyCustomerDto
                {
                    EmailAddress = customer.EmailAddress,
                    Name = customer.FullName,
                    PhoneNumber = customer.PhoneNumber,
                    BoughtItems = customer.SowFoodCompanySalesRecords?.Select(boughtItem => new BoughtItemDto
                    {
                        Amount = boughtItem.TotalPrice,
                        ItemName = boughtItem.SowFoodCompanyProductItem.Name,
                        PurchaseDate = boughtItem.CreatedAt
                    }).ToList() ?? new List<BoughtItemDto>()
                }).ToList();

                // Return paginated response
                var response = new PaginatorDto<IEnumerable<GetSowFoodCompanyCustomerDto>>
                {
                    PageItems = customerDetails,
                    PageSize = customers.PageSize,
                    CurrentPage = customers.CurrentPage,
                    NumberOfPages = customers.NumberOfPages
                };

                return ResponseFactory.Success(response);
            }
            catch (Exception ex)
            {
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<GetSowFoodCompanyCustomerDto>>>(ex, "An error occurred while searching for customers.");
            }
        }

        #endregion

        #region Sow Food Company Production Item
        public async Task<BaseResponse<string>> AddCompanyProductionItem(CreateSowFoodCompanyProductionItemDto dto)
        {
            try
            {
                Log.Information("Starting AddCompanyProductionItem method");
                var productionItem = new SowFoodCompanyProductionItem
                {
                    Name = dto.Name,
                    Quantity = dto.Quantity,
                    //UnitPrice = dto.UnitPrice,
                    ImageUrl = dto.ImageUrl,
                    SowFoodCompanyId = dto.SowFoodCompanyId
                };

                Log.Information("Adding production item to repository");
                _repositoryManager.SowFoodCompanyProductionItemRepository.AddCompanyProductionItem(productionItem);
                await _repositoryManager.SaveChangesAsync();

                Log.Information("Production item added successfully");
                return ResponseFactory.Success("Success", "Production item added successfully.");
            }
            catch (Exception ex)
            {
                Log.Information($"Error in AddCompanyProductionItem: {ex.Message}");
                return ResponseFactory.Fail<string>(ex, "An error occurred while adding the production item.");
            }
        }

        public async Task<BaseResponse<string>> UpdateCompanyProductionItem(UpdateSowFoodCompanyProductionItemDto dto)
        {
            try
            {
                Log.Information("Starting UpdateCompanyProductionItem method");
                var productionItem = await _repositoryManager.SowFoodCompanyProductionItemRepository.GetCompanyProductionItemById(dto.Id, dto.SowFoodCompanyId, true);
                if (productionItem == null)
                {
                    Log.Information("No record found for update");
                    return ResponseFactory.Fail<string>(new NotFoundException("No Record Found."), "Record not found.");
                }

                Log.Information("Updating production item");
                productionItem.Name = dto.Name;
                productionItem.Quantity = dto.Quantity;
                //productionItem.UnitPrice = dto.UnitPrice;
                productionItem.ImageUrl = dto.ImageUrl;
                productionItem.UpdatedAt = DateTime.UtcNow;

                await _repositoryManager.SaveChangesAsync();
                Log.Information("Production item updated successfully");
                return ResponseFactory.Success("Success", "Production item updated successfully.");
            }
            catch (Exception ex)
            {
                Log.Information($"Error in UpdateCompanyProductionItem: {ex.Message}");
                return ResponseFactory.Fail<string>(ex, "An error occurred while updating the production item.");
            }
        }

        public async Task<BaseResponse<string>> DeleteCompanyProductionItem(string id, string companyId)
        {
            try
            {
                Log.Information("Starting DeleteCompanyProductionItem method");
                var productionItem = await _repositoryManager.SowFoodCompanyProductionItemRepository.GetCompanyProductionItemById(id, companyId, true);
                if (productionItem == null)
                {
                    Log.Information("No record found for deletion");
                    return ResponseFactory.Fail<string>(new NotFoundException("No Record Found."), "Record not found.");
                }

                Log.Information("Deleting production item");
                _repositoryManager.SowFoodCompanyProductionItemRepository.DeleteCompanyProductionItem(productionItem);
                await _repositoryManager.SaveChangesAsync();

                Log.Information("Production item deleted successfully");
                return ResponseFactory.Success("Success", "Production item deleted successfully.");
            }
            catch (Exception ex)
            {
                Log.Information($"Error in DeleteCompanyProductionItem: {ex.Message}");
                return ResponseFactory.Fail<string>(ex, "An error occurred while deleting the production item.");
            }
        }

        public async Task<BaseResponse<GetSowFoodCompanyProductionItemDto>> GetCompanyProductionItemById(string id, string companyId)
        {
            try
            {
                Log.Information("Fetching production item by ID");
                var productionItem = await _repositoryManager.SowFoodCompanyProductionItemRepository.GetCompanyProductionItemById(id, companyId, false);
                if (productionItem == null)
                {
                    Log.Information("No record found");
                    return ResponseFactory.Fail<GetSowFoodCompanyProductionItemDto>(new NotFoundException("No Record Found."), "Record not found.");
                }

                Log.Information("Mapping production item details");
                var productionItemDetails = new GetSowFoodCompanyProductionItemDto
                {
                    Id = productionItem.Id,
                    Name = productionItem.Name,
                    Quantity = productionItem.Quantity,
                    //UnitPrice = productionItem.UnitPrice,
                    ImageUrl = productionItem.ImageUrl
                };

                return ResponseFactory.Success(productionItemDetails);
            }
            catch (Exception ex)
            {
                Log.Information($"Error in GetCompanyProductionItemById: {ex.Message}");
                return ResponseFactory.Fail<GetSowFoodCompanyProductionItemDto>(ex, "An error occurred while retrieving the production item.");
            }
        }
        public async Task<BaseResponse<PaginatorDto<IEnumerable<GetSowFoodCompanyProductionItemDto>>>> GetCompanyProduction(string id, string companyId)
        {
            try
            {
                Log.Information("Fetching production item by ID");
                var productionItem = await _repositoryManager.SowFoodCompanyProductionItemRepository.GetCompanyProductionItemById(id, companyId, false);
                if (productionItem == null)
                {
                    Log.Information("No record found");
                    return ResponseFactory.Fail<GetSowFoodCompanyProductionItemDto>(new NotFoundException("No Record Found."), "Record not found.");
                }

                Log.Information("Mapping production item details");
                var productionItemDetails = new GetSowFoodCompanyProductionItemDto
                {
                    Id = productionItem.Id,
                    Name = productionItem.Name,
                    Quantity = productionItem.Quantity,
                    //UnitPrice = productionItem.UnitPrice,
                    ImageUrl = productionItem.ImageUrl
                };

                return ResponseFactory.Success(productionItemDetails);
            }
            catch (Exception ex)
            {
                Log.Information($"Error in GetCompanyProductionItemById: {ex.Message}");
                return ResponseFactory.Fail<GetSowFoodCompanyProductionItemDto>(ex, "An error occurred while retrieving the production item.");
            }
        }
        #endregion


        #region Sales Record
        public async Task<BaseResponse<string>> AddCompanySalesRecord(CreateSowFoodCompanySalesRecordDto dto)
        {
            try
            {
                var logInUser = Helper.GetUserDetails(_httpContextAccessor);
                Log.Information("Starting AddCompanySalesRecord method");

                var salesRecord = new SowFoodCompanySalesRecord
                {
                    Quantity = dto.Quantity,
                    UnitPrice = dto.UnitPrice,
                    SowFoodCompanyProductItemId = dto.SowFoodCompanyProductItemId ?? null,
                    SowFoodCompanyShelfItemId = dto.SowFoodCompanyShelfItemId ?? null,
                    SowFoodCompanyCustomerId = dto.SowFoodCompanyCustomerId ?? null,
                    SowFoodCompanyStaffId = logInUser.Id
                };

                Log.Information("Checking if sales record is from Shelf Item or Production Item");

                if (!string.IsNullOrEmpty(dto.SowFoodCompanyShelfItemId))
                {
                    Log.Information("Fetching shelf item with ID {ShelfItemId}", dto.SowFoodCompanyShelfItemId);
                    var shelfItem = await _repositoryManager.SowFoodCompanyShelfItemRepository
                        .GetCompanyShelfItemById(dto.SowFoodCompanyShelfItemId, true);

                    if (shelfItem == null)
                    {
                        Log.Warning("Shelf item with ID {ShelfItemId} not found", dto.SowFoodCompanyShelfItemId);
                        return ResponseFactory.Fail<string>(new NotFoundException("Shelf item not found."), "Shelf item not found.");
                    }

                    if (shelfItem.Quantity < dto.Quantity)
                    {
                        Log.Warning("Not enough stock in shelf item. Available: {AvailableQuantity}, Requested: {RequestedQuantity}",
                            shelfItem.Quantity, dto.Quantity);
                        return ResponseFactory.Fail<string>(new Exception("Not enough stock."), "Not enough stock.");
                    }

                    shelfItem.Quantity -= dto.Quantity;
                    Log.Information("Shelf item quantity updated. Remaining: {RemainingQuantity}", shelfItem.Quantity);
                }
                else if (!string.IsNullOrEmpty(dto.SowFoodCompanyProductItemId))
                {
                    Log.Information("Fetching production item with ID {ProductItemId}", dto.SowFoodCompanyProductItemId);
                    var productionItem = await _repositoryManager.SowFoodCompanyProductionItemRepository
                        .GetCompanyProductionItemById(dto.SowFoodCompanyProductItemId, dto.SowFoodCompanyId, true);

                    if (productionItem == null)
                    {
                        Log.Warning("Production item with ID {ProductItemId} not found", dto.SowFoodCompanyProductItemId);
                        return ResponseFactory.Fail<string>(new NotFoundException("Production item not found."), "Production item not found.");
                    }

                    if (productionItem.Quantity < dto.Quantity)
                    {
                        Log.Warning("Not enough stock in production item. Available: {AvailableQuantity}, Requested: {RequestedQuantity}",
                            productionItem.Quantity, dto.Quantity);
                        return ResponseFactory.Fail<string>(new Exception("Not enough stock."), "Not enough stock.");
                    }

                    productionItem.Quantity -= dto.Quantity;
                    Log.Information("Production item quantity updated. Remaining: {RemainingQuantity}", productionItem.Quantity);
                }
                else
                {
                    Log.Error("No valid item source provided for sales record");
                    return ResponseFactory.Fail<string>(new Exception("Invalid sales record"), "Invalid sales record.");
                }

                Log.Information("Adding sales record to repository");
                _repositoryManager.SowFoodCompanySalesRecordRepository.AddCompanySalesRecord(salesRecord);
                await _repositoryManager.SaveChangesAsync();

                Log.Information("Sales record added successfully");
                return ResponseFactory.Success("Success", "Sales record added successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in AddCompanySalesRecord: {ErrorMessage}", ex.Message);
                return ResponseFactory.Fail<string>(ex, "An error occurred while adding the sales record.");
            }
        }

        public async Task<BaseResponse<string>> UpdateCompanySalesRecord(SowFoodCompanySalesRecord salesRecord)
        {
            try
            {
                Log.Information("Updating sales record with ID {SalesRecordId}", salesRecord.Id);

                var existingRecord = await _repositoryManager.SowFoodCompanySalesRecordRepository
                    .GetCompanySalesRecordById(salesRecord.Id, salesRecord.SowFoodCompanyId, true);

                if (existingRecord == null)
                {
                    Log.Warning("Sales record with ID {SalesRecordId} not found", salesRecord.Id);
                    return ResponseFactory.Fail<string>(new NotFoundException("Sales record not found"), "Sales record not found.");
                }

                existingRecord.Quantity = salesRecord.Quantity;
                existingRecord.UnitPrice = salesRecord.UnitPrice;

                await _repositoryManager.SaveChangesAsync();

                Log.Information("Sales record updated successfully");
                return ResponseFactory.Success("Success", "Sales record updated successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating sales record: {ErrorMessage}", ex.Message);
                return ResponseFactory.Fail<string>(ex, "An error occurred while updating the sales record.");
            }
        }

        public void DeleteCompanySalesRecord(string salesRecordId, string companyId)
        {
            try
            {
                Log.Information("Deleting sales record with ID {SalesRecordId}", salesRecordId);
                _repositoryManager.SowFoodCompanySalesRecordRepository.DeleteCompanySalesRecord(companyId, salesRecordId);
                _repositoryManager.SaveChangesAsync();
                Log.Information("Sales record deleted successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting sales record: {ErrorMessage}", ex.Message);
            }
        }



        #endregion
    }
}