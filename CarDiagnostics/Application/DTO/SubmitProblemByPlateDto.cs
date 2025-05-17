using CarDiagnostics.Application.DTO;

namespace CarDiagnostics.Application.DTO
{
    public class SubmitProblemByPlateDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string LicensePlate { get; set; }
        public string ProblemDescription { get; set; }
    }
}
