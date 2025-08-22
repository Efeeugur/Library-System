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