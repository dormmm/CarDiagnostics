using System.Text.Json;

namespace CarDiagnostics.Services
{
    public class LicensePlateService
    {
        private readonly string _filePath;
        private JsonElement _rootElement;
        private bool _isLoaded = false;

        public LicensePlateService()
        {
            _filePath = Path.Combine(Directory.GetCurrentDirectory(), "license_plate_data.json");

            if (!File.Exists(_filePath))
                throw new FileNotFoundException("license_plate_data.json not found", _filePath);
        }

        public Dictionary<string, object>? GetCarByPlate(string plate)
        {
            plate = plate.PadLeft(7, '0');

            if (!_isLoaded)
            {
                using var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read);
                using var doc = JsonDocument.Parse(fs);
                _rootElement = doc.RootElement.Clone();
                _isLoaded = true;
            }

            if (_rootElement.TryGetProperty(plate, out var element))
            {
                var json = element.GetRawText();
                return JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            }

            return null;
        }
    }
}
