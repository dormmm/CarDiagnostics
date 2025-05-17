using System.Text.Json;

namespace CarDiagnostics.Services
{
    public class LicensePlateService
    {
        private readonly string _filePath;
        private JsonElement _rootElement;
        private bool _isLoaded = false;

        // מילון תרגום יצרנים מעברית לאנגלית
        private readonly Dictionary<string, string> _manufacturerTranslations = new Dictionary<string, string>
        {
            { "דאצ'יה", "Dacia" },
            { "דאציה", "Dacia" },
            { "דאצ'יה רומניה", "Dacia" },
            { "קיה קוריאנה", "Kia" },
            { "יונדאי קוריאנה", "Hyundai" },
            { "סקודה צ'כיה", "Skoda" },
            { "אופל פולין", "Opel" },
            { "איווייס סין", "Aiways" },
            { "אלפא רומיאו", "Alfa Romeo" },
            { "אפטרה", "Aptera" },
            { "אריאל", "Ariel" },
            { "אסטון מרטין", "Aston Martin" },
            { "אאודי", "Audi" },
            { "ב.א.ק", "BAC" },
            
            { "בנטלי", "Bentley" },
            { "ב.מ.וו", "BMW" },
           
            { "בוגאטי", "Bugatti" },
            { "ביואיק", "Buick" },
            { "בי ווי די", "BYD" },
            { "קאדילק", "Cadillac" },
           
            
            { "צ'רי", "Chery" },
            { "שברולט", "Chevrolet" },
            { "קרייזלר", "Chrysler" },
            { "סיטרואן", "Citroen" },
            { "דייהטסו", "Daihatsu" },
            { "דייהו", "Daewoo" },
           
            { "'דודג'", "Dodge" },
            
            { "פרארי", "Ferrari" },
            { "פיאט", "Fiat" },
            
            { "פורד", "Ford" },
            { "גילי", "Geely" },
            { "ג'נסיס", "Genesis" },
            
            { "ג'י.אמ.סי", "GMC" },
            { "גרייט וול", "Great Wall" },
            { "האוואל", "Haval" },
            { "הונדה", "Honda" },
            { "האמר", "Hummer" },
            { "יונדאי", "Hyundai" },
            { "אינאוס", "INEOS" },
            { "איסוזו", "Isuzu" },
            { "יגואר", "Jaguar" },
            { "ג'יפ", "Jeep" },
            { "קיה", "Kia" },
            { "קוניגסג", "Koenigsegg" },
            { "לאדה", "Lada" },
            { "למבורגיני", "Lamborghini" },
            { "לנדרובר", "Land Rover" },
            { "לנצ'יה", "Lancia" },
            { "לקסוס", "Lexus" },
            { "לי אוטו", "Li Auto" },
            { "לינקולן", "Lincoln" },
            { "לוסיד", "Lucid" },
            { "לינק אנד קו", "Lynk & Co" },
           
            { "מרוטי סוזוקי", "Maruti Suzuki" },
            { "סוזוקי מרוטי", "Maruti Suzuki" },
            { "מזארטי", "Maserati" },
            { "מאזדה", "Mazda" },
             { "מזדה", "Mazda" },
            { "מרצדס", "Mercedes-Benz" },
            { "מ.ג", "MG" },
            { "מיני", "Mini" },
            { "מיצובישי", "Mitsubishi" },
            { "ניאו", "Nio" },
            { "ניסאן", "Nissan" },
            { "אופל", "Opel" },
            { "פאגאני", "Pagani" },
            { "פיג'ו", "Peugeot" },
           
           
            { "פולסטאר", "Polestar" },
            { "פונטיאק", "Pontiac" },
            { "פורשה", "Porsche" },
            { "פרוטון", "Proton" },
            { "ראם", "RAM" },
            
            { "רנו", "Renault" },
            { "רימאק", "Rimac" },
            { "ריוויאן", "Rivian" },
            { "רווי", "Roewe" },
            { "רולס-רויס", "Rolls-Royce" },
            { "סאאב", "Saab" },
            { "סטורן", "Saturn" },
           
            { "סיאט", "SEAT" },
            { "סקודה", "Skoda" },
            { "סמארט", "Smart" },
            { "ספייקר", "Spyker" },
            { "סאנגיונג", "SsangYong" },
            { "סובארו", "Subaru" },
            { "סוזוקי", "Suzuki" },
            { "טאטא", "Tata" },
            { "טסלה", "Tesla" },
            { "טויוטה", "Toyota" },
            { "וינפאסט", "VinFast" },
            { "פולקסווגן", "Volkswagen" },
            { "וולוו", "Volvo" },
             { "וולבו", "Volvo" },
            { "וולינג", "Wuling" },
            { "אקספנג", "XPeng" }
        };

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
                var data = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

                if (data != null)
                {
                    if (data.ContainsKey("manufacturer"))
                        data["manufacturer"] = NormalizeManufacturer(data["manufacturer"]?.ToString());

                    if (data.ContainsKey("model"))
                        data["model"] = NormalizeModel(data["model"]?.ToString());
                }

                return data;
            }

            return null;
        }

        private string NormalizeManufacturer(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input ?? "";

            input = input.Trim()
                         .Replace("׳", "'")
                         .Replace("`", "'")
                         .Replace("’", "'");

            if (_manufacturerTranslations.TryGetValue(input, out var exactMatch))
                return exactMatch;

            var firstWord = input.Split(' ')[0];
            if (_manufacturerTranslations.TryGetValue(firstWord, out var partialMatch))
                return partialMatch;

            return input;
        }

     public string NormalizeModel(string? input)
{
    if (string.IsNullOrWhiteSpace(input))
        return input ?? "";

    input = input.Trim().ToLower();

    // הסר שם יצרן בעברית מההתחלה אם קיים
    foreach (var key in _manufacturerTranslations.Keys)
    {
        var manufacturerHeb = key.Trim().ToLower();
        if (input.StartsWith(manufacturerHeb))
        {
            input = input.Substring(manufacturerHeb.Length).Trim();
            break;
        }
    }

    // החזר את כל השם עם אות ראשונה גדולה בכל מילה
    var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    for (int i = 0; i < words.Length; i++)
    {
        if (!string.IsNullOrWhiteSpace(words[i]) && char.IsLetter(words[i][0]))
        {
            words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1);
        }
    }

    return string.Join(" ", words);
}




    }
}
