using LibraryManagementSystem.DataAccess.Interfaces;
using LibraryManagementSystem.Models;
using System.Text.Json;

namespace LibraryManagementSystem.DataAccess
{
    public class FileDataRepository : IDataRepository
    {
        private readonly string _dataDirectory;
        private readonly string _usersFilePath;
        private readonly string _booksFilePath;
        private readonly string _borrowingRecordsFilePath;
        private readonly string _borrowingRequestsFilePath;

        public FileDataRepository(string dataDirectory = "Data")
        {
            _dataDirectory = dataDirectory;
            _usersFilePath = "/Users/efeugur/Desktop/Kütüphane Sistemi/Data/users.json";
            _booksFilePath = "/Users/efeugur/Desktop/Kütüphane Sistemi/Data/books.json";
            _borrowingRecordsFilePath = "/Users/efeugur/Desktop/Kütüphane Sistemi/Data/borrowing_records.json";
            _borrowingRequestsFilePath = "/Users/efeugur/Desktop/Kütüphane Sistemi/Data/borrowing_requests.json";

            if (!Directory.Exists(_dataDirectory))
            {
                Directory.CreateDirectory(_dataDirectory);
            }

            InitializeDataFiles();
        }

        private void InitializeDataFiles()
        {
            if (!File.Exists(_usersFilePath))
            {
                SaveUsersAsync(new List<User>()).Wait();
            }

            if (!File.Exists(_booksFilePath))
            {
                SaveBooksAsync(new List<Book>()).Wait();
            }

            if (!File.Exists(_borrowingRecordsFilePath))
            {
                SaveBorrowingRecordsAsync(new List<BorrowingRecord>()).Wait();
            }

            if (!File.Exists(_borrowingRequestsFilePath))
            {
                SaveBorrowingRequestsAsync(new List<BorrowingRequest>()).Wait();
            }
        }

        public async Task InitializeAsync()
        {
            // For file storage, initialization is done in constructor
            await Task.CompletedTask;
        }

