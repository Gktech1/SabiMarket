using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Domain.Entities
{
    public class Market
    {
        public string Name { get; set; }
        public string UserId { get; set; }
        public string Description { get; set; }
        public string LGA { get; set; }
        public User User { get; set; }
        public ChairMan ChairMan { get; set; }
        public ICollection<Shop> Shop { get; set; }
        public ICollection<GoodBoy> GoodBoy { get; set; }
        public ICollection<CareTaker> CareTaker { get; set; }
        public ICollection<Trader> Trader { get; set; }
        public ICollection<AssistantOfficer> AssistantOfficer { get; set; }
    }
}
