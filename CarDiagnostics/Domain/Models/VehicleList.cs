namespace CarDiagnostics.Models
{
    public class VehicleModel
    {
        public string model { get; set; }
    }

    public class VehicleList : Dictionary<string, List<VehicleModel>>
    {
    }
}
