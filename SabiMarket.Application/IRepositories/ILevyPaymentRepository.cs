﻿using SabiMarket.Application.DTOs;
using SabiMarket.Application.IRepositories;
using SabiMarket.Domain.Entities.LevyManagement;
using SabiMarket.Domain.Enum;

public interface ILevyPaymentRepository : IGeneralRepository<LevyPayment>
{
    void AddPayment(LevyPayment levyPayment);
    IQueryable<LevyPayment> GetPaymentsQuery();
    Task<IEnumerable<LevyPayment>> GetAllLevyPaymentForExport(bool trackChanges);
    Task<LevyPayment> GetPaymentById(string id, bool trackChanges);
    Task<PaginatorDto<IEnumerable<LevyPayment>>> GetPagedPayment(int? period, PaginationFilter paginationFilter);
    Task<PaginatorDto<IEnumerable<LevyPayment>>> GetLevyPaymentsAsync(string chairmanId, PaginationFilter paginationFilter, bool trackChanges);
    Task<PaginatorDto<IEnumerable<LevyPayment>>> SearchPayment(string searchString, PaginationFilter paginationFilter);
    Task<decimal> GetTotalLeviesAsync(DateTime startDate, DateTime endDate);
    void DeleteLevyPayment(LevyPayment levy);
    Task<IEnumerable<LevyPayment>> GetAllLevySetupsAsync(bool trackChanges);
    Task<LevyPayment> GetMarketLevySetup(string marketId, PaymentPeriodEnum period);
    Task<IQueryable<LevyPayment>> GetMarketLevySetups(string marketId);
}