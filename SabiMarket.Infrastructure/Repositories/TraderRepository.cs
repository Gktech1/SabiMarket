using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.Interfaces;
using SabiMarket.Domain.Entities.MarketParticipants;
using SabiMarket.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Infrastructure.Repositories
{
    public class TraderRepository : GeneralRepository<Trader>, ITraderRepository
    {
        public TraderRepository(ApplicationDbContext context) : base(context) { }

        public void AddTrader(Trader trader) => Create(trader);

        public void UpdateTrader(Trader trader) => Update(trader);

        public async Task<IEnumerable<Trader>> GetAllAssistCenterOfficer(bool trackChanges) => await FindAll(trackChanges).ToListAsync();

    }
}
