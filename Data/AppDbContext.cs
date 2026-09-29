using Microsoft.EntityFrameworkCore;
using AutoSphere.Models;

namespace AutoSphere.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<OTPVerification> OTPVerifications { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingDetail> BookingDetails { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<StaffAssignment> StaffAssignments { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<Part> Parts { get; set; }
        public DbSet<BookingPart> BookingParts { get; set; }
        public DbSet<NewsletterSubscriber> NewsletterSubscribers { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // One-to-One: Booking and Payment
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Payment)
                .WithOne(p => p.Booking)
                .HasForeignKey<Payment>(p => p.BookingId);

            // Unique Constraints
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.BookingReference)
                .IsUnique();

            modelBuilder.Entity<Coupon>()
                .HasIndex(c => c.Code)
                .IsUnique();

            // Seed Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 100, Name = "Admin" },
                new Role { Id = 101, Name = "Staff" },
                new Role { Id = 102, Name = "User" }
            );

            // Seed Admin User
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 100,
                    FullName = "System Administrator",
                    Email = "admin@autosphere.com",
                    Phone = "0000000000",
                    PasswordHash = "$2a$11$CvhWy4Ay0j5Mo72t6VYPcuYgL55Qwri1SOaberTX1Yn/mTPJzY/gu", // Admin@123
                    RoleId = 100,
                    IsEmailVerified = true,
                    CreatedAt = new System.DateTime(2026, 4, 17)
                }
            );
            // Seed Blog Posts
            modelBuilder.Entity<BlogPost>().HasData(
                new BlogPost
                {
                    Id = 1,
                    Title = "The AI Revolution in Engine Diagnostic Tools",
                    Slug = "ai-revolution-diagnostics",
                    Excerpt = "Discover how our new digital scanning arrays identify microscopic wear pattern before they become failures.",
                    Content = "Full article content about AI diagnostics...", // To be expanded in detail view
                    ImageUrl = "https://images.unsplash.com/photo-1486262715619-67b85e0b08d3?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80",
                    Category = "Technology",
                    AuthorName = "Mark Veron",
                    ReadTimeMinutes = 5,
                    CreatedAt = new DateTime(2026, 4, 12),
                    IsPublished = true
                },
                new BlogPost
                {
                    Id = 2,
                    Title = "Ceramic Coating: The Science of Surface Tension",
                    Slug = "ceramic-coating-science",
                    Excerpt = "Protect your investment with 9H Nano-technology. Why standard wax is no longer enough for modern luxury paint.",
                    Content = "Full article content about Ceramic Coating...",
                    ImageUrl = "https://images.unsplash.com/photo-1599256621730-535171e28e50?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80",
                    Category = "Maintenance",
                    AuthorName = "Sarah Chen",
                    ReadTimeMinutes = 3,
                    CreatedAt = new DateTime(2026, 4, 8),
                    IsPublished = true
                }
            );
        }
    }
}
