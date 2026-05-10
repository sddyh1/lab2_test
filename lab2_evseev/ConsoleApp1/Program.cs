using Serilog;
namespace TriangleLab
{
    public class TriangleResult
    {
        public string Type { get; set; } = "";
        public List<(int X, int Y)> Coordinates { get; set; } = new List<(int, int)>();
    }

    public class TriangleCalculator
    {
        public TriangleResult Calculate(string sideA, string sideB, string sideC)
        {
            var result = new TriangleResult();

            if (!float.TryParse(sideA, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out float a) ||
                !float.TryParse(sideB, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out float b) ||
                !float.TryParse(sideC, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out float c))
            {
                result.Type = "";
                result.Coordinates = new List<(int, int)> { (-2, -2), (-2, -2), (-2, -2) };
                Log.Warning("Нечисловые входные данные: {A}, {B}, {C}", sideA, sideB, sideC);
                return result;
            }

            if (a <= 0 || b <= 0 || c <= 0)
            {
                result.Type = "не треугольник";
                result.Coordinates = new List<(int, int)> { (-1, -1), (-1, -1), (-1, -1) };
                Log.Warning("Стороны должны быть положительными: {A}, {B}, {C}", a, b, c);
                return result;
            }

            if (a + b <= c || a + c <= b || b + c <= a)
            {
                result.Type = "не треугольник";
                result.Coordinates = new List<(int, int)> { (-1, -1), (-1, -1), (-1, -1) };
                Log.Warning("Неравенство треугольника не выполняется: {A}, {B}, {C}", a, b, c);
                return result;
            }

            const float eps = 1e-5f;
            if (Math.Abs(a - b) < eps && Math.Abs(b - c) < eps)
                result.Type = "равносторонний";
            else if (Math.Abs(a - b) < eps || Math.Abs(a - c) < eps || Math.Abs(b - c) < eps)
                result.Type = "равнобедренный";
            else
                result.Type = "разносторонний";

            var coords = CalculateCoordinates(a, b, c);
            result.Coordinates = coords;

            return result;
        }

        private List<(int X, int Y)> CalculateCoordinates(float a, float b, float c)
        {
            float xA = 0, yA = 0;
            float xB = c, yB = 0;

            float xC = (b * b + c * c - a * a) / (2 * c);
            float yC = (float)Math.Sqrt(b * b - xC * xC);

            float minX = Math.Min(xA, Math.Min(xB, xC));
            float maxX = Math.Max(xA, Math.Max(xB, xC));
            float minY = Math.Min(yA, Math.Min(yB, yC));
            float maxY = Math.Max(yA, Math.Max(yB, yC));

            float scaleX = (maxX - minX) > 0 ? 100 / (maxX - minX) : 1;
            float scaleY = (maxY - minY) > 0 ? 100 / (maxY - minY) : 1;
            float scale = Math.Min(scaleX, scaleY);

            float offsetX = (100 - scale * (maxX - minX)) / 2 - minX * scale;
            float offsetY = (100 - scale * (maxY - minY)) / 2 - minY * scale;

            int ixA = (int)(xA * scale + offsetX);
            int iyA = (int)(yA * scale + offsetY);
            int ixB = (int)(xB * scale + offsetX);
            int iyB = (int)(yB * scale + offsetY);
            int ixC = (int)(xC * scale + offsetX);
            int iyC = (int)(yC * scale + offsetY);

            return new List<(int, int)> { (ixA, iyA), (ixB, iyB), (ixC, iyC) };
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string template = "{Timestamp:HH:mm:ss} | [{Level:u3}] | {Message:lj}{NewLine}{Exception}";
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(outputTemplate: template)
                .WriteTo.File("logs/log_.txt", outputTemplate: template, rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Log.Information("Приложение запущено");

            var calculator = new TriangleCalculator();

            var testCases = new (string a, string b, string c)[]
            {
                ("5", "5", "5"),
                ("3", "4", "5"),
                ("2", "2", "3"),
                ("1", "2", "5"),
                ("abc", "4", "5"),
                ("0", "4", "5"),
                ("-3", "4", "5")
            };

            foreach (var (a, b, c) in testCases)
            {
                Log.Information("Запрос: стороны {A}, {B}, {C}", a, b, c);
                var result = calculator.Calculate(a, b, c);
                if (result.Type != "")
                {
                    Log.Information("Успешный запрос: тип = {Type}, координаты = {@Coords}",
                        result.Type, result.Coordinates);
                }
                else
                {
                    Log.Error("Неуспешный запрос: нечисловые данные");
                }
            }

            Log.CloseAndFlush();
        }
    }
}