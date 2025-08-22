using LibraryManagementSystem.BusinessLogic.Interfaces;
using LibraryManagementSystem.DataAccess.Interfaces;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.BusinessLogic.Services
{
    public class BookManagerNew : BaseService, IBookCRUD, IBookLending
    {
        public BookManagerNew(IDataRepository dataRepository) : base(dataRepository)
        {
        }

        public async Task<bool> AddBookAsync(string title, string author, int publicationYear, string? isbn = null)
        {
            var newBook = new Book(title, author, publicationYear, isbn);
            return await _dataRepository.AddBookAsync(newBook);
        }

        public async Task<bool> UpdateBookAsync(string isbn, Book bookData)
        {
            var book = await _dataRepository.GetBookByIsbnAsync(isbn);
            if (!IsEntityFound(book))
                return false;

            book.Title = bookData.Title;
            book.Author = bookData.Author;
            book.PublicationYear = bookData.PublicationYear;
            book.ISBN = bookData.ISBN;
            book.LastUpdated = DateTime.UtcNow;

            return await _dataRepository.UpdateBookAsync(book);
        }

        public async Task<bool> DeleteBookAsync(string isbn)
        {
            var book = await _dataRepository.GetBookByIsbnAsync(isbn);
            if (!IsEntityFound(book) || book.Status == BookStatus.Borrowed)
                return false;

            return await _dataRepository.DeleteBookAsync(book.BookId);
        }

        // Direct repository calls - no additional business logic needed
        public async Task<Book?> GetBookByIsbnAsync(string isbn) => 
            await _dataRepository.GetBookByIsbnAsync(isbn);

        public async Task<List<Book>> GetAllBooksAsync() => 
            await _dataRepository.LoadBooksAsync();

        public async Task<List<Book>> SearchBooksAsync(string searchTerm) => 
            await _dataRepository.SearchBooksAsync(searchTerm);

        public async Task<bool> LendBookAsync(string userId, string isbn)
        {
            if (IsNullOrEmpty(userId) || IsNullOrEmpty(isbn))
                return false;

            var book = await _dataRepository.GetBookByIsbnAsync(isbn);
            if (!IsEntityFound(book) || book.Status == BookStatus.Borrowed)
                return false;

            book.Status = BookStatus.Borrowed;
            await _dataRepository.UpdateBookAsync(book);

            var newRecord = new BorrowingRecord(userId, book.BookId);
            return await _dataRepository.AddBorrowingRecordAsync(newRecord);
        }

        public async Task<bool> ReturnBookAsync(string userId, string isbn)
        {
            if (IsNullOrEmpty(userId) || IsNullOrEmpty(isbn))
                return false;

            var book = await _dataRepository.GetBookByIsbnAsync(isbn);
            if (!IsEntityFound(book) || book.Status == BookStatus.Available)
                return false;

            var record = await _dataRepository.GetActiveBorrowingRecordAsync(userId, book.BookId);
            if (!IsEntityFound(record))
                return false;

            book.Status = BookStatus.Available;
            record.ReturnDate = DateTime.UtcNow;
            record.Status = BorrowingStatus.Returned;

            await _dataRepository.UpdateBookAsync(book);
            return await _dataRepository.UpdateBorrowingRecordAsync(record);
        }

        public async Task<List<Book>> GetBorrowedBooksAsync(string userId)
        {
            var borrowingRecords = await _dataRepository.GetBorrowingRecordsByUserIdAsync(userId);
            var activeBorrowings = borrowingRecords.Where(r => r.Status == BorrowingStatus.Active).ToList();

            var borrowedBooks = new List<Book>();
            foreach (var record in activeBorrowings)
            {
                var book = await _dataRepository.GetBookByIdAsync(record.BookId);
                if (book != null)
                    borrowedBooks.Add(book);
            }

            return borrowedBooks;
        }

        public async Task<List<Book>> GetAvailableBooksAsync()
        {
            var books = await _dataRepository.LoadBooksAsync();
            return books.Where(b => b.Status == BookStatus.Available).ToList();
        }

        public async Task<List<BorrowingRecord>> GetBorrowingHistoryAsync(string userId)
        {
            return await _dataRepository.GetBorrowingRecordsByUserIdAsync(userId);
        }

        public async Task<bool> RequestBookAsync(string userId, string isbn)
        {
            var book = await _dataRepository.GetBookByIsbnAsync(isbn);
            if (book == null || book.Status != BookStatus.Available)
                return false;

            var requests = await _dataRepository.LoadBorrowingRequestsAsync();
            var existingRequest = requests.FirstOrDefault(r =>
                r.UserId == userId &&
                r.BookId == book.BookId &&
                r.Status == RequestStatus.Pending);

            if (existingRequest != null)
                return false;

            var newRequest = new BorrowingRequest(userId, book.BookId);
            return await _dataRepository.AddBorrowingRequestAsync(newRequest);
        }

        public async Task<List<BorrowingRequest>> GetPendingRequestsAsync()
        {
            return await _dataRepository.GetPendingRequestsAsync();
        }

        public async Task<bool> ApproveRequestAsync(string requestId, string adminId)
        {
            var request = await _dataRepository.GetRequestByIdAsync(requestId);
            if (request == null || request.Status != RequestStatus.Pending)
                return false;

            request.Status = RequestStatus.Approved;
            request.AdminResponseDate = DateTime.UtcNow;
            request.AdminId = adminId;

            await _dataRepository.UpdateBorrowingRequestAsync(request);

            var book = await _dataRepository.GetBookByIdAsync(request.BookId);
            if (book == null) return false;

            return await LendBookAsync(request.UserId, book.ISBN);
        }

        public async Task<bool> RejectRequestAsync(string requestId, string adminId)
        {
            var request = await _dataRepository.GetRequestByIdAsync(requestId);
            if (request == null || request.Status != RequestStatus.Pending)
                return false;

            request.Status = RequestStatus.Rejected;
            request.AdminResponseDate = DateTime.UtcNow;
            request.AdminId = adminId;

            return await _dataRepository.UpdateBorrowingRequestAsync(request);
        }

        // Synchronous wrappers for backward compatibility
        public bool AddBook(string title, string author, int publicationYear, string? isbn = null)
        {
            return AddBookAsync(title, author, publicationYear, isbn).GetAwaiter().GetResult();
        }

        public bool UpdateBook(string isbn, Book bookData)
        {
            return UpdateBookAsync(isbn, bookData).GetAwaiter().GetResult();
        }

        public bool DeleteBook(string isbn)
        {
            return DeleteBookAsync(isbn).GetAwaiter().GetResult();
        }

        public Book GetBookByIsbn(string isbn)
        {
            return GetBookByIsbnAsync(isbn).GetAwaiter().GetResult();
        }

        public List<Book> GetAllBooks()
        {
            return GetAllBooksAsync().GetAwaiter().GetResult();
        }

        public List<Book> SearchBooks(string searchTerm)
        {
            return SearchBooksAsync(searchTerm).GetAwaiter().GetResult();
        }

        public bool LendBook(string userId, string isbn)
        {
            return LendBookAsync(userId, isbn).GetAwaiter().GetResult();
        }

        public bool ReturnBook(string userId, string isbn)
        {
            return ReturnBookAsync(userId, isbn).GetAwaiter().GetResult();
        }

        public List<Book> GetBorrowedBooks(string userId)
        {
            return GetBorrowedBooksAsync(userId).GetAwaiter().GetResult();
        }

        public List<Book> GetAvailableBooks()
        {
            return GetAvailableBooksAsync().GetAwaiter().GetResult();
        }

        public List<BorrowingRecord> GetBorrowingHistory(string userId)
        {
            return GetBorrowingHistoryAsync(userId).GetAwaiter().GetResult();
        }

        public bool RequestBook(string userId, string isbn)
        {
            return RequestBookAsync(userId, isbn).GetAwaiter().GetResult();
        }

        public List<BorrowingRequest> GetPendingRequests()
        {
            return GetPendingRequestsAsync().GetAwaiter().GetResult();
        }

        public bool ApproveRequest(string requestId, string adminId)
        {
            return ApproveRequestAsync(requestId, adminId).GetAwaiter().GetResult();
        }

        public bool RejectRequest(string requestId, string adminId)
        {
            return RejectRequestAsync(requestId, adminId).GetAwaiter().GetResult();
        }
    }
}