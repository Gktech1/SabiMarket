using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.Interfaces;
using SabiMarket.Domain.Entities.MarketParticipants;
using SabiMarket.Infrastructure.Data;

namespace SabiMarket.Infrastructure.Repositories
{
    public class GoodBoyRepository : GeneralRepository<GoodBoy>,IGoodBoyRepository
    {
        public GoodBoyRepository(ApplicationDbContext context) : base(context) { }

        public void AddGoodBoy(GoodBoy goodBoy) => Create(goodBoy);

        public void UpdateGoodBoy(GoodBoy goodBoy) => Update(goodBoy);

        public async Task<IEnumerable<GoodBoy>> GetAllAssistCenterOfficer(bool trackChanges) => await FindAll(trackChanges).ToListAsync();

    }
}
