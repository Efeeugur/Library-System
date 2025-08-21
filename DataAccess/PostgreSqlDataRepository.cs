using LibraryManagementSystem.DataAccess.Interfaces;
using LibraryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.DataAccess
{
    public class PostgreSqlDataRepository : IDataRepository, IDisposable
    {
        private readonly LibraryContext _context;

        public PostgreSqlDataRepository(LibraryContext context)
        {
            _context = context;
        }

        public async Task InitializeAsync()
        {
            try
            {
                // For schema changes, we need to recreate the database
                // This will drop and recreate tables with the correct schema
                await _context.Database.EnsureDeletedAsync();
                await _context.Database.EnsureCreatedAsync();
                Console.WriteLine("PostgreSQL database recreated with updated schema.");
                
                // Seed sample books for easier testing
                await SeedSampleBooksAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing PostgreSQL database: {ex.Message}");
                throw;
            }
        }

        private async Task SeedSampleBooksAsync()
        {
            try
            {
                var sampleBooks = new List<Book>
                {
                    new Book("The Great Gatsby", "F. Scott Fitzgerald", 1925, "978-0-7432-7356-5"),
                    new Book("To Kill a Mockingbird", "Harper Lee", 1960, "978-0-06-112008-4"),
                    new Book("1984", "George Orwell", 1949, "978-0-452-28423-4"),
                    new Book("Pride and Prejudice", "Jane Austen", 1813, "978-0-14-143951-8"),
                    new Book("The Catcher in the Rye", "J.D. Salinger", 1951, "978-0-316-76948-0"),
                    new Book("Animal Farm", "George Orwell", 1945, "978-0-452-28424-1"),
                    new Book("Lord of the Flies", "William Golding", 1954, "978-0-571-05686-2"),
                    new Book("The Hobbit", "J.R.R. Tolkien", 1937, "978-0-547-92822-7"),
                    new Book("Fahrenheit 451", "Ray Bradbury", 1953, "978-1-4516-7331-9"),
                    new Book("Jane Eyre", "Charlotte Brontë", 1847, "978-0-14-144114-6"),
                    new Book("Wuthering Heights", "Emily Brontë", 1847, "978-0-14-143955-6"),
                    new Book("The Lord of the Rings", "J.R.R. Tolkien", 1954, "978-0-544-00341-5"),
                    new Book("Of Mice and Men", "John Steinbeck", 1937, "978-0-14-017739-8"),
                    new Book("The Adventures of Huckleberry Finn", "Mark Twain", 1884, "978-0-14-243717-4"),
                    new Book("Brave New World", "Aldous Huxley", 1932, "978-0-06-085052-4"),
                    new Book("The Kite Runner", "Khaled Hosseini", 2003, "978-1-59463-193-1"),
                    new Book("Life of Pi", "Yann Martel", 2001, "978-0-15-602732-3"),
                    new Book("The Book Thief", "Markus Zusak", 2005, "978-0-375-84220-7"),
                    new Book("The Alchemist", "Paulo Coelho", 1988, "978-0-06-231500-7"),
                    new Book("One Hundred Years of Solitude", "Gabriel García Márquez", 1967, "978-0-06-088328-8")
                };

                foreach (var book in sampleBooks)
                {
                    _context.Books.Add(book);
                }

                await _context.SaveChangesAsync();
                Console.WriteLine($"Successfully seeded {sampleBooks.Count} sample books to the database.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding sample books: {ex.Message}");
                // Don't throw - seeding failure shouldn't prevent database initialization
            }
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                await _context.Database.CanConnectAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // User operations
        public async Task<List<User>> LoadUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task SaveUsersAsync(List<User> users)
        {
            throw new NotSupportedException("Use individual user operations for database storage");
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<bool> AddUserAsync(User user)
        {
            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding user to PostgreSQL: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Book operations
        public async Task<List<Book>> LoadBooksAsync()
        {
            return await _context.Books.ToListAsync();
        }

        public async Task SaveBooksAsync(List<Book> books)
        {
            // This method is primarily for file-based operations
            throw new NotSupportedException("Use individual book operations for database storage");
        }

        public async Task<Book?> GetBookByIdAsync(string bookId)
        {
            return await _context.Books.FindAsync(bookId);
        }

        public async Task<Book?> GetBookByIsbnAsync(string isbn)
        {
            return await _context.Books
                .FirstOrDefaultAsync(b => b.ISBN == isbn);
        }

        public async Task<List<Book>> SearchBooksAsync(string searchTerm)
        {
            return await _context.Books
                .Where(b => b.Title.ToLower().Contains(searchTerm.ToLower()) ||
                           b.Author.ToLower().Contains(searchTerm.ToLower()) ||
                           (b.ISBN != null && b.ISBN.ToLower().Contains(searchTerm.ToLower())))
                .ToListAsync();
        }

        public async Task<bool> AddBookAsync(Book book)
        {
            try
            {
                // Handle empty ISBN - set to null for database storage
                if (string.IsNullOrWhiteSpace(book.ISBN))
                {
                    book.ISBN = null;
                }
                
                // Check if ISBN already exists (only if not null/empty)
                if (!string.IsNullOrWhiteSpace(book.ISBN))
                {
                    var existingBook = await _context.Books
                        .FirstOrDefaultAsync(b => b.ISBN == book.ISBN);
                    if (existingBook != null)
                    {
                        Console.WriteLine($"Error: A book with ISBN '{book.ISBN}' already exists.");
                        return false;
                    }
                }
                
                _context.Books.Add(book);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding book to PostgreSQL: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }

        public async Task<bool> UpdateBookAsync(Book book)
        {
            try
            {
                _context.Books.Update(book);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteBookAsync(string bookId)
        {
            try
            {
                var book = await _context.Books.FindAsync(bookId);
                if (book == null) return false;

                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Borrowing records operations
        public async Task<List<BorrowingRecord>> LoadBorrowingRecordsAsync()
        {
            return await _context.BorrowingRecords.ToListAsync();
        }

        public async Task SaveBorrowingRecordsAsync(List<BorrowingRecord> records)
        {
            throw new NotSupportedException("Use individual borrowing record operations for database storage");
        }

        public async Task<List<BorrowingRecord>> GetBorrowingRecordsByUserIdAsync(string userId)
        {
            return await _context.BorrowingRecords
                .Where(r => r.UserId == userId)
                .ToListAsync();
        }

        public async Task<BorrowingRecord?> GetActiveBorrowingRecordAsync(string userId, string bookId)
        {
            return await _context.BorrowingRecords
                .FirstOrDefaultAsync(r => r.UserId == userId &&
                                         r.BookId == bookId &&
                                         r.Status == BorrowingStatus.Active);
        }

        public async Task<bool> AddBorrowingRecordAsync(BorrowingRecord record)
        {
            try
            {
                _context.BorrowingRecords.Add(record);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateBorrowingRecordAsync(BorrowingRecord record)
        {
            try
            {
                _context.BorrowingRecords.Update(record);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Borrowing requests operations
        public async Task<List<BorrowingRequest>> LoadBorrowingRequestsAsync()
        {
            return await _context.BorrowingRequests.ToListAsync();
        }

        public async Task SaveBorrowingRequestsAsync(List<BorrowingRequest> requests)
        {
            throw new NotSupportedException("Use individual borrowing request operations for database storage");
        }

        public async Task<List<BorrowingRequest>> GetPendingRequestsAsync()
        {
            return await _context.BorrowingRequests
                .Where(r => r.Status == RequestStatus.Pending)
                .ToListAsync();
        }

        public async Task<BorrowingRequest?> GetRequestByIdAsync(string requestId)
        {
            return await _context.BorrowingRequests.FindAsync(requestId);
        }

        public async Task<bool> AddBorrowingRequestAsync(BorrowingRequest request)
        {
            try
            {
                _context.BorrowingRequests.Add(request);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding borrowing request to PostgreSQL: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }

        public async Task<bool> UpdateBorrowingRequestAsync(BorrowingRequest request)
        {
            try
            {
                _context.BorrowingRequests.Update(request);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating borrowing request in PostgreSQL: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }

        public async Task<bool> DeleteBorrowingRequestAsync(string requestId)
        {
            try
            {
                var request = await _context.BorrowingRequests.FindAsync(requestId);
                if (request == null) return false;

                _context.BorrowingRequests.Remove(request);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}