﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Application.IRepositories
{
    public interface IRepositoryManager
    {
        public ILevyPaymentRepository LevyPaymentRepository { get; }
        public IMarketRepository MarketRepository { get; }
        public IWaivedProductRepository WaivedProductRepository { get; }
        Task SaveChangesAsync();
    }
}
