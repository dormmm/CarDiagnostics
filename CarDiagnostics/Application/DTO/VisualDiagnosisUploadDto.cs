using Microsoft.AspNetCore.Http;

namespace CarDiagnostics.DTOs
{
    public class VisualDiagnosisUploadDto
    {
        public IFormFile Image { get; set; }
        public string Description { get; set; }
        public string LicensePlate { get; set; }
    }
}
