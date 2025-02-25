using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
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
        public DbSet<Caretaker> Caretakers { get; set; }
        public DbSet<GoodBoy> GoodBoys { get; set; }
        public DbSet<AssistCenterOfficer> AssistCenterOfficers { get; set; }
        public DbSet<LevyPayment> LevyPayments { get; set; }
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
        // Add Admin DbSet
        public DbSet<Admin> Admins { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<SowFoodCompanyStaff> SowFoodStaffs { get; set; }
        public DbSet<SowFoodCompanyCustomer> SowFoodCustomers { get; set; }
        public DbSet<SowFoodCompanyProductionItem> SowFoodProductionItems { get; set; }
        public DbSet<SowFoodCompanySalesRecord> SowFoodSalesRecords { get; set; }
        public DbSet<SowFoodCompanyShelfItem> SowFoodShelfItems { get; set; }
        public DbSet<SowFoodCompanyStaffAppraiser> SowFoodStaffAppraisers { get; set; }
        public DbSet<SowFoodCompanyStaffAttendance> SowFoodStaffAttendances { get; set; }
        public DbSet<SowFoodCompany> SowFoodCompanies { get; set; }
        public DbSet<CustomerPurchase> CustomerPurchases { get; set; }

        #endregion


        /*    protected override void OnModelCreating(ModelBuilder builder)
            {
                base.OnModelCreating(builder);

                // Set default schema
                builder.HasDefaultSchema("dbo");

                #region Identity Configuration
                builder.Entity<ApplicationUser>(entity =>
                {
                    entity.ToTable("Users");

                    // Basic properties
                    entity.Property(e => e.FirstName).HasMaxLength(100);
                    entity.Property(e => e.LastName).HasMaxLength(100);
                    entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                    entity.Property(e => e.Address).IsRequired(false);
                    entity.Property(e => e.AdminId).IsRequired(false);  // Make AdminId nullable

                    // Configure LocalGovernment relationship
                    entity.HasOne(u => u.LocalGovernment)
                          .WithMany(lg => lg.Users)
                          .HasForeignKey(u => u.LocalGovernmentId)
                          .OnDelete(DeleteBehavior.Cascade);

                    // Configure Admin relationship
                    entity.HasOne(u => u.Admin)
                          .WithOne(a => a.User)
                          .HasForeignKey<Admin>(a => a.UserId)  // Foreign key on Admin
                          .OnDelete(DeleteBehavior.Cascade);
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
                          .OnDelete(DeleteBehavior.Cascade);

                    entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                });

                builder.Entity<MarketSection>(entity =>
                {
                    entity.HasOne(s => s.Market)
                          .WithMany(m => m.Sections)
                          .HasForeignKey(s => s.MarketId)
                          .OnDelete(DeleteBehavior.Cascade);
                });
                #endregion

                *//*           #region Market Participants Configuration
                           builder.Entity<Chairman>(entity =>
                           {
                               entity.HasOne(c => c.User)
                                     .WithOne(u => u.Chairman)
                                     .HasForeignKey<Chairman>(c => c.UserId)
                                     .OnDelete(DeleteBehavior.Cascade);

                               entity.HasOne(c => c.LocalGovernment)
                                     .WithMany()
                                     .HasForeignKey(c => c.LocalGovernmentId)
                                     .OnDelete(DeleteBehavior.Cascade);
                           });

                           builder.Entity<Trader>(entity =>
                           {
                               entity.HasOne(t => t.User)
                                     .WithOne(u => u.Trader)
                                     .HasForeignKey<Trader>(t => t.UserId)
                                     .OnDelete(DeleteBehavior.Cascade);

                               entity.HasOne(t => t.Market)
                                     .WithMany(m => m.Traders)
                                     .HasForeignKey(t => t.MarketId)
                                     .OnDelete(DeleteBehavior.Cascade);

                               entity.HasOne(t => t.Section)
                                     .WithMany(s => s.Traders)
                                     .HasForeignKey(t => t.SectionId)
                                     .OnDelete(DeleteBehavior.Cascade);

                               entity.HasOne(t => t.Caretaker)
                                     .WithMany(c => c.AssignedTraders)
                                     .HasForeignKey(t => t.CaretakerId)
                                     .OnDelete(DeleteBehavior.Cascade);

                               entity.HasIndex(e => e.TIN).IsUnique();
                               entity.HasIndex(e => e.QRCode).IsUnique();
                           });

                           builder.Entity<Caretaker>(entity =>
                           {
                               entity.HasOne(c => c.User)
                                     .WithOne(u => u.Caretaker)
                                     .HasForeignKey<Caretaker>(c => c.UserId)
                                     .OnDelete(DeleteBehavior.Cascade);

                               entity.HasOne(c => c.Market)
                                     .WithMany(m => m.Caretakers)
                                     .HasForeignKey(c => c.MarketId)
                                     .OnDelete(DeleteBehavior.Cascade);
                           });

                           builder.Entity<GoodBoy>(entity =>
                           {
                               entity.HasOne(g => g.User)
                                     .WithOne(u => u.GoodBoy)
                                     .HasForeignKey<GoodBoy>(g => g.UserId)
                                     .OnDelete(DeleteBehavior.Cascade);

                               entity.HasOne(g => g.Caretaker)
                                     .WithMany(c => c.GoodBoys)
                                     .HasForeignKey(g => g.CaretakerId)
                                     .OnDelete(DeleteBehavior.Cascade);
                           });

                           builder.Entity<AssistCenterOfficer>(entity =>
                           {
                               entity.HasOne(a => a.User)
                                     .WithOne(u => u.AssistCenterOfficer)
                                     .HasForeignKey<AssistCenterOfficer>(a => a.UserId)
                                     .OnDelete(DeleteBehavior.Cascade);

                               entity.HasOne(a => a.LocalGovernment)
                                     .WithMany()
                                     .HasForeignKey(a => a.LocalGovernmentId)
                                     .OnDelete(DeleteBehavior.Cascade);
                           });
                           #endregion*/

        /*     #region Market Participants Configuration
             builder.Entity<Market>(entity =>
             {
                 // Existing configuration
                 entity.HasOne(m => m.LocalGovernment)
                       .WithMany(lg => lg.Markets)
                       .HasForeignKey(m => m.LocalGovernmentId)
                       .OnDelete(DeleteBehavior.Cascade);

                 // Configure one-to-one relationship with Chairman
                 entity.HasOne(m => m.Chairman)
                       .WithOne(c => c.Market)
                       .HasForeignKey<Chairman>(c => c.MarketId)  // Specify Chairman as dependent
                       .OnDelete(DeleteBehavior.Cascade);
             });

             builder.Entity<Chairman>(entity =>
             {
                 entity.HasOne(c => c.User)
                       .WithOne(u => u.Chairman)
                       .HasForeignKey<Chairman>(c => c.UserId)
                       .OnDelete(DeleteBehavior.Cascade);

                 entity.HasOne(c => c.LocalGovernment)
                       .WithMany()
                       .HasForeignKey(c => c.LocalGovernmentId)
                       .OnDelete(DeleteBehavior.Cascade);

                 // Remove any explicit configuration of Market here since it's configured in Market entity
             });
             #endregion*//* // recent

        #region Market Participants Configuration
        builder.Entity<AssistCenterOfficer>(entity =>
        {
            entity.HasOne(a => a.User)
                  .WithOne(u => u.AssistCenterOfficer)
                  .HasForeignKey<AssistCenterOfficer>(a => a.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(a => a.Market)
                  .WithMany()
                  .HasForeignKey(a => a.MarketId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(a => a.Chairman)
                  .WithMany()
                  .HasForeignKey(a => a.ChairmanId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(a => a.LocalGovernment)
                  .WithMany()
                  .HasForeignKey(a => a.LocalGovernmentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Market>(entity =>
        {
            entity.HasOne(m => m.LocalGovernment)
                  .WithMany(lg => lg.Markets)
                  .HasForeignKey(m => m.LocalGovernmentId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(m => m.Chairman)
                  .WithOne(c => c.Market)
                  .HasForeignKey<Chairman>(c => c.MarketId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(m => m.Traders)
                  .WithOne(t => t.Market)
                  .HasForeignKey(t => t.MarketId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(m => m.Caretakers)
                  .WithOne(c => c.Market)
                  .HasForeignKey(c => c.MarketId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(m => m.Sections)
                  .WithOne(s => s.Market)
                  .HasForeignKey(s => s.MarketId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Caretaker>(entity =>
        {
            entity.HasOne(c => c.User)
                  .WithOne(u => u.Caretaker)
                  .HasForeignKey<Caretaker>(c => c.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.Market)
                  .WithMany(m => m.Caretakers)
                  .HasForeignKey(c => c.MarketId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.Chairman)
                  .WithMany()
                  .HasForeignKey(c => c.ChairmanId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(c => c.GoodBoys)
                  .WithOne(g => g.Caretaker)
                  .HasForeignKey(g => g.CaretakerId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(c => c.AssignedTraders)
                  .WithOne(t => t.Caretaker)
                  .HasForeignKey(t => t.CaretakerId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<GoodBoy>(entity =>
        {
            entity.HasOne(g => g.User)
                  .WithOne(u => u.GoodBoy)
                  .HasForeignKey<GoodBoy>(g => g.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(g => g.Caretaker)
                  .WithMany(c => c.GoodBoys)
                  .HasForeignKey(g => g.CaretakerId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(g => g.Market)
                  .WithMany()
                  .HasForeignKey(g => g.MarketId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Trader>(entity =>
        {
            entity.HasOne(t => t.User)
                  .WithOne(u => u.Trader)
                  .HasForeignKey<Trader>(t => t.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(t => t.Market)
                  .WithMany(m => m.Traders)
                  .HasForeignKey(t => t.MarketId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(t => t.Section)
                  .WithMany(s => s.Traders)
                  .HasForeignKey(t => t.SectionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(t => t.Caretaker)
                  .WithMany(c => c.AssignedTraders)
                  .HasForeignKey(t => t.CaretakerId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(t => t.TIN).IsUnique();
            entity.HasIndex(t => t.QRCode).IsUnique();
        });

        builder.Entity<MarketSection>(entity =>
        {
            entity.HasOne(s => s.Market)
                  .WithMany(m => m.Sections)
                  .HasForeignKey(s => s.MarketId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(s => s.Traders)
                  .WithOne(t => t.Section)
                  .HasForeignKey(t => t.SectionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Chairman>(entity =>
        {
            entity.HasOne(c => c.User)
                  .WithOne(u => u.Chairman)
                  .HasForeignKey<Chairman>(c => c.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.Market)
                  .WithOne(m => m.Chairman)
                  .HasForeignKey<Chairman>(c => c.MarketId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.LocalGovernment)
                  .WithMany()
                  .HasForeignKey(c => c.LocalGovernmentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        #endregion

        #region Levy Management Configuration
        builder.Entity<LevyPayment>(entity =>
        {
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.IncentiveAmount).HasPrecision(18, 2);
            entity.Property(e => e.PaymentDate).HasDefaultValueSql("GETDATE()");

            entity.HasOne(c => c.GoodBoy)
                 .WithMany(g => g.LevyPayments)
                 .HasForeignKey(c => c.GoodBoyId)
                 .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.Trader)
                  .WithMany(t => t.LevyPayments)
                  .HasForeignKey(p => p.TraderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        #endregion

        #region Waived Market Configuration
        builder.Entity<WaivedProduct>(entity =>
        {
            entity.ToTable("WaivedProducts");

            // Required Properties
            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.Description)
                  .HasMaxLength(500);

            entity.Property(e => e.ImageUrl)
                  .HasMaxLength(255);

            entity.Property(e => e.ProductCode)
                  .HasMaxLength(50);

            // Decimal Precision
            entity.Property(e => e.OriginalPrice)
                  .HasPrecision(18, 2);

            entity.Property(e => e.WaivedPrice)
                  .HasPrecision(18, 2);

            // Indexes
            entity.HasIndex(e => e.ProductCode)
                  .IsUnique();

            // Relationships
            entity.HasOne(p => p.Vendor)
                  .WithMany(v => v.Products)
                  .HasForeignKey(p => p.VendorId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.Categories)
                  .WithMany(c => c.Products)
                  .UsingEntity(j => j.ToTable("ProductCategoryMappings"));

            // Updated to match CustomerOrderItem's navigation property name
            entity.HasMany(p => p.OrderItems)
                  .WithOne(oi => oi.Product)  // Changed from WaivedProduct to Product
                  .HasForeignKey(oi => oi.ProductId)  // Changed from WaivedProductId to ProductId
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Vendor>(entity =>
        {
            entity.ToTable("Vendors");

            // Required Properties
            entity.Property(e => e.BusinessName)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.BusinessAddress)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(e => e.VendorCode)
                  .IsRequired();

            entity.Property(e => e.BusinessDescription)
                  .HasMaxLength(500);

            // Indexes
            entity.HasIndex(e => e.VendorCode)
                  .IsUnique();

            // Relationships
            entity.HasOne(v => v.User)
                  .WithOne(u => u.Vendor)
                  .HasForeignKey<Vendor>(v => v.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(v => v.LocalGovernment)
                  .WithMany(lg => lg.Vendors)
                  .HasForeignKey(v => v.LocalGovernmentId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(v => v.Products)
                  .WithOne(p => p.Vendor)
                  .HasForeignKey(p => p.VendorId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(v => v.Orders)
                  .WithOne()
                  .HasForeignKey("VendorId")
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(v => v.Feedbacks)
                  .WithOne()
                  .HasForeignKey("VendorId")
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(v => v.Advertisements)
                  .WithOne()
                  .HasForeignKey("VendorId")
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<WaivedProduct>(entity =>
        {
            entity.ToTable("WaivedProducts");

            // Required Properties
            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.Description)
                  .HasMaxLength(500);

            entity.Property(e => e.ImageUrl)
                  .HasMaxLength(255);

            entity.Property(e => e.ProductCode)
                  .HasMaxLength(50);

            // Decimal Precision
            entity.Property(e => e.OriginalPrice)
                  .HasPrecision(18, 2);

            entity.Property(e => e.WaivedPrice)
                  .HasPrecision(18, 2);

            // Indexes
            entity.HasIndex(e => e.ProductCode)
                  .IsUnique();

            // Relationships
            entity.HasOne(p => p.Vendor)
                  .WithMany(v => v.Products)
                  .HasForeignKey(p => p.VendorId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.Categories)
                  .WithMany(c => c.Products)
                  .UsingEntity(j => j.ToTable("ProductCategoryMappings"));

            entity.HasMany(p => p.OrderItems)
                  .WithOne()
                  .HasForeignKey("WaivedProductId")
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customers");

            // Required Properties
            entity.Property(e => e.FullName)
                  .IsRequired()
                  .HasMaxLength(100);

            // Relationships
            entity.HasOne(c => c.User)
                  .WithOne(u => u.Customer)
                  .HasForeignKey<Customer>(c => c.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.LocalGovernment)
                  .WithMany()
                  .HasForeignKey(c => c.LocalGovernmentId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(c => c.Orders)
                  .WithOne()
                  .HasForeignKey("CustomerId")
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(c => c.Feedbacks)
                  .WithOne()
                  .HasForeignKey("CustomerId")
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Subscription>(entity =>
        {
            entity.ToTable("Subscriptions");

            // Properties
            entity.Property(e => e.SGId)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.Amount)
                  .HasPrecision(18, 2);

            entity.Property(e => e.ProofOfPayment)
                  .HasMaxLength(500);

            // Relationships
            entity.HasOne(s => s.Subscriber)
                  .WithMany()
                  .HasForeignKey(s => s.SubscriberId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.SubscriptionActivator)
                  .WithMany()
                  .HasForeignKey(s => s.SubscriptionActivatorId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<SubscriptionPlan>(entity =>
        {
            entity.ToTable("SubscriptionPlans");

            // Properties
            entity.Property(e => e.Frequency)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(e => e.Amount)
                  .HasPrecision(18, 2);

            // Relationships
            entity.HasMany(sp => sp.Subscriptions)
                  .WithOne()
                  .HasForeignKey("SubscriptionPlanId")
                  .OnDelete(DeleteBehavior.Cascade);
        });

        #endregion

        *//*  #region Waived Market Configuration
          builder.Entity<Vendor>(entity =>
          {
              // Relationships
              entity.HasOne(v => v.User)
                    .WithOne(u => u.Vendor)
                    .HasForeignKey<Vendor>(v => v.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

              entity.HasOne(v => v.LocalGovernment)
                    .WithMany(lg => lg.Vendors)
                    .HasForeignKey(v => v.LocalGovernmentId)
                    .OnDelete(DeleteBehavior.Cascade);

              // Indexes
              entity.HasIndex(e => e.VendorCode)
                    .IsUnique();

              // Property configurations
              entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");
          });

          builder.Entity<WaivedProduct>(entity =>
          {
              // Required Properties
              entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

              // Optional Properties
              entity.Property(e => e.Description)
                    .HasMaxLength(500);

              entity.Property(e => e.ImageUrl)
                    .HasMaxLength(255);

              entity.Property(e => e.ProductCode)
                    .HasMaxLength(50);

              // Decimal Precision
              entity.Property(e => e.OriginalPrice)
                    .HasPrecision(18, 2);

              entity.Property(e => e.WaivedPrice)
                    .HasPrecision(18, 2);

              // Indexes
              entity.HasIndex(e => e.ProductCode)
                    .IsUnique();

              // Relationships
              entity.HasOne(p => p.Vendor)
                    .WithMany(v => v.Products)
                    .HasForeignKey(p => p.VendorId)
                    .OnDelete(DeleteBehavior.Cascade);

              entity.HasMany(p => p.Categories)
                    .WithMany(c => c.Products)
                    .UsingEntity(j => j.ToTable("ProductCategoryMappings"));

              // Collection Properties
              entity.HasMany(p => p.OrderItems)
                    .WithOne(oi => oi.WaivedProduct)
                    .HasForeignKey(oi => oi.WaivedProductId)
                    .OnDelete(DeleteBehavior.Cascade);
          });
          #endregion*//*

        #region Customer & Order Configuration
        builder.Entity<Customer>(entity =>
        {
            entity.HasOne(c => c.User)
                  .WithOne(u => u.Customer)
                  .HasForeignKey<Customer>(c => c.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.LocalGovernment)
                  .WithMany(lg => lg.Customers)
                  .HasForeignKey(c => c.LocalGovernmentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CustomerOrder>(entity =>
        {
            entity.ToTable("CustomerOrders");

            entity.Property(e => e.TotalAmount)
                  .HasPrecision(18, 2);

            entity.Property(e => e.DeliveryAddress)
                  .HasMaxLength(500);

            entity.Property(e => e.Notes)
                  .HasMaxLength(1000);

            // Relationships
            entity.HasOne(o => o.Customer)
                  .WithMany(c => c.Orders)
                  .HasForeignKey(o => o.CustomerId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(o => o.Vendor)
                  .WithMany(v => v.Orders)
                  .HasForeignKey(o => o.VendorId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(o => o.OrderItems)
                  .WithOne(oi => oi.Order)
                  .HasForeignKey(oi => oi.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CustomerOrderItem>(entity =>
        {
            entity.ToTable("CustomerOrderItems");

            entity.Property(e => e.UnitPrice)
                  .HasPrecision(18, 2);

            entity.Property(e => e.TotalPrice)
                  .HasPrecision(18, 2);

            // Relationships
            entity.HasOne(oi => oi.Order)
                  .WithMany(o => o.OrderItems)
                  .HasForeignKey(oi => oi.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(oi => oi.Product)
                  .WithMany(p => p.OrderItems)
                  .HasForeignKey(oi => oi.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CustomerFeedback>(entity =>
        {
            entity.HasOne(f => f.Customer)
                  .WithMany(c => c.Feedbacks)
                  .HasForeignKey(f => f.CustomerId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(f => f.Vendor)
                  .WithMany(v => v.Feedbacks)
                  .HasForeignKey(f => f.VendorId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        #endregion

        #region Advertisement Configuration
        builder.Entity<Advertisement>(entity =>
        {
            entity.Property(e => e.Price).HasPrecision(18, 2);

            entity.HasOne(a => a.Vendor)
                  .WithMany(v => v.Advertisements)
                  .HasForeignKey(a => a.VendorId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AdvertisementView>(entity =>
        {
            entity.HasOne(v => v.Advertisement)
                  .WithMany(a => a.Views)
                  .HasForeignKey(v => v.AdvertisementId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(v => v.User)
                  .WithMany()
                  .HasForeignKey(v => v.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        #endregion

        #region Identity Configuration
        // Update ApplicationRole configuration
        builder.Entity<ApplicationRole>(entity =>
        {
            entity.ToTable("Roles");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.LastModifiedBy).HasMaxLength(100);
        });
        #endregion

        #region Admin Configuration
        builder.Entity<Admin>(entity =>
        {
            entity.ToTable("Admins");

            // Properties
            entity.Property(e => e.AdminLevel)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.Department)
                  .HasMaxLength(100);

            entity.Property(e => e.Position)
                  .HasMaxLength(100);

            entity.Property(e => e.TotalRevenue)
                  .HasPrecision(18, 2);

            entity.Property(e => e.StatsLastUpdatedAt)
                  .HasDefaultValueSql("GETDATE()");

            // Access Control Properties with Default Values
            entity.Property(e => e.HasDashboardAccess)
                  .HasDefaultValue(true);

            entity.Property(e => e.HasRoleManagementAccess)
                  .HasDefaultValue(true);

            entity.Property(e => e.HasTeamManagementAccess)
                  .HasDefaultValue(true);

            entity.Property(e => e.HasAuditLogAccess)
                  .HasDefaultValue(true);

            entity.Property(e => e.HasAdvertManagementAccess)
                  .HasDefaultValue(true);

            // Relationships
            entity.HasOne(a => a.User)
                  .WithOne(u => u.Admin)
                  .HasForeignKey<Admin>(a => a.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(a => a.Advertisements)
                  .WithOne(ad => ad.Admin)
                  .HasForeignKey(ad => ad.AdminId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(a => a.AdminAuditLogs)
                  .WithOne()
                  .HasForeignKey("AdminId")
                  .OnDelete(DeleteBehavior.Cascade);
        });
        #endregion

        *//*    #region Admin Configuration
            builder.Entity<Admin>(entity =>
            {
                entity.ToTable("Admins");

                // Properties
                entity.Property(e => e.AdminLevel)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(e => e.Department)
                      .HasMaxLength(100);

                entity.Property(e => e.Position)
                      .HasMaxLength(100);

                entity.Property(e => e.TotalRevenue)
                      .HasPrecision(18, 2);

                entity.Property(e => e.StatsLastUpdatedAt)
                      .HasDefaultValueSql("GETDATE()");

                // Default values for access flags
                entity.Property(e => e.HasDashboardAccess).HasDefaultValue(true);
                entity.Property(e => e.HasRoleManagementAccess).HasDefaultValue(true);
                entity.Property(e => e.HasTeamManagementAccess).HasDefaultValue(true);
                entity.Property(e => e.HasAuditLogAccess).HasDefaultValue(true);

                // Configure User relationship (inverse side)
                entity.HasOne(a => a.User)
                      .WithOne(u => u.Admin)
                      .HasForeignKey<Admin>(a => a.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            #endregion*//*

        #region AuditLog Configuration
        builder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");

            // Properties
            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.Time).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Activity).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Module).HasMaxLength(50);
            entity.Property(e => e.Details).HasMaxLength(500);
            entity.Property(e => e.IpAddress).HasMaxLength(50);

            // Relationships
            entity.HasOne(a => a.User)
                  .WithMany()
                  .HasForeignKey(a => a.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        #endregion

        #region Role Configuration
        builder.Entity<ApplicationRole>(entity =>
        {
            entity.ToTable("Roles");

            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.LastModifiedBy).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
        });

        builder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("RolePermissions");

            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);

            entity.HasOne(rp => rp.Role)
                  .WithMany(r => r.Permissions)
                  .HasForeignKey(rp => rp.RoleId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(rp => new { rp.RoleId, rp.Name }).IsUnique();
        });
        #endregion

        #region Subscription Configuration
        builder.Entity<Subscription>(entity =>
        {
            entity.ToTable("Subscriptions");

            // Configure properties
            entity.Property(e => e.SGId).IsRequired();
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.ProofOfPayment).IsRequired();
            entity.Property(e => e.SubscriptionStartDate).HasDefaultValueSql("GETDATE()");

            // Configure relationships
            entity.HasOne(s => s.Subscriber)
                .WithMany()
                .HasForeignKey(s => s.SubscriberId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.SubscriptionActivator)
                .WithMany()
                .HasForeignKey(s => s.SubscriptionActivatorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes if needed
            entity.HasIndex(e => e.SGId);
        });
        #endregion

        #region Advertisement Configuration

        builder.Entity<Advertisement>(entity =>
        {
            entity.ToTable("Advertisements");

            // Properties
            entity.Property(e => e.Title)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(e => e.Description)
                  .IsRequired()
                  .HasMaxLength(500);

            entity.Property(e => e.ImageUrl)
                  .HasMaxLength(500);

            entity.Property(e => e.TargetUrl)
                  .HasMaxLength(500);

            entity.Property(e => e.AdvertId)
                  .HasMaxLength(20);

            entity.Property(e => e.Language)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(e => e.Location)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.AdvertPlacement)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.Price)
                  .HasPrecision(18, 2);

            entity.Property(e => e.PaymentStatus)
                  .HasMaxLength(50);

            entity.Property(e => e.PaymentProofUrl)
                  .HasMaxLength(500);

            entity.Property(e => e.BankTransferReference)
                  .HasMaxLength(100);

            // Relationships
            entity.HasOne(a => a.Vendor)
                  .WithMany()
                  .HasForeignKey(a => a.VendorId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(a => a.Views)
                  .WithOne(v => v.Advertisement)
                  .HasForeignKey(v => v.AdvertisementId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(a => a.Translations)
                  .WithOne(t => t.Advertisement)
                  .HasForeignKey(t => t.AdvertisementId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(a => a.Payment)
                  .WithOne(p => p.Advertisement)
                  .HasForeignKey<AdvertPayment>(p => p.AdvertisementId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AdvertisementView>(entity =>
        {
            entity.ToTable("AdvertisementViews");

            // Properties
            entity.Property(e => e.IPAddress)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(e => e.ViewedAt)
                  .IsRequired();

            // Relationships
            entity.HasOne(v => v.Advertisement)
                  .WithMany(a => a.Views)
                  .HasForeignKey(v => v.AdvertisementId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(v => v.User)
                  .WithMany()
                  .HasForeignKey(v => v.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AdvertisementLanguage>(entity =>
        {
            entity.ToTable("AdvertisementLanguages");

            // Properties
            entity.Property(e => e.Language)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(e => e.Title)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(e => e.Description)
                  .IsRequired()
                  .HasMaxLength(500);

            // Relationships
            entity.HasOne(l => l.Advertisement)
                  .WithMany(a => a.Translations)
                  .HasForeignKey(l => l.AdvertisementId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AdvertPayment>(entity =>
        {
            entity.ToTable("AdvertPayments");

            // Properties
            entity.Property(e => e.Amount)
                  .HasPrecision(18, 2);

            entity.Property(e => e.PaymentMethod)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(e => e.BankName)
                  .HasMaxLength(100);

            entity.Property(e => e.AccountNumber)
                  .HasMaxLength(20);

            entity.Property(e => e.AccountName)
                  .HasMaxLength(100);

            entity.Property(e => e.Status)
                  .HasMaxLength(50);

            entity.Property(e => e.ProofOfPaymentUrl)
                  .HasMaxLength(500);

            // Relationships
            entity.HasOne(p => p.Advertisement)
                  .WithOne(a => a.Payment)
                  .HasForeignKey<AdvertPayment>(p => p.AdvertisementId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        #endregion

        #region SowFood Configuration

        builder.Entity<SowFoodCompany>(entity =>
        {
            entity.ToTable("SowFoodCompanies");

            // Properties
            entity.Property(e => e.CompanyName)
                  .IsRequired()
                  .HasMaxLength(200);

            // Relationships - Remove two-way navigation with User
            entity.HasOne(s => s.User)
                  .WithMany()
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<SowFoodCompanyCustomer>(entity =>
        {
            entity.ToTable("SowFoodCompanyCustomers");

            // Properties
            entity.Property(e => e.RegisteredBy)
                  .IsRequired()
                  .HasMaxLength(450);

            // Relationships - Remove two-way navigation with User
            entity.HasOne(sc => sc.User)
                  .WithMany()
                  .HasForeignKey(sc => sc.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(sc => sc.SowFoodCompany)
                  .WithMany(s => s.Customers)
                  .HasForeignKey(sc => sc.SowFoodCompanyId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<SowFoodCompanyProductionItem>(entity =>
        {
            entity.ToTable("SowFoodCompanyProductionItems");

            // Properties
            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(e => e.UnitPrice)
                  .HasPrecision(18, 2);

            entity.Property(e => e.ImageUrl)
                  .HasMaxLength(500);

            // Relationships
            entity.HasOne(p => p.SowFoodCompany)
                  .WithMany(s => s.SowFoodProducts)
                  .HasForeignKey(p => p.SowFoodCompanyId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<SowFoodCompanyShelfItem>(entity =>
        {
            entity.ToTable("SowFoodCompanyShelfItems");

            // Properties
            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(e => e.UnitPrice)
                  .HasPrecision(18, 2);

            entity.Property(e => e.ImageUrl)
                  .HasMaxLength(500);

            // Relationships
            entity.HasOne(p => p.SowFoodCompany)
                  .WithMany(s => s.SowFoodShelfItems)
                  .HasForeignKey(p => p.SowFoodCompanyId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<SowFoodCompanyStaff>(entity =>
        {
            entity.ToTable("SowFoodCompanyStaff");

            // Properties
            entity.Property(e => e.StaffId)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.ImageUrl)
                  .HasMaxLength(500);

            // Relationships - Remove two-way navigation with User
            entity.HasOne(s => s.User)
                  .WithMany()
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.SowFoodCompany)
                  .WithMany(c => c.Staff)
                  .HasForeignKey(s => s.SowFoodCompanyId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<SowFoodCompanySalesRecord>(entity =>
        {
            entity.ToTable("SowFoodCompanySalesRecords");

            // Properties
            entity.Property(e => e.UnitPrice)
                  .HasPrecision(18, 2);

            entity.Ignore(e => e.TotalPrice);

            // Relationships
            entity.HasOne(sr => sr.SowFoodCompanyCustomer)
                  .WithMany(c => c.SowFoodCompanySalesRecords)
                  .HasForeignKey(sr => sr.SowFoodCompanyCustomerId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(sr => sr.SowFoodCompanyProductItem)
                  .WithMany()
                  .HasForeignKey(sr => sr.SowFoodCompanyProductItemId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(sr => sr.SowFoodCompanyShelfItem)
                  .WithMany()
                  .HasForeignKey(sr => sr.SowFoodCompanyShelfItemId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(sr => sr.SowFoodCompanyStaff)
                  .WithMany(s => s.SowFoodCompanySalesRecords)
                  .HasForeignKey(sr => sr.SowFoodCompanyStaffId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<SowFoodCompanyStaffAppraiser>(entity =>
        {
            entity.ToTable("SowFoodCompanyStaffAppraisers");

            // Properties
            entity.Property(e => e.Remark)
                  .IsRequired()
                  .HasMaxLength(500);

            // Relationships - Remove two-way navigation with User
            entity.HasOne(a => a.User)
                  .WithMany()
                  .HasForeignKey(a => a.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(a => a.SowFoodCompanyStaff)
                  .WithMany()
                  .HasForeignKey(a => a.SowFoodCompanyStaffId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<SowFoodCompanyStaffAttendance>(entity =>
        {
            entity.ToTable("SowFoodCompanyStaffAttendances");

            // Properties
            entity.Property(e => e.LogonTime)
                  .IsRequired();

            entity.Property(e => e.LogoutTime)
                  .IsRequired();

            // Relationships - Remove two-way navigation with User
            entity.HasOne(a => a.ConfirmedByUser)
                  .WithMany()
                  .HasForeignKey(a => a.ConfirmedByUserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(a => a.SowFoodCompanyStaff)
                  .WithMany(s => s.SowFoodCompanyStaffAttendances)
                  .HasForeignKey(a => a.SowFoodCompanyStaffId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        #endregion

    }*/


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Market>()
                .HasOne(m => m.Caretaker)
                .WithMany() // No navigation back from Caretaker
                .HasForeignKey(m => m.CaretakerId)
                .IsRequired(false) // This is the key change to make the relationship optional
                .OnDelete(DeleteBehavior.NoAction); // Prevent cascading deletes
            base.OnModelCreating(modelBuilder);
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