namespace CrossCutting.Core.Contract.Settings
{
    public interface ISettingsProvider
    {
        T Get<T>(string name, T defaultValue);
        void Set<T>(string name, T? value);
    }
}
