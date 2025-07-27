using System.Text.Json;
using cxp.Models;

namespace cxp.Services;

public static class ConfigurationService
{
    private static AppSettings? _settings;
    private static readonly object _lock = new object();

    public static AppSettings Settings
    {
        get
        {
            if (_settings == null)
            {
                lock (_lock)
                {
                    if (_settings == null)
                    {
                        LoadSettings();
                    }
                }
            }
            return _settings!;
        }
    }

    private static void LoadSettings()
    {
        try
        {
            var settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            if (File.Exists(settingsPath))
            {
                var json = File.ReadAllText(settingsPath);
                _settings = JsonSerializer.Deserialize(json, JsonContext.Default.AppSettings);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to load appsettings.json: {ex.Message}");
        }

        _settings ??= new AppSettings();
    }
}