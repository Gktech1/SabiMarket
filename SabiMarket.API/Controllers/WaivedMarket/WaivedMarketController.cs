﻿using System;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using SabiMarket.API.ServiceExtensions;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.PaymentsDto;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.Interfaces;
using SabiMarket.Infrastructure.Services;

namespace SabiMarket.API.Controllers.WaivedMarket
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class WaivedMarketController : ControllerBase    //[Authorize]

    {
        private readonly IServiceManager _serviceManager;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IPayments _paymentService;
        public WaivedMarketController(IServiceManager serviceManager, ICloudinaryService cloudinaryService, IPayments paymentService)
        {
            _serviceManager = serviceManager;
            _cloudinaryService = cloudinaryService;
            _paymentService = paymentService;
        }

        [HttpGet("GetWaivedProductById")]
        public async Task<IActionResult> GetWaivedProductById([FromQuery] string id)
        {
            var response = await _serviceManager.IWaivedProductService.GetWaivedProductById(id);
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpGet("GetWaivedProducts")]
        public async Task<IActionResult> GetWaivedProducts([FromQuery] string? category, [FromQuery] PaginationFilter filter)
        {
            var response = await _serviceManager.IWaivedProductService.GetAllWaivedProducts(category, filter);
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpGet("GetUrgentPurchaseWaivedProduct")]
        public async Task<IActionResult> GetUrgentPurchaseWaivedProduct([FromQuery] PaginationFilter filter, [FromQuery] string? searchString)
        {
            var response = await _serviceManager.IWaivedProductService.GetUrgentPurchaseWaivedProduct(filter, searchString);
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpGet("GetUrgentPurchaseWaivedProductCount")]
        public IActionResult GetUrgentPurchaseWaivedProductCount()
        {
            var response = _serviceManager.IWaivedProductService.GetUrgentPurchaseWaivedProductCount();
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpPost("CreateWaivedProducts")]
        public async Task<IActionResult> CreateWaivedProducts(CreateWaivedProductDto dto)
        {
            var response = await _serviceManager.IWaivedProductService.CreateWaivedProduct(dto);
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpPut("UpdateWaivedProducts")]
        public async Task<IActionResult> UpdateWaivedProducts(UpdateWaivedProductDto dto)
        {
            var response = await _serviceManager.IWaivedProductService.UpdateProduct(dto);
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpDelete("DeleteWaivedProducts")]
        public async Task<IActionResult> DeleteWaivedProducts(IdModel dto)
        {
            var response = await _serviceManager.IWaivedProductService.DeleteProduct(dto.id);
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpPost("UploadFile")]
        public async Task<IActionResult> UploadFiles(IFormFile file)
        {
            var response = await _cloudinaryService.UploadImage(file, "SabiMaket");
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpPost("CreateSubscription")]
        public async Task<IActionResult> CreateSubscription(CreateSubscriptionDto dto)
        {
            var response = await _serviceManager.ISubscriptionService.CreateSubscription(dto);
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }


        [HttpGet("CheckActiveVendorSubscription")]
        [Authorize(Policy = PolicyNames.RequiredVendorCustomerAndAdmin)]
        public async Task<IActionResult> CheckActiveVendorSubscription([FromQuery] string userId)
        {
            var response = await _serviceManager.ISubscriptionService.CheckActiveVendorSubscription(userId);
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpGet("CheckActiveCustomerSubscription")]
        [Authorize(Policy = PolicyNames.RequiredVendorCustomerAndAdmin)]
        public async Task<IActionResult> CheckActiveCustomerSubscription([FromQuery] string userId)
        {
            var response = await _serviceManager.ISubscriptionService.CheckActiveCustomerSubscription(userId);
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }
        [Authorize(Policy = PolicyNames.RequireAdminOnly)]
        [HttpPost("AdminConfirmSubscriptionPayment")]
        public async Task<IActionResult> AdminConfirmSubscriptionPayment(IdModel dto)
        {
            var response = await _serviceManager.ISubscriptionService.AdminConfirmSubscriptionPayment(dto.id);
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpPost("UserConfirmSubscriptionPayment")]
        public async Task<IActionResult> UserConfirmSubscriptionPayment(IdModel dto)
        {
            var response = await _serviceManager.ISubscriptionService.UserConfirmSubscriptionPayment(dto.id);
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        //[HttpGet("GetSubscriptionById")]
        //public async Task<IActionResult> GetSubscriptionById([FromQuery] string subscriptionId)
        //{
        //    var response = await _serviceManager.ISubscriptionService.GetSubscriptionById(subscriptionId);
        //    if (!response.IsSuccessful)
        //    {
        //        // Handle different types of registration failures
        //        return response.Error?.StatusCode switch
        //        {
        //            StatusCodes.Status400BadRequest => BadRequest(response),
        //            StatusCodes.Status409Conflict => Conflict(response),
        //            _ => BadRequest(response)
        //        };
        //    }

        //    return Ok(response);
        //}

        [HttpGet("GetAllSubscription")]
        public async Task<IActionResult> GetAllSubscription([FromQuery] PaginationFilter filter, [FromQuery] string? searchString)
        {
            var response = await _serviceManager.ISubscriptionService.GetAllSubscription(filter, searchString);
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpGet(" SearchSubscription")]
        public async Task<IActionResult> SearchSubscription([FromQuery] string searchString, [FromQuery] PaginationFilter filter)
        {
            var response = await _serviceManager.ISubscriptionService.SearchSubscription(searchString, filter);
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }
            return Ok(response);
        }

        [HttpGet(" SubscriptionDashBoardDetails")]
        public async Task<IActionResult> SubscriptionDashBoardDetails()
        {
            var response = await _serviceManager.ISubscriptionService.SubscriptionDashBoardDetails();
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }
            return Ok(response);
        }

        [HttpPost("CreateSubscriptionPlan")]
        public async Task<IActionResult> CreateSubscriptionPlan(CreateSubscriptionPlanDto dto)
        {
            var response = await _serviceManager.ISubscriptionPlanService.CreateSubscriptionPlan(dto);
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpPost("UpdateSubscriptionPlan")]
        public async Task<IActionResult> UpdateSubscriptionPlan(UpdateSubscriptionPlanDto dto)
        {
            var response = await _serviceManager.ISubscriptionPlanService.UpdateSubscriptionPlan(dto);
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpPost("DeleteSubscriptionPlan")]
        public async Task<IActionResult> DeleteSubscriptionPlan(string Id)
        {
            var response = await _serviceManager.ISubscriptionPlanService.DeleteSubscriptionPlan(Id);
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpGet("GetAllSubscriptionPlans")]
        public async Task<IActionResult> GetAllSubscriptionPlans([FromQuery] PaginationFilter filter, [FromQuery] string? searchString, [FromQuery] string? filterString, [FromQuery] string? frequencyFilter, [FromQuery] DateTime? dateCreatedFilter)
        {
            BaseResponse<PaginatorDto<IEnumerable<GetSubscriptionDto>>>? response = await _serviceManager.ISubscriptionPlanService.GetAllSubscriptionPlans(filter, searchString, frequencyFilter, dateCreatedFilter);
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpPost("GetSubscriptionPlanById")]
        public async Task<IActionResult> GetSubscriptionPlanById(IdModel dto)
        {
            var response = await _serviceManager.ISubscriptionPlanService.GetSubscriptionPlanById(dto.id);
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpGet("GetVendorAndProducts")]
        [Authorize(Policy = PolicyNames.RequiredVendorCustomerAndAdmin)]
        public async Task<IActionResult> GetVendorAndProducts([FromQuery] PaginationFilter filter, string? searchString, string? filterString)
        {
            BaseResponse<PaginatorDto<IEnumerable<VendorDto>>>? response = await _serviceManager.IWaivedProductService.GetVendorAndProducts(filter, searchString, filterString);
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpGet("GetSubscribersBySubscriptionPlan")]
        [Authorize(Policy = PolicyNames.RequiredVendorCustomerAndAdmin)]
        public async Task<IActionResult> GetSubscribersBySubscriptionPlan([FromQuery] PaginationFilter filter, [FromQuery] string subscriptionPlanId, [FromQuery] DateTime? createdAtFilter, int? currencyTypeFilter, string? statusFilter)
        {
            var response = await _serviceManager.ISubscriptionPlanService.GetSubscribersBySubscriptionPlan(filter, subscriptionPlanId, createdAtFilter, currencyTypeFilter, statusFilter);
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpGet("GetAllSubscriptionPlansS")]
        [Authorize(Policy = PolicyNames.RequiredVendorCustomerAndAdmin)]
        public async Task<IActionResult> GetAllSubscriptionPlansS([FromQuery] PaginationFilter filter, [FromQuery] string subscriptionPlanId)
        {
            var response = await _serviceManager.ISubscriptionPlanService.GetAllSubscriptionPlans(filter);
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpPost("BlockOrUnblockVendor")]
        [Authorize(Policy = PolicyNames.RequiredVendorCustomerAndAdmin)]
        public async Task<IActionResult> BlockOrUnblockVendor(IdModel dto)
        {
            var response = await _serviceManager.IWaivedProductService.BlockOrUnblockVendor(dto.id);
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpPost("BlockOrUnblockCustomer")]
        [Authorize(Policy = PolicyNames.RequiredVendorCustomerAndAdmin)]
        public async Task<IActionResult> BlockOrUnblockCustomer(IdModel dto)
        {
            var response = await _serviceManager.IWaivedProductService.BlockOrUnblockCustomer(dto.id);
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpPost("RegisterCustomerPurchase")]
        public async Task<IActionResult> RegisterCustomerPurchase(CustomerPurchaseDto dto)
        {
            var response = await _serviceManager.IWaivedProductService.RegisterCustomerPurchase(dto);
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpPost("ConfirmCustomerPurchase")]
        public async Task<IActionResult> ConfirmCustomerPurchase(IdModel dto)
        {
            var response = await _serviceManager.IWaivedProductService.ConfirmCustomerPurchase(dto.id);
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpPost("CreateProductCategory")]
        public async Task<IActionResult> CreateProductCategory(CreateProductCategory dto)
        {
            var response = await _serviceManager.IWaivedProductService.CreateProductCategory(dto.categoryName, dto.description);
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpGet("GetAllProductCategories")]
        public async Task<IActionResult> GetAllProductCategories()
        {
            var response = await _serviceManager.IWaivedProductService.GetAllProductCategories();
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpPost("CreateCustomerComplaint")]
        public async Task<IActionResult> CreateComplaint(CreateCustomerComplaint dto)
        {
            var response = await _serviceManager.IWaivedProductService.CreateComplaint(dto.vendorId, dto.comPlaintMsg, dto.imageUrl);
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpPut("SetComplaintAsResolved")]
        public async Task<IActionResult> SetComplaintAsResolved(string complaintId)
        {
            var response = await _serviceManager.IWaivedProductService.SetComplaintAsResolved(complaintId);
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpPost("ApproveDollarVendor")]
        [Authorize(Policy = PolicyNames.RequireAdminOnly)]
        public async Task<IActionResult> ApproveDollarVendor(string vendorId)
        {
            var response = await _serviceManager.IWaivedProductService.ApproveDollarVendor(vendorId);
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpPost("UpdateCustomerComplaint")]
        public async Task<IActionResult> UpdateComplaint(UpdateCustomerComplaint dto)
        {
            var response = await _serviceManager.IWaivedProductService.UpdateComplaint(dto.complaintId, dto.vendorId, dto.comPlaintMsg, dto.imageUrl);
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpGet("GetCustomerComplaintById")]
        public async Task<IActionResult> UpdateComplaint([FromQuery] string complaintId)
        {
            var response = await _serviceManager.IWaivedProductService.GetCustomerFeedbackById(complaintId);
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpGet("GetCustomerComplaints")]
        public async Task<IActionResult> GetCustomerComplaint([FromQuery] PaginationFilter filter, [FromQuery] string? searchString, [FromQuery] string? filterSting)
        {
            var response = await _serviceManager.IWaivedProductService.GetAllComplaint(filter, searchString, filterSting);
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpGet("GetResolvedCustomerComplaints")]
        public async Task<IActionResult> GetResolvedCustomerComplaint([FromQuery] PaginationFilter filter)
        {
            var response = await _serviceManager.IWaivedProductService.GetResolvedComplaint(filter);
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }
        [HttpGet("GetCustomers")]
        public async Task<IActionResult> GetCustomers([FromQuery] PaginationFilter filter, [FromQuery] string? searchString, [FromQuery] string? filterString)
        {
            var response = await _serviceManager.IWaivedProductService.GetCustomers(filter, searchString, filterString);
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpDelete("DeleteCustomerComplaint")]
        public async Task<IActionResult> DeleteCustomerComplaint(DeleteProductCategory dto)
        {
            var response = await _serviceManager.IWaivedProductService.DeleteComplaint(dto.complaintId);
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }
        [HttpDelete("DeleteProductCategory")]
        public async Task<IActionResult> DeleteProductCategory(DeleteProductCategory dto)
        {
            var response = await _serviceManager.IWaivedProductService.DeleteProductCategory(dto.complaintId);
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }
        [HttpPost("CreateNextWaiveMarketDate")]
        public async Task<IActionResult> CreateNextWaiveMarketDate(NextWaiveMarketDateDto dto)
        {
            var response = await _serviceManager.IWaivedProductService.CreateNextWaiveMarketDate(dto);
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }
        [HttpPost("UpdateNextWaiveMarketDate")]
        public async Task<IActionResult> UpdateNextWaiveMarketDate(UpdateNextWaiveMarketDateDto dto)
        {
            var response = await _serviceManager.IWaivedProductService.UpdateNextWaiveMarketDate(dto);
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }
        [HttpGet("GetNextWaiveMarketDate")]
        public async Task<IActionResult> GetNextWaiveMarketDate()
        {
            var response = await _serviceManager.IWaivedProductService.GetNextWaiveMarketDate();
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpPost("MakePaymentWithPaystack")]
        public async Task<IActionResult> MakePaymentWithPaystack(FundWalletVM fund)
        {
            var response = await _paymentService.Initialize(fund);
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpPost("VerifyPaymentWithPaystack")]
        public async Task<IActionResult> VerifyPaymentWithPaystack(PaymentRef paymentRef)
        {
            var response = await _paymentService.Verify(paymentRef.paymentRef);
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }


        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications()
        {
            var response = await _serviceManager.IWaivedProductService.GetNotificationsAsync();

            if (!response.Status)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("notifications/{notificationId}/can-proceed")]
        public async Task<IActionResult> CanProceedToPurchase(string notificationId, [FromBody] string vendorResponse)
        {
            var response = await _serviceManager.IWaivedProductService.CanProceedToPurchaseAsync(notificationId, vendorResponse);

            if (!response.Status)
            {
                return response.Message switch
                {
                    "Notification not found." => NotFound(response),
                    "Only vendors can perform this action." => Unauthorized(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpPost("CustomerIndicateInterestForWaivedProduct")]
        public async Task<IActionResult> CanProceedToPurchase(CustomerInterstForUrgentPurchase dto)
        {
            var response = await _serviceManager.IWaivedProductService.CustomerIndicateInterestForWaivedProduct(dto);

            if (!response.Status)
            {
                return response.Message switch
                {
                    "Notification not found." => NotFound(response),
                    "Only vendors can perform this action." => Unauthorized(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpGet("notifications/{notificationId}")]
        public async Task<IActionResult> GetNotificationById(string notificationId)
        {
            var response = await _serviceManager.IWaivedProductService.GetNotificationByIdAsync(notificationId);

            if (!response.Status)
            {
                return response.Message switch
                {
                    "Unauthorized access." => Unauthorized(response),
                    "Notification not found." => NotFound(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }
        [HttpGet("get-all-next-waive-market-records")]
        public async Task<IActionResult> GetNotificationById([FromQuery] PaginationFilter filter)
        {
            var response = await _serviceManager.IWaivedProductService.GetAllNextWaiveMarketDateRecords(filter);

            if (!response.IsSuccessful)
            {
                return response.Message switch
                {
                    "Unauthorized access." => Unauthorized(response),
                    "Notification not found." => NotFound(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpPost("RecordUrgentWaiveProductPurchase")]
        public async Task<IActionResult> RecordUrgentWaiveProductPurchase(string waiveProductId, string customerId, string deliveryAddress)
        {
            var response = await _serviceManager.IWaivedProductService.RecordUrgentWaiveProductPurchase(waiveProductId, customerId, deliveryAddress);

            if (!response.IsSuccessful)
            {
                return response.Message switch
                {
                    "Unauthorized access." => Unauthorized(response),
                    "Notification not found." => NotFound(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpGet("GetAllUrgentPurchaseAsync")]
        public async Task<IActionResult> GetAllUrgentPurchaseAsync([FromQuery] UrgentPurchaseFilter filter)
        {
            var response = await _serviceManager.IWaivedProductService.GetAllUrgentPurchaseAsync(filter);
            if (!response.IsSuccessful)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

    }
}
