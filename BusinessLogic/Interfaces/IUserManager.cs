using LibraryManagementSystem.Models;
using System.Collections.Generic;

namespace LibraryManagementSystem.BusinessLogic.Interfaces
{
    public interface IUserManager
    {
        User AuthenticateUser(string username, string password);
        bool CreateUser(string username, string password, UserRole role);
        bool RegisterUser(string username, string password, UserRole role = UserRole.RegularUser);
        bool UpdateUser(string userId, User userData);
        bool DeleteUser(string userId);
        User GetUserById(string userId);
        User GetUserByUsername(string username);
        List<User> GetAllUsers();
        bool ChangePassword(string userId, string oldPassword, string newPassword);
    }
}