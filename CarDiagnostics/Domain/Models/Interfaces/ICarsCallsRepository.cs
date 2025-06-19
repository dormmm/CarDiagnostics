using CarDiagnostics.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarDiagnostics.Domain.Interfaces
{
    public interface ICarsCallsRepository
    {
        Task<List<Car>> ReadCallsAsync();
        Task SaveCallsAsync(List<Car> calls);

        
    }
}
