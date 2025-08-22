using LibraryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.DataAccess
{
    public class LibraryContext : DbContext
    {
        public LibraryContext(DbContextOptions<LibraryContext> options) : base(options)
        {
        }

        // DbSets represent tables in the database
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Book> Books { get; set; } = null!;
        public DbSet<BorrowingRecord> BorrowingRecords { get; set; } = null!;
        public DbSet<BorrowingRequest> BorrowingRequests { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.UserId).HasMaxLength(50);
                entity.Property(e => e.Username).HasMaxLength(100).IsRequired();
                entity.Property(e => e.PasswordHash).HasMaxLength(500).IsRequired();
                entity.Property(e => e.Role).HasConversion<string>();
                entity.HasIndex(e => e.Username).IsUnique();
            });

            // Configure Book entity
            modelBuilder.Entity<Book>(entity =>
            {
                entity.HasKey(e => e.BookId);
                entity.Property(e => e.BookId).HasMaxLength(50);
                entity.Property(e => e.Title).HasMaxLength(500).IsRequired();
                entity.Property(e => e.Author).HasMaxLength(300).IsRequired();
                entity.Property(e => e.ISBN).HasMaxLength(50).IsRequired(false); // Make ISBN optional
                entity.Property(e => e.Status).HasConversion<string>();

                // Create a unique index on ISBN only for non-null values
                entity.HasIndex(e => e.ISBN)
                    .IsUnique()
                    .HasFilter("\"ISBN\" IS NOT NULL");
            });

            // Configure BorrowingRecord entity
            modelBuilder.Entity<BorrowingRecord>(entity =>
            {
                entity.HasKey(e => e.RecordId);
                entity.Property(e => e.RecordId).HasMaxLength(50);
                entity.Property(e => e.UserId).HasMaxLength(50).IsRequired();
                entity.Property(e => e.BookId).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Status).HasConversion<string>();

                // Foreign key relationships
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<Book>()
                    .WithMany()
                    .HasForeignKey(e => e.BookId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure BorrowingRequest entity
            modelBuilder.Entity<BorrowingRequest>(entity =>
            {
                entity.HasKey(e => e.RequestId);
                entity.Property(e => e.RequestId).HasMaxLength(50);
                entity.Property(e => e.UserId).HasMaxLength(50).IsRequired();
                entity.Property(e => e.BookId).HasMaxLength(50).IsRequired();
                entity.Property(e => e.AdminId).HasMaxLength(50).IsRequired(false);
                entity.Property(e => e.Status).HasConversion<string>();

                // Foreign key relationships
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<Book>()
                    .WithMany()
                    .HasForeignKey(e => e.BookId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.AdminId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}