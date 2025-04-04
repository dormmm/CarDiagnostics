using CarDiagnostics.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarDiagnostics.Services
{
    public interface IUserService
    {
        Task<List<User>> GetAllUsersAsync();
        Task RegisterAsync(string username, string password, string email);
        Task UpdateUserProfileAsync(int userId, string username, string email, string password);
        Task<bool> IsValidUserAsync(string username, string email);
    }
}
