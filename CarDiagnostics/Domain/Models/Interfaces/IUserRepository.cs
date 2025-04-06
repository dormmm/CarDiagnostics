using CarDiagnostics.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarDiagnostics.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllUsersAsync();
        Task SaveUsersAsync(List<User> users);
        Task<bool> IsValidUserAsync(string username, string email);
    }
}
