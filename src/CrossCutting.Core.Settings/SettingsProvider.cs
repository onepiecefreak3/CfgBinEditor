using CrossCutting.Core.Contract.Settings;
using CrossCutting.Core.Serialization.JsonAdapter;

namespace CrossCutting.Core.Settings
{
    public class SettingsProvider : ISettingsProvider
    {
        private readonly JsonSerializer _serializer;
        private readonly IDictionary<string, string> _values;

        public SettingsProvider()
        {
            _serializer = new JsonSerializer();
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
            string settingsJson = _serializer.Serialize(_values);

            File.WriteAllText(settingsPath, settingsJson);
        }

        private IDictionary<string, string> CreateOrReadValues()
        {
            string settingsPath = GetSettingsPath();
            if (!File.Exists(settingsPath))
                return new Dictionary<string, string>();

            string settingsJson = File.ReadAllText(settingsPath);
            return _serializer.Deserialize<IDictionary<string, string>>(settingsJson);
        }

        private string GetSettingsPath()
        {
            return Path.Combine(Path.GetDirectoryName(Environment.ProcessPath)!, "settings.json");
        }
    }
}