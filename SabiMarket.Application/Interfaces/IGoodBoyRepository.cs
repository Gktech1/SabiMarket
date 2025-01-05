namespace SabiMarket.Application.Interfaces
{
    public interface IGoodBoyRepository
    {
        void AddGoodBoy(GoodBoy goodBoy);
        void UpdateGoodBoy(GoodBoy goodBoy);
        Task<IEnumerable<GoodBoy>> GetAllAssistCenterOfficer(bool trackChanges);
    }
}
