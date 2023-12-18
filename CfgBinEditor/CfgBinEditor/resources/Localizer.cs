using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CrossCutting.Core.Contract.Serialization;
using CrossCutting.Core.Contract.Settings;
using ImGui.Forms.Localization;

namespace CfgBinEditor.resources
{
    internal class Localizer : ILocalizer
    {
        private const string LocalizationFolder_ = "resources/langs";
        private const string DefaultLocale_ = "en";
        private const string NameValue_ = "Name";

        private const string Undefined_ = "<undefined>";

        private readonly ISerializer _serializer;
        private readonly ISettingsProvider _settingsProvider;
        private readonly IDictionary<string, IDictionary<string, string>> _localizations;

        public string CurrentLocale { get; private set; } = DefaultLocale_;

        public Localizer(ISerializer serializer, ISettingsProvider settingsProvider)
        {
            _serializer = serializer;
            _settingsProvider = settingsProvider;

            // Load localizations
            _localizations = GetLocalizations();

            // Set locale
            string locale = GetLocaleSetting();
            if (_localizations.ContainsKey(locale))
                CurrentLocale = locale;
            else if (!_localizations.ContainsKey(DefaultLocale_))
                CurrentLocale = _localizations.FirstOrDefault().Key;
        }

        public IList<string> GetLocales()
        {
            return _localizations.Keys.ToArray();
        }

        public string GetLanguageName(string locale)
        {
            if (!_localizations.ContainsKey(locale) || !_localizations[locale].ContainsKey(NameValue_))
                return Undefined_;

            return _localizations[locale][NameValue_];
        }

        public string GetLocaleByName(string name)
        {
            foreach (var locale in GetLocales())
                if (GetLanguageName(locale) == name)
                    return locale;

            return Undefined_;
        }

        public void ChangeLocale(string locale)
        {
            // Do nothing, if locale was not found
            if (!_localizations.ContainsKey(locale))
                return;

            CurrentLocale = locale;

            SetLocaleSetting(locale);
        }

        public string Localize(string name, params object[] args)
        {
            // Return localization of current locale
            if (!string.IsNullOrEmpty(CurrentLocale) && _localizations.ContainsKey(CurrentLocale) && _localizations[CurrentLocale].ContainsKey(name))
                return string.Format(_localizations[CurrentLocale][name], args);

            // Otherwise, return localization of default locale
            if (!string.IsNullOrEmpty(DefaultLocale_) && _localizations.ContainsKey(DefaultLocale_) && _localizations[DefaultLocale_].ContainsKey(name))
                return string.Format(_localizations[DefaultLocale_][name], args);

            // Otherwise, return localization placeholder
            return Undefined_;
        }

        private IDictionary<string, IDictionary<string, string>> GetLocalizations()
        {
            var result = new Dictionary<string, IDictionary<string, string>>();

            string applicationDirectory = Path.GetDirectoryName(Environment.ProcessPath);
            string localeDirectory = Path.Combine(applicationDirectory, LocalizationFolder_);
            if (!Directory.Exists(localeDirectory))
                return result;

            string[] localeFiles = Directory.GetFiles(localeDirectory);

            foreach (string localeFile in localeFiles)
            {
                // Read text from stream
                string json = File.ReadAllText(localeFile);

                // Deserialize JSON
                result.Add(GetLocale(localeFile), _serializer.Deserialize<IDictionary<string, string>>(json));
            }

            return result;
        }

        private string GetLocale(string localeFile)
        {
            return Path.GetFileNameWithoutExtension(localeFile);
        }

        private string GetLocaleSetting()
        {
            return _settingsProvider.Get("CfgBinEditor.Settings.Locale", string.Empty);
        }

        private void SetLocaleSetting(string locale)
        {
            _settingsProvider.Set("CfgBinEditor.Settings.Locale", locale);
        }
    }
}
