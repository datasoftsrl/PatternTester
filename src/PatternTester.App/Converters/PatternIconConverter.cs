using System.Collections.Concurrent;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace PatternTester.App.Converters;

public sealed class PatternIconConverter : IValueConverter
{
    private static readonly ConcurrentDictionary<string, Bitmap?> Cache = new();

    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        if (value is not string iconName || string.IsNullOrWhiteSpace(iconName))
            return null;

        var path = Path.Combine(
            AppContext.BaseDirectory,
            "Assets",
            "Patterns",
            iconName);

        return Cache.GetOrAdd(path, LoadBitmap);
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    private static Bitmap? LoadBitmap(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                Console.Error.WriteLine(
                    $"Pattern icon not found: {path}");

                return null;
            }

            return new Bitmap(path);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(
                $"Unable to load pattern icon '{path}': {ex.Message}");

            return null;
        }
    }
}

