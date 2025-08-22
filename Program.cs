using LibraryManagementSystem.BusinessLogic.Interfaces;
using LibraryManagementSystem.BusinessLogic.Services;
using LibraryManagementSystem.DataAccess;
using LibraryManagementSystem.DataAccess.Interfaces;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem
{
    class Program
    {
        private static IUserManager _userManager;
        private static IBookCRUD _bookCRUD;
        private static IBookLending _bookLending;
        private static User _currentUser;
        private static IDataRepository _dataRepository;

        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("=== Library Management System ===");
                Console.WriteLine("Initializing data access...");

                // Initialize data repository from configuration
                var factory = DataRepositoryFactory.Create();
                _dataRepository = await factory.CreateRepositoryAsync();

                // Initialize services
                var userManager = new UserManagerNew(_dataRepository);
                var bookManager = new BookManagerNew(_dataRepository);

                _userManager = userManager;
                _bookCRUD = bookManager;
                _bookLending = bookManager;

                // Display current data source
                await DisplayDataSourceInfo();

                await ShowMainMenu();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing system: {ex.Message}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }

        private static async Task DisplayDataSourceInfo()
        {
            var connectionTest = await _dataRepository.TestConnectionAsync();
            var dataSourceType = _dataRepository.GetType().Name;

            Console.WriteLine($"Data Source: {dataSourceType}");
            Console.WriteLine($"Connection Status: {(connectionTest ? "✓ Connected" : "✗ Failed")}");
            Console.WriteLine();
        }

        private static async Task ShowMainMenu()
        {
            while (true)
            {
                Console.WriteLine("\n=== Main Menu ===");
                if (_currentUser == null)
                {
                    Console.WriteLine("1. Login");
                    Console.WriteLine("2. Register");
                    Console.WriteLine("3. Switch Data Source");
                    Console.WriteLine("0. Exit");
                }
                else
                {
                    Console.WriteLine($"Welcome, {_currentUser.Username} ({_currentUser.Role})");

                    if (_currentUser.Role == UserRole.Admin)
                    {
                        // Admin Menu
                        Console.WriteLine("1. List Books");
                        Console.WriteLine("2. Add Book");
                        Console.WriteLine("3. Edit Book");
                        Console.WriteLine("4. Delete Book");
                        Console.WriteLine("5. Manage Loan Requests");
                        Console.WriteLine("6. Manage Returns");
                        Console.WriteLine("7. View All Users");
                        Console.WriteLine("8. Logout");
                        Console.WriteLine("0. Exit");
                    }
                    else
                    {
                        // Regular User Menu
                        Console.WriteLine("1. List Available Books");
                        Console.WriteLine("2. Send Loan Request");
                        Console.WriteLine("3. My Borrowed Books");
                        Console.WriteLine("4. Return Book");
                        Console.WriteLine("5. Logout");
                        Console.WriteLine("0. Exit");
                    }
                }

                var choice = Console.ReadLine();
                await HandleMenuChoice(choice);
            }
        }

        private static async Task HandleMenuChoice(string choice)
        {
            try
            {
                // Handle login menu
                if (_currentUser == null)
                {
                    switch (choice)
                    {
                        case "1":
                            await LoginAsync();
                            break;
                        case "2":
                            await RegisterAsync();
                            break;
                        case "3":
                            await SwitchDataSourceAsync();
                            break;
                        case "0":
                            Environment.Exit(0);
                            break;
                        default:
                            Console.WriteLine("Invalid choice. Please try again.");
                            break;
                    }
                }
                // Handle admin menu
                else if (_currentUser.Role == UserRole.Admin)
                {
                    switch (choice)
                    {
                        case "1":
                            await ViewAllBooksAsync();
                            break;
                        case "2":
                            await AddBookAsync();
                            break;
                        case "3":
                            await UpdateBookAsync();
                            break;
                        case "4":
                            await DeleteBookAsync();
                            break;
                        case "5":
                            await ManageLoanRequestsAsync();
                            break;
                        case "6":
                            await ManageReturnsAsync();
                            break;
                        case "7":
                            await ViewAllUsersAsync();
                            break;
                        case "8":
                            _currentUser = null;
                            break;
                        case "0":
                            Environment.Exit(0);
                            break;
                        default:
                            Console.WriteLine("Invalid choice. Please try again.");
                            break;
                    }
                }
                // Handle regular user menu
                else
                {
                    switch (choice)
                    {
                        case "1":
                            await ListAvailableBooksAsync();
                            break;
                        case "2":
                            await SendLoanRequestAsync();
                            break;
                        case "3":
                            await ViewMyBorrowedBooksAsync();
                            break;
                        case "4":
                            await ReturnBookAsync();
                            break;
                        case "5":
                            _currentUser = null;
                            break;
                        case "0":
                            Environment.Exit(0);
                            break;
                        default:
                            Console.WriteLine("Invalid choice. Please try again.");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private static async Task SwitchDataSourceAsync()
        {
            Console.WriteLine("\n=== Switch Data Source ===");
            Console.WriteLine("Current Data Source: " + _dataRepository.GetType().Name);
            Console.WriteLine("1. File (JSON)");
            Console.WriteLine("2. PostgreSQL Database");
            Console.Write("Select new data source: ");

            var choice = Console.ReadLine();
            var newDataSource = choice == "2" ? "PostgreSQL" : "File";

            if (ShouldSwitchDataSource(newDataSource))
            {
                await SwitchToNewDataSourceAsync(newDataSource);
            }
            else
            {
                Console.WriteLine("You're already using this data source.");
            }
        }

        private static bool ShouldSwitchDataSource(string newDataSource)
        {
            var currentType = _dataRepository.GetType().Name;
            return (newDataSource.ToLower() == "postgresql" && currentType != "PostgreSqlDataRepository") ||
                   (newDataSource.ToLower() == "file" && currentType != "FileDataRepository");
        }

        private static async Task SwitchToNewDataSourceAsync(string newDataSource)
        {
            try
            {
                Console.WriteLine($"Switching to {newDataSource}...");

                // Preserve current user session
                var currentUser = _currentUser;

                // Dispose current repository if it implements IDisposable
                if (_dataRepository is IDisposable disposableRepo)
                {
                    disposableRepo.Dispose();
                }

                // Create new repository
                var factory = DataRepositoryFactory.Create();
                var newRepository = await factory.CreateRepositoryAsync(newDataSource);

                // Update repository reference
                _dataRepository = newRepository;

                // Recreate service instances with new repository
                var userManager = new UserManagerNew(_dataRepository);
                var bookManager = new BookManagerNew(_dataRepository);

                _userManager = userManager;
                _bookCRUD = bookManager;
                _bookLending = bookManager;

                // Restore user session if valid
                if (currentUser != null)
                {
                    var restoredUser = await userManager.GetUserByIdAsync(currentUser.UserId);
                    _currentUser = restoredUser ?? null;

                    if (_currentUser == null)
                    {
                        Console.WriteLine("Warning: Your session was lost during the switch. Please login again.");
                    }
                }

                Console.WriteLine($"Successfully switched to {newDataSource}!");
                await DisplayDataSourceInfo();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to switch data source: {ex.Message}");
                Console.WriteLine("Continuing with current data source...");
            }
        }

        private static async Task LoginAsync()
        {
            Console.WriteLine("\n=== Login ===");
            Console.Write("Username: ");
            var username = Console.ReadLine();
            Console.Write("Password: ");
            var password = Console.ReadLine();

            var userManager = (UserManagerNew)_userManager;
            _currentUser = await userManager.AuthenticateUserAsync(username, password);

            if (_currentUser != null)
            {
                Console.WriteLine($"Login successful! Welcome, {_currentUser.Username}.");
            }
            else
            {
                Console.WriteLine("Invalid username or password.");
            }
        }

        private static async Task RegisterAsync()
        {
            Console.WriteLine("\n=== Register ===");
            Console.Write("Username: ");
            var username = Console.ReadLine();
            Console.Write("Password: ");
            var password = Console.ReadLine();

            Console.WriteLine("\nSelect User Role:");
            Console.WriteLine("1. Normal User");
            Console.WriteLine("2. Admin");
            Console.Write("Choose role (1-2): ");
            var roleChoice = Console.ReadLine();

            UserRole role = roleChoice switch
            {
                "2" => UserRole.Admin,
                _ => UserRole.RegularUser
            };

            var userManager = (UserManagerNew)_userManager;
            var success = await userManager.RegisterUserAsync(username, password, role);

            if (success)
            {
                Console.WriteLine($"Registration successful! User created as {role}. You can now login.");
            }
            else
            {
                Console.WriteLine("Registration failed. Username might already exist.");
            }
        }

        private static async Task ViewAllBooksAsync()
        {
            Console.WriteLine("\n=== All Books ===");
            var bookManager = (BookManagerNew)_bookCRUD;
            var books = await bookManager.GetAllBooksAsync();

            if (books.Any())
            {
                foreach (var book in books)
                {
                    Console.WriteLine($"[{book.Status}] {book.Title} by {book.Author} ({book.PublicationYear}) - ISBN: {book.ISBN}");
                }
            }
            else
            {
                Console.WriteLine("No books available.");
            }
        }


        private static async Task ViewMyBorrowedBooksAsync()
        {
            Console.WriteLine("\n=== My Borrowed Books ===");
            var bookManager = (BookManagerNew)_bookLending;
            var books = await bookManager.GetBorrowedBooksAsync(_currentUser.UserId);

            if (books.Any())
            {
                foreach (var book in books)
                {
                    Console.WriteLine($"{book.Title} by {book.Author} - ISBN: {book.ISBN}");
                }
            }
            else
            {
                Console.WriteLine("You have no borrowed books.");
            }
        }

        private static async Task AddBookAsync()
        {
            Console.WriteLine("\n=== Add Book ===");
            Console.Write("Title: ");
            var title = Console.ReadLine();
            Console.Write("Author: ");
            var author = Console.ReadLine();
            Console.Write("Publication Year: ");
            if (int.TryParse(Console.ReadLine(), out int year))
            {
                Console.Write("ISBN (optional): ");
                var isbn = Console.ReadLine();

                var bookManager = (BookManagerNew)_bookCRUD;
                var success = await bookManager.AddBookAsync(title, author, year, isbn);

                if (success)
                {
                    Console.WriteLine("Book added successfully!");
                }
                else
                {
                    Console.WriteLine("Failed to add book.");
                }
            }
            else
            {
                Console.WriteLine("Invalid publication year.");
            }
        }

        private static async Task UpdateBookAsync()
        {
            Console.WriteLine("\n=== Update Book ===");
            Console.Write("Enter ISBN of book to update: ");
            var isbn = Console.ReadLine();

            var bookManager = (BookManagerNew)_bookCRUD;
            var book = await bookManager.GetBookByIsbnAsync(isbn);

            if (book != null)
            {
                Console.WriteLine($"Current: {book.Title} by {book.Author} ({book.PublicationYear})");
                Console.Write("New Title (press enter to keep current): ");
                var newTitle = Console.ReadLine();
                Console.Write("New Author (press enter to keep current): ");
                var newAuthor = Console.ReadLine();
                Console.Write("New Publication Year (press enter to keep current): ");
                var newYearStr = Console.ReadLine();

                var updatedBook = new Book(
                    string.IsNullOrWhiteSpace(newTitle) ? book.Title : newTitle,
                    string.IsNullOrWhiteSpace(newAuthor) ? book.Author : newAuthor,
                    string.IsNullOrWhiteSpace(newYearStr) ? book.PublicationYear : int.Parse(newYearStr),
                    book.ISBN
                );

                var success = await bookManager.UpdateBookAsync(isbn, updatedBook);
                if (success)
                {
                    Console.WriteLine("Book updated successfully!");
                }
                else
                {
                    Console.WriteLine("Failed to update book.");
                }
            }
            else
            {
                Console.WriteLine("Book not found.");
            }
        }

        private static async Task DeleteBookAsync()
        {
            Console.WriteLine("\n=== Delete Book ===");
            Console.Write("Enter ISBN of book to delete: ");
            var isbn = Console.ReadLine();

            var bookManager = (BookManagerNew)_bookCRUD;
            var success = await bookManager.DeleteBookAsync(isbn);

            if (success)
            {
                Console.WriteLine("Book deleted successfully!");
            }
            else
            {
                Console.WriteLine("Failed to delete book. It might not exist or be currently borrowed.");
            }
        }

        private static async Task ManageLoanRequestsAsync()
        {
            Console.WriteLine("\n=== Manage Loan Requests ===");
            var bookManager = (BookManagerNew)_bookLending;
            var userManager = (UserManagerNew)_userManager;
            var requests = await bookManager.GetPendingRequestsAsync();

            if (!requests.Any())
            {
                Console.WriteLine("No pending loan requests.");
                return;
            }

            Console.WriteLine($"Found {requests.Count} pending request(s):\n");

            // Display all requests with detailed information
            for (int i = 0; i < requests.Count; i++)
            {
                var request = requests[i];
                var user = await userManager.GetUserByIdAsync(request.UserId);
                var book = await _dataRepository.GetBookByIdAsync(request.BookId);

                Console.WriteLine($"[{i + 1}] Request from: {user?.Username ?? "Unknown User"}");
                Console.WriteLine($"    Book: {book?.Title ?? "Unknown Book"} by {book?.Author ?? "Unknown Author"}");
                Console.WriteLine($"    ISBN: {book?.ISBN ?? "N/A"}");
                Console.WriteLine($"    Request Date: {request.RequestDate:yyyy-MM-dd HH:mm}");
                Console.WriteLine($"    Request ID: {request.RequestId}");
                Console.WriteLine();
            }

            Console.WriteLine("Options:");
            Console.WriteLine("1. Approve a request");
            Console.WriteLine("2. Reject a request");
            Console.WriteLine("0. Back to main menu");
            Console.Write("Choose option: ");

            var choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    await ApproveRequestAsync(requests);
                    break;
                case "2":
                    await RejectRequestAsync(requests);
                    break;
            }
        }

        private static async Task ApproveRequestAsync(List<BorrowingRequest> requests)
        {
            Console.WriteLine("\n=== Approve Request ===");
            Console.Write($"Enter request number to approve (1-{requests.Count}): ");

            if (int.TryParse(Console.ReadLine(), out int requestNumber) &&
                requestNumber >= 1 && requestNumber <= requests.Count)
            {
                var request = requests[requestNumber - 1];
                var bookManager = (BookManagerNew)_bookLending;

                var success = await bookManager.ApproveRequestAsync(request.RequestId, _currentUser.UserId);

                if (success)
                {
                    Console.WriteLine("Request approved successfully! The book has been lent to the user.");
                }
                else
                {
                    Console.WriteLine("Failed to approve request. The book might not be available or an error occurred.");
                }
            }
            else
            {
                Console.WriteLine("Invalid request number.");
            }
        }

        private static async Task RejectRequestAsync(List<BorrowingRequest> requests)
        {
            Console.WriteLine("\n=== Reject Request ===");
            Console.Write($"Enter request number to reject (1-{requests.Count}): ");

            if (int.TryParse(Console.ReadLine(), out int requestNumber) &&
                requestNumber >= 1 && requestNumber <= requests.Count)
            {
                var request = requests[requestNumber - 1];
                var bookManager = (BookManagerNew)_bookLending;

                var success = await bookManager.RejectRequestAsync(request.RequestId, _currentUser.UserId);

                if (success)
                {
                    Console.WriteLine("Request rejected successfully.");
                }
                else
                {
                    Console.WriteLine("Failed to reject request. An error occurred.");
                }
            }
            else
            {
                Console.WriteLine("Invalid request number.");
            }
        }

        private static async Task ViewAllUsersAsync()
        {
            Console.WriteLine("\n=== All Users ===");
            var userManager = (UserManagerNew)_userManager;
            var users = await userManager.GetAllUsersAsync();

            foreach (var user in users)
            {
                Console.WriteLine($"{user.Username} - Role: {user.Role}");
            }
        }

        private static async Task ListAvailableBooksAsync()
        {
            Console.WriteLine("\n=== Available Books ===");
            var bookManager = (BookManagerNew)_bookCRUD;
            var books = await bookManager.GetAvailableBooksAsync();

            if (books.Any())
            {
                foreach (var book in books)
                {
                    Console.WriteLine($"[AVAILABLE] {book.Title} by {book.Author} ({book.PublicationYear}) - ISBN: {book.ISBN ?? "N/A"}");
                }
            }
            else
            {
                Console.WriteLine("No books are currently available for borrowing.");
            }
        }

        private static async Task SendLoanRequestAsync()
        {
            Console.WriteLine("\n=== Send Loan Request ===");
            Console.Write("Enter ISBN of book to request: ");
            var isbn = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(isbn))
            {
                Console.WriteLine("ISBN cannot be empty.");
                return;
            }

            var bookManager = (BookManagerNew)_bookLending;
            var book = await bookManager.GetBookByIsbnAsync(isbn);

            if (book == null)
            {
                Console.WriteLine("Book not found.");
                return;
            }

            if (book.Status == BookStatus.Available)
            {
                var success = await bookManager.RequestBookAsync(_currentUser.UserId, isbn);
                if (success)
                {
                    Console.WriteLine("Loan request submitted successfully! Please wait for admin approval.");
                }
                else
                {
                    Console.WriteLine("Failed to submit loan request. You may have already requested this book.");
                }
            }
            else
            {
                Console.WriteLine("This book is currently borrowed and not available for new requests.");
            }
        }

        private static async Task ReturnBookAsync()
        {
            Console.WriteLine("\n=== Return Book ===");

            // First, show user's borrowed books
            var bookManager = (BookManagerNew)_bookLending;
            var borrowedBooks = await bookManager.GetBorrowedBooksAsync(_currentUser.UserId);

            if (!borrowedBooks.Any())
            {
                Console.WriteLine("You have no borrowed books to return.");
                return;
            }

            Console.WriteLine("Your borrowed books:");
            foreach (var book in borrowedBooks)
            {
                Console.WriteLine($"- {book.Title} by {book.Author} (ISBN: {book.ISBN ?? "N/A"})");
            }

            Console.Write("Enter ISBN of book to return: ");
            var isbn = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(isbn))
            {
                Console.WriteLine("ISBN cannot be empty.");
                return;
            }

            var success = await bookManager.ReturnBookAsync(_currentUser.UserId, isbn);
            if (success)
            {
                Console.WriteLine("Book returned successfully!");
            }
            else
            {
                Console.WriteLine("Failed to return book. Please check the ISBN or contact an admin.");
            }
        }

        private static async Task ManageReturnsAsync()
        {
            Console.WriteLine("\n=== Manage Returns ===");
            var bookManager = (BookManagerNew)_bookLending;

            // Get all active borrowing records (books that are currently borrowed)
            var allBorrowingRecords = await _dataRepository.LoadBorrowingRecordsAsync();
            var activeBorrowings = allBorrowingRecords.Where(r => r.Status == BorrowingStatus.Active).ToList();

            if (!activeBorrowings.Any())
            {
                Console.WriteLine("No books are currently borrowed.");
                return;
            }

            Console.WriteLine("Currently borrowed books:");
            foreach (var record in activeBorrowings)
            {
                var user = await _dataRepository.GetUserByIdAsync(record.UserId);
                var book = await _dataRepository.GetBookByIdAsync(record.BookId);

                if (user != null && book != null)
                {
                    var overdue = record.DueDate < DateTime.UtcNow ? " [OVERDUE]" : "";
                    Console.WriteLine($"- {book.Title} borrowed by {user.Username} (Due: {record.DueDate:yyyy-MM-dd}){overdue} - ISBN: {book.ISBN ?? "N/A"}");
                }
            }

            Console.WriteLine("\nOptions:");
            Console.WriteLine("1. Process book return");
            Console.WriteLine("2. View overdue books");
            Console.WriteLine("0. Back to main menu");
            Console.Write("Choose option: ");

            var choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    await ProcessBookReturnAsync();
                    break;
                case "2":
                    await ViewOverdueBooksAsync();
                    break;
            }
        }

        private static async Task ProcessBookReturnAsync()
        {
            Console.WriteLine("\n=== Process Book Return ===");
            Console.Write("Enter username of person returning book: ");
            var username = Console.ReadLine();
            Console.Write("Enter ISBN of book being returned: ");
            var isbn = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(isbn))
            {
                Console.WriteLine("Username and ISBN cannot be empty.");
                return;
            }

            var userManager = (UserManagerNew)_userManager;
            var user = await userManager.GetUserByUsernameAsync(username);

            if (user == null)
            {
                Console.WriteLine("User not found.");
                return;
            }

            var bookManager = (BookManagerNew)_bookLending;
            var success = await bookManager.ReturnBookAsync(user.UserId, isbn);

            if (success)
            {
                Console.WriteLine("Book return processed successfully!");
            }
            else
            {
                Console.WriteLine("Failed to process return. Please check if the user has borrowed this book.");
            }
        }

        private static async Task ViewOverdueBooksAsync()
        {
            Console.WriteLine("\n=== Overdue Books ===");
            var allBorrowingRecords = await _dataRepository.LoadBorrowingRecordsAsync();
            var overdueBorrowings = allBorrowingRecords
                .Where(r => r.Status == BorrowingStatus.Active && r.DueDate < DateTime.UtcNow)
                .ToList();

            if (!overdueBorrowings.Any())
            {
                Console.WriteLine("No overdue books.");
                return;
            }

            foreach (var record in overdueBorrowings)
            {
                var user = await _dataRepository.GetUserByIdAsync(record.UserId);
                var book = await _dataRepository.GetBookByIdAsync(record.BookId);

                if (user != null && book != null)
                {
                    var daysOverdue = (DateTime.UtcNow - record.DueDate).Days;
                    Console.WriteLine($"- {book.Title} by {user.Username} ({daysOverdue} days overdue) - ISBN: {book.ISBN ?? "N/A"}");
                }
            }
        }
    }
}