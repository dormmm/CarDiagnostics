using System.Collections.Generic;
using CarDiagnostics.Models;

namespace CarDiagnostics.Services
{
    public interface IUserService
    {
        void Register(string username, string password, string email);
        List<User> GetAllUsers();
        void UpdateUserProfile(int userId, string name, string email, string password = null); // פונקציה לעדכון פרטי משתמש
    }
}
