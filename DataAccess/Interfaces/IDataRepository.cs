using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.DataAccess.Interfaces
{
    public interface IDataRepository
    {
        // User operations
        Task<List<User>> LoadUsersAsync();
        Task<User?> GetUserByIdAsync(string userId);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<bool> AddUserAsync(User user);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(string userId);

        // Book operations
        Task<List<Book>> LoadBooksAsync();
        Task<Book?> GetBookByIdAsync(string bookId);
        Task<Book?> GetBookByIsbnAsync(string isbn);
        Task<List<Book>> SearchBooksAsync(string searchTerm);
        Task<bool> AddBookAsync(Book book);
        Task<bool> UpdateBookAsync(Book book);
        Task<bool> DeleteBookAsync(string bookId);

        // Borrowing records operations
        Task<List<BorrowingRecord>> LoadBorrowingRecordsAsync();
        Task<List<BorrowingRecord>> GetBorrowingRecordsByUserIdAsync(string userId);
        Task<BorrowingRecord?> GetActiveBorrowingRecordAsync(string userId, string bookId);
        Task<bool> AddBorrowingRecordAsync(BorrowingRecord record);
        Task<bool> UpdateBorrowingRecordAsync(BorrowingRecord record);

        // Borrowing requests operations
        Task<List<BorrowingRequest>> LoadBorrowingRequestsAsync();
        Task<List<BorrowingRequest>> GetPendingRequestsAsync();
        Task<BorrowingRequest?> GetRequestByIdAsync(string requestId);
        Task<bool> AddBorrowingRequestAsync(BorrowingRequest request);
        Task<bool> UpdateBorrowingRequestAsync(BorrowingRequest request);
        Task<bool> DeleteBorrowingRequestAsync(string requestId);

        // Database specific operations (will be no-op for file storage)
        Task InitializeAsync();
        Task<bool> TestConnectionAsync();
    }
}