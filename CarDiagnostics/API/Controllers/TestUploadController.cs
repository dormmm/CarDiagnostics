using CarDiagnostics.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CarDiagnostics.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestUploadController : ControllerBase
    {
        private readonly AzureStorageService _storageService;

        public TestUploadController(AzureStorageService storageService)
        {
            _storageService = storageService;
        }

        [HttpPost("upload-test")]
        public async Task<IActionResult> UploadTest()
        {
            string fileName = "test-users.json";
            string content = "{ \"message\": \"Hello from Azure Blob!\" }";

            await _storageService.UploadFileAsync(fileName, content);

            return Ok("File uploaded successfully to Azure Blob Storage.");
        }
    }
}
