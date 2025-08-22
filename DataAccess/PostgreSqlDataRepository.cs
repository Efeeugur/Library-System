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

        // Generic CRUD helper methods to reduce code duplication
        private async Task<bool> TryExecuteAsync(Func<Task> operation)
        {
            try
            {
                await operation();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database operation failed: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> AddEntityAsync<T>(T entity) where T : class
        {
            return await TryExecuteAsync(async () =>
            {
                _context.Set<T>().Add(entity);
                await _context.SaveChangesAsync();
            });
        }

        private async Task<bool> UpdateEntityAsync<T>(T entity) where T : class
        {
            return await TryExecuteAsync(async () =>
            {
                _context.Set<T>().Update(entity);
                await _context.SaveChangesAsync();
            });
        }

        private async Task<bool> DeleteEntityAsync<T>(string id) where T : class
        {
            return await TryExecuteAsync(async () =>
            {
                var entity = await _context.Set<T>().FindAsync(id);
                if (entity == null) return;
                _context.Set<T>().Remove(entity);
                await _context.SaveChangesAsync();
            });
        }

        public async Task InitializeAsync()
        {
            try
            {
                // Only create database if it doesn't exist - preserves existing data
                await _context.Database.EnsureCreatedAsync();
                Console.WriteLine("PostgreSQL database initialized successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing PostgreSQL database: {ex.Message}");
                throw;
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
        public async Task<List<User>> LoadUsersAsync() => 
            await _context.Users.ToListAsync();

        public async Task<User?> GetUserByIdAsync(string userId) => 
            await _context.Users.FindAsync(userId);

        public async Task<User?> GetUserByUsernameAsync(string username) => 
            await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

        public async Task<bool> AddUserAsync(User user)
        {
            return await AddEntityAsync(user);
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            return await UpdateEntityAsync(user);
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            return await DeleteEntityAsync<User>(userId);
        }

        // Book operations
        public async Task<List<Book>> LoadBooksAsync() => 
            await _context.Books.ToListAsync();

        public async Task<Book?> GetBookByIdAsync(string bookId) => 
            await _context.Books.FindAsync(bookId);

        public async Task<Book?> GetBookByIsbnAsync(string isbn) => 
            await _context.Books.FirstOrDefaultAsync(b => b.ISBN == isbn);

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
            return await TryExecuteAsync(async () =>
            {
                // Handle empty ISBN - set to null for database storage
                if (string.IsNullOrWhiteSpace(book.ISBN))
                    book.ISBN = null;

                // Check if ISBN already exists (only if not null/empty)
                if (!string.IsNullOrWhiteSpace(book.ISBN))
                {
                    var existingBook = await _context.Books
                        .FirstOrDefaultAsync(b => b.ISBN == book.ISBN);
                    if (existingBook != null)
                        throw new InvalidOperationException($"A book with ISBN '{book.ISBN}' already exists.");
                }

                _context.Books.Add(book);
                await _context.SaveChangesAsync();
            });
        }

        public async Task<bool> UpdateBookAsync(Book book)
        {
            return await UpdateEntityAsync(book);
        }

        public async Task<bool> DeleteBookAsync(string bookId)
        {
            return await DeleteEntityAsync<Book>(bookId);
        }

        // Borrowing records operations
        public async Task<List<BorrowingRecord>> LoadBorrowingRecordsAsync() => 
            await _context.BorrowingRecords.ToListAsync();


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
            return await AddEntityAsync(record);
        }

        public async Task<bool> UpdateBorrowingRecordAsync(BorrowingRecord record)
        {
            return await UpdateEntityAsync(record);
        }

        // Borrowing requests operations
        public async Task<List<BorrowingRequest>> LoadBorrowingRequestsAsync() => 
            await _context.BorrowingRequests.ToListAsync();


        public async Task<List<BorrowingRequest>> GetPendingRequestsAsync()
        {
            return await _context.BorrowingRequests
                .Where(r => r.Status == RequestStatus.Pending)
                .ToListAsync();
        }

        public async Task<BorrowingRequest?> GetRequestByIdAsync(string requestId) => 
            await _context.BorrowingRequests.FindAsync(requestId);

        public async Task<bool> AddBorrowingRequestAsync(BorrowingRequest request)
        {
            return await AddEntityAsync(request);
        }

        public async Task<bool> UpdateBorrowingRequestAsync(BorrowingRequest request)
        {
            return await UpdateEntityAsync(request);
        }

        public async Task<bool> DeleteBorrowingRequestAsync(string requestId)
        {
            return await DeleteEntityAsync<BorrowingRequest>(requestId);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}