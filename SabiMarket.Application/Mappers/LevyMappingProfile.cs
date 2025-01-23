using AutoMapper;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Domain.Entities.LevyManagement;
using SabiMarket.Domain.Enum;

namespace SabiMarket.Application.Mappings
{
    public class LevyPaymentMappingProfile : Profile
    {
        public LevyPaymentMappingProfile()
        {
            CreateMap<LevyPayment, LevyInfoResponseDto>()
                // Market Information
                .ForMember(dest => dest.MarketId,
                    opt => opt.MapFrom(src => src.MarketId))
                .ForMember(dest => dest.MarketName,
                    opt => opt.MapFrom(src => src.Market.Name))
                .ForMember(dest => dest.MarketAddress,
                    opt => opt.MapFrom(src => src.Market.Address))
                .ForMember(dest => dest.MarketType,
                    opt => opt.MapFrom(src => src.Market.MarketType))

                // Levy Configuration
                .ForMember(dest => dest.TraderOccupancy,
                    opt => opt.MapFrom(src => src.Trader.OccupancyType))
                .ForMember(dest => dest.PaymentFrequencyDays,
                    opt => opt.MapFrom(src => MapPaymentPeriodToDays(src.Period)))
                .ForMember(dest => dest.Amount,
                    opt => opt.MapFrom(src => src.Amount))

                // Statistics
                .ForMember(dest => dest.ActiveTradersCount,
                    opt => opt.MapFrom(src =>
                        src.Market.Traders.Count(t => t.IsActive)))
                .ForMember(dest => dest.PaidTradersCount,
                    opt => opt.MapFrom(src =>
                        src.Market.LevyPayments.Count(lp =>
                            lp.PaymentStatus == PaymentStatusEnum.Successful &&
                            lp.PaymentDate.Date == DateTime.UtcNow.Date)))
                .ForMember(dest => dest.DefaultersTradersCount,
                    opt => opt.MapFrom(src =>
                        src.Market.Traders.Count(t => t.IsActive) -
                        src.Market.LevyPayments.Count(lp =>
                            lp.PaymentStatus == PaymentStatusEnum.Successful &&
                            lp.PaymentDate.Date == DateTime.UtcNow.Date)))
                .ForMember(dest => dest.ExpectedDailyRevenue,
                    opt => opt.MapFrom(src =>
                        src.Amount * src.Market.Traders.Count(t => t.IsActive)))
                .ForMember(dest => dest.ActualDailyRevenue,
                    opt => opt.MapFrom(src =>
                        src.Market.LevyPayments
                            .Where(lp =>
                                lp.PaymentStatus == PaymentStatusEnum.Successful &&
                                lp.PaymentDate.Date == DateTime.UtcNow.Date)
                            .Sum(lp => lp.Amount)))

                // Audit Information
                .ForMember(dest => dest.CreatedBy,
                    opt => opt.MapFrom(src => src.CreatedBy))
                .ForMember(dest => dest.LastModifiedBy,
                    opt => opt.MapFrom(src => src.LastModifiedBy))
                .ForMember(dest => dest.CreatedAt,
                    opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.LastModifiedAt,
                    opt => opt.MapFrom(src => src.LastModifiedAt));

            CreateMap<UpdateLevyRequestDto, LevyPayment>()
                .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
                .ForMember(dest => dest.TraderId, opt => opt.MapFrom(src => src.TraderId))
                .ForMember(dest => dest.GoodBoyId, opt => opt.MapFrom(src => src.GoodBoyId))
                .ForAllOtherMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }

        private int MapPaymentPeriodToDays(PaymentPeriodEnum period)
        {
            return period switch
            {
                PaymentPeriodEnum.Daily => 1,
                PaymentPeriodEnum.Weekly => 7,
                PaymentPeriodEnum.Monthly => 30,
                PaymentPeriodEnum.Yearly => 365,
                _ => 1 // Default to daily for custom periods
            };
        }
    }
}