        public async Task<bool> TestConnectionAsync()
        {
            // For file storage, test if we can read/write to directory
            try
            {
                var testFile = Path.Combine(_dataDirectory, "test.tmp");
                await File.WriteAllTextAsync(testFile, "test");
                File.Delete(testFile);
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
            try
            {
                if (!File.Exists(_usersFilePath))
                    return new List<User>();

                var json = await File.ReadAllTextAsync(_usersFilePath);
                return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading users: {ex.Message}");
                return new List<User>();
            }
        }

        public async Task SaveUsersAsync(List<User> users)
        {
            try
            {
                var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_usersFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving users: {ex.Message}");
            }
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            var users = await LoadUsersAsync();
            return users.FirstOrDefault(u => u.UserId == userId);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            var users = await LoadUsersAsync();
            return users.FirstOrDefault(u => u.Username == username);
        }

        public async Task<bool> AddUserAsync(User user)
        {
            try
            {
                var users = await LoadUsersAsync();
                users.Add(user);
                await SaveUsersAsync(users);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                var users = await LoadUsersAsync();
                var existingUser = users.FirstOrDefault(u => u.UserId == user.UserId);
                if (existingUser == null) return false;

                var index = users.IndexOf(existingUser);
                users[index] = user;
                await SaveUsersAsync(users);
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
                var users = await LoadUsersAsync();
                var user = users.FirstOrDefault(u => u.UserId == userId);
                if (user == null) return false;

                users.Remove(user);
                await SaveUsersAsync(users);
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
            try
            {
                if (!File.Exists(_booksFilePath))
                    return new List<Book>();

                var json = await File.ReadAllTextAsync(_booksFilePath);
                return JsonSerializer.Deserialize<List<Book>>(json) ?? new List<Book>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading books: {ex.Message}");
                return new List<Book>();
            }
        }

        public async Task SaveBooksAsync(List<Book> books)
        {
            try
            {
                var json = JsonSerializer.Serialize(books, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_booksFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving books: {ex.Message}");
            }
        }

        public async Task<Book?> GetBookByIdAsync(string bookId)
        {
            var books = await LoadBooksAsync();
            return books.FirstOrDefault(b => b.BookId == bookId);
        }

        public async Task<Book?> GetBookByIsbnAsync(string isbn)
        {
            var books = await LoadBooksAsync();
            return books.FirstOrDefault(b => b.ISBN == isbn);
        }

        public async Task<List<Book>> SearchBooksAsync(string searchTerm)
        {
            var books = await LoadBooksAsync();
            return books.Where(b =>
                b.Title.ToLower().Contains(searchTerm.ToLower()) ||
                b.Author.ToLower().Contains(searchTerm.ToLower()) ||
                (b.ISBN?.ToLower().Contains(searchTerm.ToLower()) ?? false)
            ).ToList();
        }

        public async Task<bool> AddBookAsync(Book book)
        {
            try
            {
                var books = await LoadBooksAsync();
                books.Add(book);
                await SaveBooksAsync(books);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateBookAsync(Book book)
        {
            try
            {
                var books = await LoadBooksAsync();
                var existingBook = books.FirstOrDefault(b => b.BookId == book.BookId);
                if (existingBook == null) return false;

                var index = books.IndexOf(existingBook);
                books[index] = book;
                await SaveBooksAsync(books);
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
                var books = await LoadBooksAsync();
                var book = books.FirstOrDefault(b => b.BookId == bookId);
                if (book == null) return false;

                books.Remove(book);
                await SaveBooksAsync(books);
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
            try
            {
                if (!File.Exists(_borrowingRecordsFilePath))
                    return new List<BorrowingRecord>();

                var json = await File.ReadAllTextAsync(_borrowingRecordsFilePath);
                return JsonSerializer.Deserialize<List<BorrowingRecord>>(json) ?? new List<BorrowingRecord>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading borrowing records: {ex.Message}");
                return new List<BorrowingRecord>();
            }
        }

        public async Task SaveBorrowingRecordsAsync(List<BorrowingRecord> records)
        {
            try
            {
                var json = JsonSerializer.Serialize(records, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_borrowingRecordsFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving borrowing records: {ex.Message}");
            }
        }

        public async Task<List<BorrowingRecord>> GetBorrowingRecordsByUserIdAsync(string userId)
        {
            var records = await LoadBorrowingRecordsAsync();
            return records.Where(r => r.UserId == userId).ToList();
        }

        public async Task<BorrowingRecord?> GetActiveBorrowingRecordAsync(string userId, string bookId)
        {
            var records = await LoadBorrowingRecordsAsync();
            return records.FirstOrDefault(r => r.UserId == userId &&
                                              r.BookId == bookId &&
                                              r.Status == BorrowingStatus.Active);
        }

        public async Task<bool> AddBorrowingRecordAsync(BorrowingRecord record)
        {
            try
            {
                var records = await LoadBorrowingRecordsAsync();
                records.Add(record);
                await SaveBorrowingRecordsAsync(records);
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
                var records = await LoadBorrowingRecordsAsync();
                var existingRecord = records.FirstOrDefault(r => r.RecordId == record.RecordId);
                if (existingRecord == null) return false;

                var index = records.IndexOf(existingRecord);
                records[index] = record;
                await SaveBorrowingRecordsAsync(records);
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
            try
            {
                if (!File.Exists(_borrowingRequestsFilePath))
                    return new List<BorrowingRequest>();

                var json = await File.ReadAllTextAsync(_borrowingRequestsFilePath);
                return JsonSerializer.Deserialize<List<BorrowingRequest>>(json) ?? new List<BorrowingRequest>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading borrowing requests: {ex.Message}");
                return new List<BorrowingRequest>();
            }
        }

        public async Task SaveBorrowingRequestsAsync(List<BorrowingRequest> requests)
        {
            try
            {
                var json = JsonSerializer.Serialize(requests, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_borrowingRequestsFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving borrowing requests: {ex.Message}");
            }
        }

        public async Task<List<BorrowingRequest>> GetPendingRequestsAsync()
        {
            var requests = await LoadBorrowingRequestsAsync();
            return requests.Where(r => r.Status == RequestStatus.Pending).ToList();
        }

        public async Task<BorrowingRequest?> GetRequestByIdAsync(string requestId)
        {
            var requests = await LoadBorrowingRequestsAsync();
            return requests.FirstOrDefault(r => r.RequestId == requestId);
        }

        public async Task<bool> AddBorrowingRequestAsync(BorrowingRequest request)
        {
            try
            {
                var requests = await LoadBorrowingRequestsAsync();
                requests.Add(request);
                await SaveBorrowingRequestsAsync(requests);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateBorrowingRequestAsync(BorrowingRequest request)
        {
            try
            {
                var requests = await LoadBorrowingRequestsAsync();
                var existingRequest = requests.FirstOrDefault(r => r.RequestId == request.RequestId);
                if (existingRequest == null) return false;

                var index = requests.IndexOf(existingRequest);
                requests[index] = request;
                await SaveBorrowingRequestsAsync(requests);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteBorrowingRequestAsync(string requestId)
        {
            try
            {
                var requests = await LoadBorrowingRequestsAsync();
                var request = requests.FirstOrDefault(r => r.RequestId == requestId);
                if (request == null) return false;

                requests.Remove(request);
                await SaveBorrowingRequestsAsync(requests);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}