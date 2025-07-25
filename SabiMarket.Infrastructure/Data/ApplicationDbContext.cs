﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Domain.Entities;
using SabiMarket.Domain.Entities.Administration;
using SabiMarket.Domain.Entities.AdvertisementModule;
using SabiMarket.Domain.Entities.LevyManagement;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;
using SabiMarket.Domain.Entities.MarketParticipants;
using SabiMarket.Domain.Entities.OrdersAndFeedback;
using SabiMarket.Domain.Entities.Supporting;
using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Domain.Entities.WaiveMarketModule;

namespace SabiMarket.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        #region DbSet Properties
        public DbSet<LocalGovernment> LocalGovernments { get; set; }
        public DbSet<Market> Markets { get; set; }
        public DbSet<MarketSection> MarketSections { get; set; }
        public DbSet<Trader> Traders { get; set; }
        public DbSet<TraderBuildingType> TraderBuildingTypes { get; set; }
        public DbSet<Caretaker> Caretakers { get; set; }
        public DbSet<GoodBoy> GoodBoys { get; set; }
        public DbSet<AssistCenterOfficer> AssistCenterOfficers { get; set; }
        public DbSet<LevyPayment> LevyPayments { get; set; }
        public DbSet<LevySetup> LevySetups { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<WaivedProduct> WaivedProducts { get; set; }
        public DbSet<CustomerOrder> CustomerOrders { get; set; }
        public DbSet<CustomerOrderItem> CustomerOrderItems { get; set; }
        public DbSet<CustomerFeedback> CustomerFeedbacks { get; set; }
        public DbSet<Advertisement> Advertisements { get; set; }
        public DbSet<AdvertPayment> AdvertPayments { get; set; }
        public DbSet<AdvertisementLanguage> AdvertisementLanguage { get; set; }
        public DbSet<AdvertisementView> AdvertisementViews { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<Chairman> Chairmen { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        // Add Admin DbSet
        public DbSet<Admin> Admins { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<CustomerPurchase> CustomerPurchases { get; set; }
        public DbSet<WaiveMarketDates> WaiveMarketDates { get; set; }
        public DbSet<OfficerMarketAssignment> OfficerMarketAssignments { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<WaiveMarketNotification> WaiveMarketNotifications { get; set; }
        public DbSet<CustomerWaiveProductPurchase> CustomerWaiveProductPurchases { get; set; }

        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Market>()
                .HasOne(m => m.Caretaker)
                .WithMany() // No navigation back from Caretaker
                .HasForeignKey(m => m.CaretakerId)
                .IsRequired(false) // This is the key change to make the relationship optional
                .OnDelete(DeleteBehavior.NoAction); // Prevent cascading deletes
            base.OnModelCreating(modelBuilder);
            // In your ApplicationDbContext

            // GoodBoy-Caretaker relationship
            modelBuilder.Entity<GoodBoy>()
                .Property(g => g.CaretakerId)
                .IsRequired(false);

            modelBuilder.Entity<GoodBoy>()
                .HasOne(g => g.Caretaker)
                .WithMany(c => c.GoodBoys)
                .HasForeignKey(g => g.CaretakerId)
                .OnDelete(DeleteBehavior.NoAction);

            // In OnModelCreating method
            modelBuilder.Entity<OfficerMarketAssignment>()
            .HasKey(o => o.Id);

            modelBuilder.Entity<OfficerMarketAssignment>()
        .HasOne(o => o.AssistCenterOfficer)
        .WithMany(a => a.MarketAssignments)
        .HasForeignKey(o => o.AssistCenterOfficerId)
        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OfficerMarketAssignment>()
        .HasOne(o => o.Market)
        .WithMany()
        .HasForeignKey(o => o.MarketId)
        .OnDelete(DeleteBehavior.NoAction);

            /*modelBuilder.Entity<LevyPayment>()
           .HasOne(lp => lp.Trader)
           .WithMany(t => t.LevyPayments)
           .HasForeignKey(lp => lp.TraderId)
           .OnDelete(DeleteBehavior.NoAction);*/

            modelBuilder.Entity<Trader>()
                .HasOne(t => t.User)
                .WithOne(u => u.Trader)
                .HasForeignKey<Trader>(t => t.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Name=DefaultConnection");
            }
        }
    }
}