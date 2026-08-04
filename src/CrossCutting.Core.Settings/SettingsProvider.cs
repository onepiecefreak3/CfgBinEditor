using CrossCutting.Core.Contract.Settings;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CrossCutting.Core.Settings;

public class SettingsProvider : ISettingsProvider
{
    private readonly Dictionary<string, string> _values;

    public SettingsProvider()
    {
        _values = CreateOrReadValues();
    }

    public T Get<T>(string name, T defaultValue)
    {
        if (!_values.TryGetValue(name, out string? value))
            return defaultValue;

        if (typeof(T).IsEnum)
            return (T)Enum.Parse(typeof(T), value);

        return (T)Convert.ChangeType(value, typeof(T));
    }

    public void Set<T>(string name, T? value)
    {
        if (value == null)
            return;

        _values[name] = $"{value}";

        Persist();
    }

    private void Persist()
    {
        string settingsPath = GetSettingsPath();
        string settingsJson = JsonSerializer.Serialize(_values, SettingsFileJsonContext.Default.DictionaryStringString);

        File.WriteAllText(settingsPath, settingsJson);
    }

    private Dictionary<string, string> CreateOrReadValues()
    {
        string settingsPath = GetSettingsPath();
        if (!File.Exists(settingsPath))
            return new Dictionary<string, string>();

        string settingsJson = File.ReadAllText(settingsPath);
        var settings = JsonSerializer.Deserialize(settingsJson, SettingsFileJsonContext.Default.DictionaryStringString);
        if (settings is null)
            return new Dictionary<string, string>();

        return settings;
    }

    private string GetSettingsPath()
    {
        return Path.Combine(Path.GetDirectoryName(Environment.ProcessPath)!, "settings.json");
    }
}

[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true, ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true)]
internal partial class SettingsFileJsonContext : JsonSerializerContext;