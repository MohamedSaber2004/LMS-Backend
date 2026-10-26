using LMS.Application.Common.Interfaces;
using System.Globalization;
using System.Text.Json;

namespace LMS.Application.Localization
{
    public class JsonLocalizationProvider : ILocalizationProvider
    {
        private static readonly Dictionary<string, Dictionary<string, string>> _localizations = new(StringComparer.OrdinalIgnoreCase);

        public JsonLocalizationProvider()
        {
            Initialize();
        }

        public static void Initialize(string? rootPath = null)
        {
            var baseDirectory = rootPath ?? AppContext.BaseDirectory;
            var assemblyLocation = Path.GetDirectoryName(typeof(JsonLocalizationProvider).Assembly.Location);

            var possiblePaths = new List<string>();

            if (!string.IsNullOrEmpty(rootPath))
            {
                possiblePaths.Add(Path.Combine(rootPath, "Localization", "Resources"));
                possiblePaths.Add(Path.Combine(rootPath, "LMS.Application", "Localization", "Resources"));
            }

            possiblePaths.Add(Path.Combine(baseDirectory, "Localization", "Resources"));
            possiblePaths.Add(Path.Combine(assemblyLocation ?? "", "Localization", "Resources"));
            possiblePaths.Add(Path.Combine(Directory.GetCurrentDirectory(), "Localization", "Resources"));

            var resourcePath = possiblePaths.FirstOrDefault(Directory.Exists);

            if (resourcePath == null)
            {
                foreach (var path in possiblePaths) Console.WriteLine($" - {path}");
                return;
            }

            var cultures = new[] { "en", "ar" };

            foreach (var culture in cultures)
            {
                var filePath = Path.Combine(resourcePath, $"messages.{culture}.json");
                if (File.Exists(filePath))
                {
                    try
                    {
                        var json = File.ReadAllText(filePath);
                        using var doc = JsonDocument.Parse(json);
                        var cultureData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                        FlattenJson(doc.RootElement, "", cultureData);

                        _localizations[culture] = cultureData;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading localization file {filePath}: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"Warning: Localization file not found: {filePath}");
                }
            }
        }

        private static void FlattenJson(JsonElement element, string prefix, Dictionary<string, string> result)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var property in element.EnumerateObject())
                    {
                        var name = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";
                        FlattenJson(property.Value, name, result);
                    }
                    break;
                case JsonValueKind.Array:
                    int index = 0;
                    foreach (var item in element.EnumerateArray())
                    {
                        FlattenJson(item, $"{prefix}[{index}]", result);
                        index++;
                    }
                    break;
                case JsonValueKind.String:
                    result[prefix] = element.GetString() ?? "";
                    break;
                case JsonValueKind.Number:
                case JsonValueKind.True:
                case JsonValueKind.False:
                case JsonValueKind.Null:
                    result[prefix] = element.ToString();
                    break;
            }
        }

        public static string GetLocalizedString(string key, string? culture = null)
        {
            culture ??= CultureInfo.CurrentUICulture.Name;

            if (!string.IsNullOrEmpty(culture) && culture.Contains('-'))
            {
                culture = culture.Split('-')[0];
            }
            else if (!string.IsNullOrEmpty(culture) && culture.Length > 2)
            {
                culture = culture.Substring(0, 2);
            }

            if (string.IsNullOrEmpty(culture) || (culture != "en" && culture != "ar"))
            {
                culture = "ar";
            }

            if (_localizations.TryGetValue(culture, out var cultureData) && cultureData.TryGetValue(key, out var value))
            {
                return value;
            }

            if (culture != "en" && _localizations.TryGetValue("en", out var enData) && enData.TryGetValue(key, out var enValue))
            {
                return enValue;
            }

            return key;
        }

        public static string GetLocalizedString(string key, string? culture, params object[] args)
        {
            var baseValue = GetLocalizedString(key, culture);

            try
            {
                return string.Format(baseValue, args);
            }
            catch
            {
                return baseValue;
            }
        }

        string ILocalizationProvider.GetLocalizedString(string key, string? culture)
            => GetLocalizedString(key, culture);

        string ILocalizationProvider.GetLocalizedString(string key, string? culture, params object[] args)
            => GetLocalizedString(key, culture, args);
    }
}
