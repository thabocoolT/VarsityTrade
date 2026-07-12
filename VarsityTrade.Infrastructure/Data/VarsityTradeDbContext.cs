using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VarsityTrade.Core.Entities;
using Notification = VarsityTrade.Core.Entities.Notification;


namespace VarsityTrade.Infrastructure.Data
{
    public class VarsityTradeDbContext : IdentityDbContext<User, Microsoft.AspNetCore.Identity.IdentityRole<int>, int>

    {
        public VarsityTradeDbContext(DbContextOptions<VarsityTradeDbContext> options) : base(options) { }
       
        public DbSet<University> Universities { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<SellerProfile> SellerProfiles { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Condition> Condition { get; set; }
        public DbSet<ListingStatus> ListingStatuses { get; set; }
        public DbSet<Listing> Listings { get; set; }
        public DbSet<ListingImage> ListingImages { get; set; }
        public DbSet<SavedListing> SavedListigs { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<OfferItem> OfferItems { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Notification> Notifictions { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<HeroBannerSlide> HeroBannerSlides { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Explicit primary key declarations to avoid convention conflicts
            modelBuilder.Entity<Notification>()
                .HasKey(n => n.NotificationId);

            modelBuilder.Entity<SystemSettings>()
                .HasKey(s => s.SystemSettingsId);

            modelBuilder.Entity<AuditLog>()
                .HasKey(a => a.AuditLogId);

            modelBuilder.Entity<HeroBannerSlide>()
                .HasKey(h => h.HeroBannerSlideId);

            //Report-3 FKs to User, must be confifured explicitly
            modelBuilder.Entity<Report>()
                .HasOne(r=> r.ReportedBy)
                .WithMany(u => u.SubmittedReports)
                .HasForeignKey(r => r.ReportedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Report>()
                .HasOne(r => r.ReportedUser)
                .WithMany()
                .HasForeignKey(r => r.ReportedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Report>()
                .HasOne(r => r.ResolvedBy)
                .WithMany()
                .HasForeignKey(r => r.ResolvedById)
                .OnDelete(DeleteBehavior.Restrict);

        // Transaction — restrict delete to avoid cascade conflicts
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Listing)
                .WithOne(l => l.Transaction)
                .HasForeignKey<Transaction>(t => t.ListingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Buyer)
                .WithMany(u => u.Transactions)
                .HasForeignKey(t => t.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.SellerProfile)
                .WithMany(s => s.Transactions)
                .HasForeignKey(t => t.SellerProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            // Review — restrict delete to avoid cascade conflicts
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Transaction)
                .WithOne(t => t.Review)
                .HasForeignKey<Review>(r => r.TransactionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Reviewer)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.SellerProfile)
                .WithMany(s => s.Reviews)
                .HasForeignKey(r => r.SellerProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            // Conversation — restrict delete to avoid cascade conflicts
            modelBuilder.Entity<Conversation>()
                .HasOne(c => c.Buyer)
                .WithMany(u => u.Conversations)
                .HasForeignKey(c => c.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Conversation>()
                .HasOne(c => c.SellerProfile)
                .WithMany(s => s.Conversations)
                .HasForeignKey(c => c.SellerProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Conversation>()
                .HasOne(c => c.Listing)
                .WithMany(l => l.Conversations)
                .HasForeignKey(c => c.ListingId)
                .OnDelete(DeleteBehavior.Restrict);

            // Offer
            modelBuilder.Entity<Offer>()
                .HasOne(o => o.Buyer)
                .WithMany(u => u.Offers)
                .HasForeignKey(o => o.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Offer>()
                .HasOne(o => o.SellerProfile)
                .WithMany(s => s.Offers)
                .HasForeignKey(o => o.SellerProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            // Message
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.Messages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Listing
            modelBuilder.Entity<Listing>()
                .HasOne(l => l.SellerProfile)
                .WithMany(s => s.Listings)
                .HasForeignKey(l => l.SellerProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            // SavedListing — unique constraint
            modelBuilder.Entity<SavedListing>()
                .HasIndex(sl => new { sl.UserId, sl.ListingId })
                .IsUnique();

            // Category self-referencing
            modelBuilder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Decimal precision
            modelBuilder.Entity<SellerProfile>()
                .Property(s => s.AverageRating)
                .HasPrecision(3, 2);

            modelBuilder.Entity<Listing>()
                .Property(l => l.Price)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Offer>()
                .Property(o => o.OfferAmount)
                .HasPrecision(10, 2);

            modelBuilder.Entity<OfferItem>()
                .Property(oi => oi.EstimatedValue)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Transaction>()
                .Property(t => t.FinalPrice)
                .HasPrecision(10, 2);
            // SavedListing — prevent cascade delete conflict
            // When a Listing is deleted, do not automatically delete SavedListings
            // Admin or application logic handles cleanup manually
            modelBuilder.Entity<SavedListing>()
                .HasOne(sl => sl.Listing)
                .WithMany(l => l.SavedByUsers)
                .HasForeignKey(sl => sl.ListingId)
                .OnDelete(DeleteBehavior.Restrict);

            // SavedListing — prevent cascade delete conflict on User side
            // When a User is deleted, do not automatically delete SavedListings
            modelBuilder.Entity<SavedListing>()
                .HasOne(sl => sl.User)
                .WithMany(u => u.SavedListings)
                .HasForeignKey(sl => sl.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ListingImage — prevent cascade conflicts
            // Images are cleaned up by application logic, not database cascade
            modelBuilder.Entity<ListingImage>()
                .HasOne(li => li.Listing)
                .WithMany(l => l.ListingImages)
                .HasForeignKey(li => li.ListingImageId)
                .OnDelete(DeleteBehavior.Restrict);

            // Offer — prevent cascade conflicts on Listing side
            modelBuilder.Entity<Offer>()
                .HasOne(o => o.Listing)
                .WithMany(l => l.Offers)
                .HasForeignKey(o => o.ListingId)
                .OnDelete(DeleteBehavior.Restrict);

            // OfferItem — cascade delete is safe here
            // OfferItems belong exclusively to one Offer and have no other paths
            modelBuilder.Entity<OfferItem>()
                .HasOne(oi => oi.Offer)
                .WithMany(o => o.OfferItems)
                .HasForeignKey(oi => oi.OfferId)
                .OnDelete(DeleteBehavior.Cascade);

            // Report — prevent cascade on Listing side
            modelBuilder.Entity<Report>()
                .HasOne(r => r.Listing)
                .WithMany(l => l.Reports)
                .HasForeignKey(r => r.ListingId)
                .OnDelete(DeleteBehavior.Restrict);

            // HeroBannerSlide — prevent cascade on Listing side
            modelBuilder.Entity<HeroBannerSlide>()
                .HasOne(h => h.Listing)
                .WithMany(l => l.BannerSlides)
                .HasForeignKey(h => h.ListingId)
                .OnDelete(DeleteBehavior.Restrict);

            // HeroBannerSlide — prevent cascade on Review side
            modelBuilder.Entity<HeroBannerSlide>()
                .HasOne(h => h.Review)
                .WithMany(r => r.BannerSlides)
                .HasForeignKey(h => h.ReviewId)
                .OnDelete(DeleteBehavior.Restrict);

            // Notification — prevent cascade conflicts
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // AuditLog — prevent cascade conflicts
            // Audit logs must be preserved even if a user is deleted
            modelBuilder.Entity<AuditLog>()
                .HasOne(a => a.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // RefreshToken — safe to cascade delete when user is deleted
            // Tokens are meaningless without their user
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User — restrict delete on University side
            // Deleting a university should not cascade delete all its students
            modelBuilder.Entity<User>()
                .HasOne(u => u.University)
                .WithMany(un => un.Users)
                .HasForeignKey(u => u.UniversityId)
                .OnDelete(DeleteBehavior.Restrict);

            // User — restrict delete on Location side
            modelBuilder.Entity<User>()
                .HasOne(u => u.Location)
                .WithMany(l => l.Users)
                .HasForeignKey(u => u.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            // SellerProfile — restrict delete on User side
            modelBuilder.Entity<SellerProfile>()
                .HasOne(sp => sp.User)
                .WithOne(u => u.SellerProfile)
                .HasForeignKey<SellerProfile>(sp => sp.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Listing — restrict delete on University side
            modelBuilder.Entity<Listing>()
                .HasOne(l => l.University)
                .WithMany(u => u.Listings)
                .HasForeignKey(l => l.UniversityId)
                .OnDelete(DeleteBehavior.Restrict);

            // Listing — restrict delete on Category side
            modelBuilder.Entity<Listing>()
                .HasOne(l => l.Category)
                .WithMany(c => c.Listings)
                .HasForeignKey(l => l.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Listing — restrict delete on Condition side
            modelBuilder.Entity<Listing>()
                .HasOne(l => l.Condition)
                .WithMany(c => c.Listings)
                .HasForeignKey(l => l.ConditionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Listing — restrict delete on ListingStatus side
            modelBuilder.Entity<Listing>()
                .HasOne(l => l.ListingStatus)
                .WithMany(ls => ls.Listings)
                .HasForeignKey(l => l.ListingStatusId)
                .OnDelete(DeleteBehavior.Restrict);

        }



    }
}
