using System.Collections.ObjectModel;
using System.Text.Json;
using Avalonia;
using PatternTester.App.ViewModels;

namespace PatternTester.App.Services;

public sealed class LocalizationService
{
    private const string FallbackCode = "it";
    private readonly string _languagesDirectory;
    private readonly Dictionary<string, LanguageFile> _languages = new(StringComparer.OrdinalIgnoreCase);

    public LocalizationService()
    {
        _languagesDirectory = Path.Combine(AppContext.BaseDirectory, "Languages");
        LoadAvailableLanguages();
    }

    public IReadOnlyList<LanguageOptionItem> AvailableLanguages =>
        _languages.Values
            .OrderBy(x => x.Code.Equals(FallbackCode, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
            .ThenBy(x => x.DisplayName)
            .Select(x => new LanguageOptionItem(x.Code, x.DisplayName))
            .ToList();

    public string CurrentLanguage { get; private set; } = FallbackCode;

    public string Translate(string key)
    {
        if (Application.Current?.Resources.TryGetResource(key, null, out var value) == true && value is not null)
            return value.ToString() ?? key;
        return key;
    }

    public void Apply(string? languageCode)
    {
        var code = languageCode;
        if (string.IsNullOrWhiteSpace(code) || !_languages.ContainsKey(code))
            code = _languages.ContainsKey(FallbackCode) ? FallbackCode : _languages.Keys.FirstOrDefault() ?? FallbackCode;

        var fallback = _languages.TryGetValue(FallbackCode, out var it) ? it : null;
        var selected = _languages.TryGetValue(code!, out var lang) ? lang : fallback;

        if (selected is null)
            return;

        CurrentLanguage = selected.Code;

        if (Application.Current is null)
            return;

        foreach (var pair in selected.Values)
            Application.Current.Resources[pair.Key] = pair.Value;

        if (fallback is not null)
        {
            foreach (var pair in fallback.Values)
            {
                if (!selected.Values.ContainsKey(pair.Key))
                    Application.Current.Resources[pair.Key] = pair.Value;
            }
        }
    }

    public string GetGuidePath()
    {
        var language = _languages.TryGetValue(CurrentLanguage, out var selected) ? selected : null;
        var fileName = language?.HelpFile;

        if (string.IsNullOrWhiteSpace(fileName))
            fileName = $"PatternTester_Guida_{CurrentLanguage}.html";

        var path = Path.Combine(_languagesDirectory, fileName);
        if (File.Exists(path))
            return path;

        var fallback = Path.Combine(_languagesDirectory, "PatternTester_Guida_it.html");
        return File.Exists(fallback) ? fallback : path;
    }

    private void LoadAvailableLanguages()
    {
        _languages.Clear();

        if (!Directory.Exists(_languagesDirectory))
            return;

        foreach (var file in Directory.EnumerateFiles(_languagesDirectory, "*.json"))
        {
            try
            {
                var json = File.ReadAllText(file);
                var data = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (data is null)
                    continue;

                var code = data.TryGetValue("_code", out var codeValue) && !string.IsNullOrWhiteSpace(codeValue)
                    ? codeValue
                    : Path.GetFileNameWithoutExtension(file);
                var name = data.TryGetValue("_name", out var nameValue) && !string.IsNullOrWhiteSpace(nameValue)
                    ? nameValue
                    : code;
                var help = data.TryGetValue("_help", out var helpValue) ? helpValue : null;

                data.Remove("_code");
                data.Remove("_name");
                data.Remove("_help");

                _languages[code] = new LanguageFile(code, name, help, data);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Language file ignored: {file}: {ex.Message}");
            }
        }
    }

    private sealed record LanguageFile(string Code, string DisplayName, string? HelpFile, Dictionary<string, string> Values);
}
