using System.Collections.Generic;
using CarDiagnostics.Models;

namespace CarDiagnostics.Services
{
    public interface IUserService
    {
        void Register(string username, string password, string email);
        List<User> GetAllUsers();
    }
}
