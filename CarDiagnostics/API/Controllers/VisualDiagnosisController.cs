using Microsoft.AspNetCore.Mvc;
using CarDiagnostics.Services;
using CarDiagnostics.DTOs;
using System.IO;
using System.Threading.Tasks;

namespace CarDiagnostics.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VisualDiagnosisController : ControllerBase
    {
        private readonly VisualDiagnosisService _service;

        public VisualDiagnosisController(VisualDiagnosisService service)
        {
            _service = service;
        }

        [HttpPost("analyze")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AnalyzeImageWithUpload([FromForm] VisualDiagnosisUploadDto dto)
        {
            using var ms = new MemoryStream();
            await dto.Image.CopyToAsync(ms);
            var imageBytes = ms.ToArray();
            var base64 = Convert.ToBase64String(imageBytes);

            var result = await _service.AnalyzeAsync(base64, dto.Description, dto.LicensePlate);
            return Ok(result);
        }
    }
}
