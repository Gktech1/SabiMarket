using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Domain.Entities;
using SabiMarket.Domain.Entities.AdvertisementModule;
using SabiMarket.Domain.Entities.LevyManagement;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;
using SabiMarket.Domain.Entities.MarketParticipants;
using SabiMarket.Domain.Entities.OrdersAndFeedback;
using SabiMarket.Domain.Entities.Supporting;
using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Domain.Entities.WaiveMarketModule;
using System.Collections.Generic;
using System.Reflection.Emit;

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
        public DbSet<Caretaker> Caretakers { get; set; }
        public DbSet<GoodBoy> GoodBoys { get; set; }
        public DbSet<AssistCenterOfficer> AssistCenterOfficers { get; set; }
        public DbSet<LevyPayment> LevyPayments { get; set; }
        public DbSet<LevyCollection> LevyCollections { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<WaivedProduct> WaivedProducts { get; set; }
        public DbSet<CustomerOrder> CustomerOrders { get; set; }
        public DbSet<CustomerOrderItem> CustomerOrderItems { get; set; }
        public DbSet<CustomerFeedback> CustomerFeedbacks { get; set; }
        public DbSet<Advertisement> Advertisements { get; set; }
        public DbSet<AdvertisementView> AdvertisementViews { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Set default schema
            builder.HasDefaultSchema("dbo");

            #region Identity Configuration
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("Users");
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            });

            builder.Entity<ApplicationRole>(entity =>
            {
                entity.ToTable("Roles");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            });
            #endregion

            #region Local Government & Market Configuration
            builder.Entity<LocalGovernment>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.CurrentRevenue).HasPrecision(18, 2);
            });

            builder.Entity<Market>(entity =>
            {
                entity.HasOne(m => m.LocalGovernment)
                      .WithMany(lg => lg.Markets)
                      .HasForeignKey(m => m.LocalGovernmentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            });

            builder.Entity<MarketSection>(entity =>
            {
                entity.HasOne(s => s.Market)
                      .WithMany(m => m.Sections)
                      .HasForeignKey(s => s.MarketId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            #endregion

            #region Market Participants Configuration
            builder.Entity<Trader>(entity =>
            {
                entity.HasOne(t => t.User)
                      .WithOne(u => u.Trader)
                      .HasForeignKey<Trader>(t => t.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.Market)
                      .WithMany(m => m.Traders)
                      .HasForeignKey(t => t.MarketId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.Section)
                      .WithMany(s => s.Traders)
                      .HasForeignKey(t => t.SectionId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.TIN).IsUnique();
                entity.HasIndex(e => e.QRCode).IsUnique();
            });

            builder.Entity<Caretaker>(entity =>
            {
                entity.HasOne(c => c.User)
                      .WithOne(u => u.Caretaker)
                      .HasForeignKey<Caretaker>(c => c.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Market)
                      .WithMany(m => m.Caretakers)
                      .HasForeignKey(c => c.MarketId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            #endregion

            #region Levy Management Configuration
            builder.Entity<LevyPayment>(entity =>
            {
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.IncentiveAmount).HasPrecision(18, 2);
                entity.Property(e => e.PaymentDate).HasDefaultValueSql("GETDATE()");

                entity.HasOne(p => p.Trader)
                      .WithMany(t => t.LevyPayments)
                      .HasForeignKey(p => p.TraderId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<LevyCollections>(entity =>
            {
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.CollectionDate).HasDefaultValueSql("GETDATE()");
            });
            #endregion

            #region Waived Market Configuration
            builder.Entity<Vendor>(entity =>
            {
                entity.HasOne(v => v.User)
                      .WithOne(u => u.Vendor)
                      .HasForeignKey<Vendor>(v => v.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.VendorCode).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            });

            builder.Entity<WaivedProduct>(entity =>
            {
                entity.Property(e => e.OriginalPrice).HasPrecision(18, 2);
                entity.Property(e => e.WaivedPrice).HasPrecision(18, 2);
                entity.HasIndex(e => e.ProductCode).IsUnique();

                entity.HasMany(p => p.Categories)
                      .WithMany(c => c.Products)
                      .UsingEntity(j => j.ToTable("ProductCategoryMappings"));
            });
            #endregion

            #region Customer & Order Configuration
            builder.Entity<Customer>(entity =>
            {
                entity.HasOne(c => c.User)
                      .WithOne(u => u.Customer)
                      .HasForeignKey<Customer>(c => c.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            });

            builder.Entity<CustomerOrder>(entity =>
            {
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
                entity.Property(e => e.OrderDate).HasDefaultValueSql("GETDATE()");

                entity.HasOne(o => o.Customer)
                      .WithMany(c => c.Orders)
                      .HasForeignKey(o => o.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<CustomerOrderItem>(entity =>
            {
                entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
                entity.Property(e => e.TotalPrice).HasPrecision(18, 2);
            });
            #endregion

            #region Advertisement Configuration
            builder.Entity<Advertisement>(entity =>
            {
                entity.Property(e => e.Price).HasPrecision(18, 2);

                entity.HasOne(a => a.Vendor)
                      .WithMany(v => v.Advertisements)
                      .HasForeignKey(a => a.VendorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<AdvertisementView>(entity =>
            {
                entity.HasOne(v => v.Advertisement)
                      .WithMany(a => a.Views)
                      .HasForeignKey(v => v.AdvertisementId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            #endregion
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