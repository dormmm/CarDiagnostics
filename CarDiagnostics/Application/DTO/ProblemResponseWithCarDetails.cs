using CarDiagnostics.Application.DTO;

namespace CarDiagnostics.Application.DTO

{
    public class ProblemResponseWithCarDetails
    {
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string ProblemDescription { get; set; }
        public string Solution { get; set; }
    }
}
