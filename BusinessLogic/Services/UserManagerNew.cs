using LibraryManagementSystem.BusinessLogic.Interfaces;
using LibraryManagementSystem.DataAccess.Interfaces;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Utils;

namespace LibraryManagementSystem.BusinessLogic.Services
{
    public class UserManagerNew : IUserManager
    {
        private readonly IDataRepository _dataRepository;

        public UserManagerNew(IDataRepository dataRepository)
        {
            _dataRepository = dataRepository;
        }

        public async Task<User?> AuthenticateUserAsync(string username, string password)
        {
            var user = await _dataRepository.GetUserByUsernameAsync(username);
            if (user != null && SecurityHelper.VerifyPassword(password, user.PasswordHash))
            {
                return user;
            }
            return null;
        }

        public async Task<bool> RegisterUserAsync(string username, string password, UserRole role = UserRole.RegularUser)
        {
            // Check if username already exists
            var existingUser = await _dataRepository.GetUserByUsernameAsync(username);
            if (existingUser != null)
                return false;

            var passwordHash = SecurityHelper.HashPassword(password);
            var newUser = new User(username, passwordHash, role);
            
            return await _dataRepository.AddUserAsync(newUser);
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            return await _dataRepository.GetUserByIdAsync(userId);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _dataRepository.GetUserByUsernameAsync(username);
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _dataRepository.LoadUsersAsync();
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            return await _dataRepository.UpdateUserAsync(user);
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            return await _dataRepository.DeleteUserAsync(userId);
        }

        public async Task<bool> ChangePasswordAsync(string userId, string oldPassword, string newPassword)
        {
            var user = await _dataRepository.GetUserByIdAsync(userId);
            if (user == null)
                return false;

            if (!SecurityHelper.VerifyPassword(oldPassword, user.PasswordHash))
                return false;

            user.PasswordHash = SecurityHelper.HashPassword(newPassword);
            return await _dataRepository.UpdateUserAsync(user);
        }

        public async Task<List<User>> GetUsersByRoleAsync(UserRole role)
        {
            var users = await _dataRepository.LoadUsersAsync();
            return users.Where(u => u.Role == role).ToList();
        }

        // Synchronous wrappers for backward compatibility
        public User AuthenticateUser(string username, string password)
        {
            return AuthenticateUserAsync(username, password).GetAwaiter().GetResult();
        }

        public bool RegisterUser(string username, string password, UserRole role = UserRole.RegularUser)
        {
            return RegisterUserAsync(username, password, role).GetAwaiter().GetResult();
        }

        public User GetUserById(string userId)
        {
            return GetUserByIdAsync(userId).GetAwaiter().GetResult();
        }

        public User GetUserByUsername(string username)
        {
            return GetUserByUsernameAsync(username).GetAwaiter().GetResult();
        }

        public List<User> GetAllUsers()
        {
            return GetAllUsersAsync().GetAwaiter().GetResult();
        }

        public bool UpdateUser(User user)
        {
            return UpdateUserAsync(user).GetAwaiter().GetResult();
        }

        public bool DeleteUser(string userId)
        {
            return DeleteUserAsync(userId).GetAwaiter().GetResult();
        }

        public bool ChangePassword(string userId, string oldPassword, string newPassword)
        {
            return ChangePasswordAsync(userId, oldPassword, newPassword).GetAwaiter().GetResult();
        }

        public List<User> GetUsersByRole(UserRole role)
        {
            return GetUsersByRoleAsync(role).GetAwaiter().GetResult();
        }

        // Additional interface methods
        public bool CreateUser(string username, string password, UserRole role)
        {
            return RegisterUserAsync(username, password, role).GetAwaiter().GetResult();
        }

        public bool UpdateUser(string userId, User userData)
        {
            return UpdateUserAsync(userData).GetAwaiter().GetResult();
        }
    }
}