using SabiMarket.Application.DTOs;
using SabiMarket.Domain.Entities.LevyManagement;
using SabiMarket.Domain.Entities.MarketParticipants;

namespace SabiMarket.Application.Interfaces
{
    public interface ICaretakerRepository
    {
        Task<Caretaker> GetCaretakerById(string userId, bool trackChanges);
        Task<Caretaker> GetCaretakerByMarketId(string marketId, bool trackChanges);
        Task<PaginatorDto<IEnumerable<Caretaker>>> GetCaretakersWithPagination(PaginationFilter paginationFilter, bool trackChanges);
        Task<bool> CaretakerExists(string userId, string marketId);
        Task<PaginatorDto<IEnumerable<LevyPayment>>> GetLevyPayments(string caretakerId, PaginationFilter paginationFilter, bool trackChanges);
        Task<LevyPayment> GetLevyPaymentDetails(string levyId, bool trackChanges);
        Task<PaginatorDto<IEnumerable<GoodBoy>>> GetGoodBoys(string caretakerId, PaginationFilter paginationFilter, bool trackChanges);
        void CreateCaretaker(Caretaker caretaker);
        void DeleteCaretaker(Caretaker caretaker);
    }
}
