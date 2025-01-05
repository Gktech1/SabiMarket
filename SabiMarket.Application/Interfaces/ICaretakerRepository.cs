using SabiMarket.Domain.Entities.MarketParticipants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Application.Interfaces
{
    public interface ICaretakerRepository
    {
        void AddCaretaker(Caretaker caretaker);
        void UpdateCaretaker(Caretaker caretaker);
        Task<IEnumerable<Caretaker>> GetAllCaretaker(bool trackChanges);
    }
}
