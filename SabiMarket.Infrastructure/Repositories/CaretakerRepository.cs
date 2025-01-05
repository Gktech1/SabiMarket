using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.Interfaces;
using SabiMarket.Application.IRepositories;
using SabiMarket.Domain.Entities.MarketParticipants;
using SabiMarket.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Infrastructure.Repositories
{
    public class CaretakerRepository : GeneralRepository<Caretaker>, ICaretakerRepository
    {
        public CaretakerRepository(ApplicationDbContext context) : base(context) { }

        public void AddCaretaker(Caretaker caretaker) => Create(caretaker);

        public void UpdateCaretaker(Caretaker caretaker) => Update(caretaker);

        public async Task<IEnumerable<Caretaker>> GetAllCaretaker(bool trackChanges) => await FindAll(trackChanges).ToListAsync();

    }
}
