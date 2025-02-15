using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Domain.Entities;
using SabiMarket.Domain.Entities.Administration;
using SabiMarket.Domain.Entities.AdvertisementModule;
using SabiMarket.Domain.Entities.LevyManagement;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;
using SabiMarket.Domain.Entities.MarketParticipants;
using SabiMarket.Domain.Entities.OrdersAndFeedback;
using SabiMarket.Domain.Entities.SowFoodLinkUp;
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
                      .OnDelete(DeleteBehavior.Restrict);

                // Configure Admin relationship
                entity.HasOne(u => u.Admin)
                      .WithOne(a => a.User)
                      .HasForeignKey<Admin>(a => a.UserId)  // Foreign key on Admin
                      .OnDelete(DeleteBehavior.Restrict);
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

 /*           #region Market Participants Configuration
            builder.Entity<Chairman>(entity =>
            {
                entity.HasOne(c => c.User)
                      .WithOne(u => u.Chairman)
                      .HasForeignKey<Chairman>(c => c.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.LocalGovernment)
                      .WithMany()
                      .HasForeignKey(c => c.LocalGovernmentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

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

                entity.HasOne(t => t.Caretaker)
                      .WithMany(c => c.AssignedTraders)
                      .HasForeignKey(t => t.CaretakerId)
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

            builder.Entity<GoodBoy>(entity =>
            {
                entity.HasOne(g => g.User)
                      .WithOne(u => u.GoodBoy)
                      .HasForeignKey<GoodBoy>(g => g.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(g => g.Caretaker)
                      .WithMany(c => c.GoodBoys)
                      .HasForeignKey(g => g.CaretakerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<AssistCenterOfficer>(entity =>
            {
                entity.HasOne(a => a.User)
                      .WithOne(u => u.AssistCenterOfficer)
                      .HasForeignKey<AssistCenterOfficer>(a => a.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.LocalGovernment)
                      .WithMany()
                      .HasForeignKey(a => a.LocalGovernmentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            #endregion*/

            #region Market Participants Configuration
            builder.Entity<Market>(entity =>
            {
                // Existing configuration
                entity.HasOne(m => m.LocalGovernment)
                      .WithMany(lg => lg.Markets)
                      .HasForeignKey(m => m.LocalGovernmentId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Configure one-to-one relationship with Chairman
                entity.HasOne(m => m.Chairman)
                      .WithOne(c => c.Market)
                      .HasForeignKey<Chairman>(c => c.MarketId)  // Specify Chairman as dependent
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Chairman>(entity =>
            {
                entity.HasOne(c => c.User)
                      .WithOne(u => u.Chairman)
                      .HasForeignKey<Chairman>(c => c.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.LocalGovernment)
                      .WithMany()
                      .HasForeignKey(c => c.LocalGovernmentId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Remove any explicit configuration of Market here since it's configured in Market entity
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
                     .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Trader)
                      .WithMany(t => t.LevyPayments)
                      .HasForeignKey(p => p.TraderId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            #endregion

            #region Waived Market Configuration
            builder.Entity<Vendor>(entity =>
            {
                entity.HasOne(v => v.User)
                      .WithOne(u => u.Vendor)
                      .HasForeignKey<Vendor>(v => v.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(v => v.LocalGovernment)
                      .WithMany(lg => lg.Vendors)
                      .HasForeignKey(v => v.LocalGovernmentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.VendorCode).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            });

            builder.Entity<WaivedProduct>(entity =>
            {
                entity.Property(e => e.OriginalPrice).HasPrecision(18, 2);
                entity.Property(e => e.WaivedPrice).HasPrecision(18, 2);
                entity.HasIndex(e => e.ProductCode).IsUnique();

                entity.HasOne(p => p.Vendor)
                      .WithMany(v => v.Products)
                      .HasForeignKey(p => p.VendorId)
                      .OnDelete(DeleteBehavior.Restrict);

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

                entity.HasOne(c => c.LocalGovernment)
                      .WithMany(lg => lg.Customers)
                      .HasForeignKey(c => c.LocalGovernmentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<CustomerOrder>(entity =>
            {
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
                entity.Property(e => e.OrderDate).HasDefaultValueSql("GETDATE()");

                entity.HasOne(o => o.Customer)
                      .WithMany(c => c.Orders)
                      .HasForeignKey(o => o.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(o => o.Vendor)
                      .WithMany(v => v.Orders)
                      .HasForeignKey(o => o.VendorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<CustomerOrderItem>(entity =>
            {
                entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
                entity.Property(e => e.TotalPrice).HasPrecision(18, 2);

                entity.HasOne(i => i.Order)
                      .WithMany(o => o.OrderItems)
                      .HasForeignKey(i => i.OrderId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(i => i.Product)
                      .WithMany(p => p.OrderItems)
                      .HasForeignKey(i => i.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<CustomerFeedback>(entity =>
            {
                entity.HasOne(f => f.Customer)
                      .WithMany(c => c.Feedbacks)
                      .HasForeignKey(f => f.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(f => f.Vendor)
                      .WithMany(v => v.Feedbacks)
                      .HasForeignKey(f => f.VendorId)
                      .OnDelete(DeleteBehavior.Restrict);
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

                entity.HasOne(v => v.User)
                      .WithMany()
                      .HasForeignKey(v => v.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
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

                // Default values for access flags
                entity.Property(e => e.HasDashboardAccess).HasDefaultValue(true);
                entity.Property(e => e.HasRoleManagementAccess).HasDefaultValue(true);
                entity.Property(e => e.HasTeamManagementAccess).HasDefaultValue(true);
                entity.Property(e => e.HasAuditLogAccess).HasDefaultValue(true);

                // Configure User relationship (inverse side)
                entity.HasOne(a => a.User)
                      .WithOne(u => u.Admin)
                      .HasForeignKey<Admin>(a => a.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            #endregion

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
                      .OnDelete(DeleteBehavior.Restrict);
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
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.SubscriptionActivator)
                    .WithMany()
                    .HasForeignKey(s => s.SubscriptionActivatorId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes if needed
                entity.HasIndex(e => e.SGId);
            });
            #endregion
            #region SowFood Configuration
            builder.Entity<SowFoodCompany>(entity =>
            {
                entity.ToTable("SowFoodCompanies");
                entity.Property(e => e.CompanyName).IsRequired().HasMaxLength(200);
            });

            builder.Entity<SowFoodCompanyCustomer>(entity =>
            {
                entity.ToTable("SowFoodCompanyCustomers");

                entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.EmailAddress).HasMaxLength(100);

                entity.HasOne(sc => sc.SowFoodCompany)
                      .WithMany(s => s.SowFoodCustomers)
                      .HasForeignKey(sc => sc.SowFoodCompanyId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<SowFoodCompanyProductionItem>(entity =>
            {
                entity.ToTable("SowFoodCompanyProductionItems");

                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.UnitPrice).HasPrecision(18, 2);

                entity.HasOne(p => p.SowFoodCompany)
                      .WithMany(s => s.SowFoodProducts)
                      .HasForeignKey(p => p.SowFoodCompanyId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<SowFoodCompanyShelfItem>(entity =>
            {
                entity.ToTable("SowFoodCompanyShelfItems");

                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.UnitPrice).HasPrecision(18, 2);

                entity.HasOne(p => p.SowFoodCompany)
                      .WithMany(s => s.SowFoodShelfItems)
                      .HasForeignKey(p => p.SowFoodCompanyId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<SowFoodCompanySalesRecord>(entity =>
            {
                entity.ToTable("SowFoodCompanySalesRecords");

                entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
                entity.Ignore(e => e.TotalPrice); // This is a computed property

                entity.HasOne(sr => sr.SowFoodCompanyCustomer)
                      .WithMany(c => c.SowFoodCompanySalesRecords)
                      .HasForeignKey(sr => sr.SowFoodCompanyCustomerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(sr => sr.SowFoodCompanyProductItem)
                      .WithMany()
                      .HasForeignKey(sr => sr.SowFoodCompanyProductItemId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(sr => sr.SowFoodCompanyShelfItem)
                      .WithMany()
                      .HasForeignKey(sr => sr.SowFoodCompanyShelfItemId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<SowFoodCompanyStaff>(entity =>
            {
                entity.ToTable("SowFoodCompanyStaff");

                entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.EmailAddress).HasMaxLength(100);
                entity.Property(e => e.Role).HasMaxLength(50);

                entity.HasOne(s => s.SowFoodCompany)
                      .WithMany(c => c.Staff)
                      .HasForeignKey(s => s.SowFoodCompanyId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<SowFoodCompanyStaffAppraiser>(entity =>
            {
                entity.ToTable("SowFoodCompanyStaffAppraisers");

                // Configure primary key
                entity.HasKey(e => e.SowFoodCompanyStaffId);

                entity.Property(e => e.Remark).IsRequired().HasMaxLength(500);

                entity.HasOne(a => a.SowFoodCompanyStaff)
                      .WithOne()
                      .HasForeignKey<SowFoodCompanyStaffAppraiser>(a => a.SowFoodCompanyStaffId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<SowFoodCompanyStaffAttendance>(entity =>
            {
                entity.ToTable("SowFoodCompanyStaffAttendances");

                // Configure composite primary key
                entity.HasKey(e => new { e.SowFoodCompanyStaffId, e.LogonTime });

                entity.Property(e => e.LogonTime).IsRequired();
                entity.Property(e => e.LogoutTime).IsRequired();

                entity.HasOne(a => a.SowFoodCompanyStaff)
                      .WithMany(s => s.SowFoodCompanyStaffAttendances)
                      .HasForeignKey(a => a.SowFoodCompanyStaffId)
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