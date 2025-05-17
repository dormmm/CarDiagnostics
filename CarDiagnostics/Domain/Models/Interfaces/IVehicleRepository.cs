using CarDiagnostics.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarDiagnostics.Domain.Interfaces
{
    public interface IVehicleRepository
    {
        Task<VehicleList> GetAllVehiclesAsync();
        Task<bool> IsCompanyExistsAsync(string company);
        Task<bool> IsModelExistsAsync(string company, string model);
        Task<bool> IsModelExistsNormalizedAsync(string company, string model); // ✅ חדש
        Task<List<string>> GetModelsByCompanyAsync(string company);
    }
}
