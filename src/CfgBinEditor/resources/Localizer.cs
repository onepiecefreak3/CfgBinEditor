using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using CrossCutting.Core.Contract.Settings;
using ImGui.Forms.Localization;

namespace CfgBinEditor.resources;

internal class Localizer : BaseLocalizer
{
    private const string NameValue_ = "Name";

    private readonly CfgBinEditorConfiguration _config;
    private readonly ISettingsProvider _settingsProvider;

    protected override string UndefinedValue => "<undefined>";
    protected override string DefaultLocale => _config.DefaultLocale;

    public Localizer(CfgBinEditorConfiguration config, ISettingsProvider settingsProvider)
    {
        _config = config;
        _settingsProvider = settingsProvider;

        Initialize();
    }

    protected override IList<LanguageInfo> InitializeLocalizations()
    {
        string? applicationDirectory = Path.GetDirectoryName(Environment.ProcessPath);
        if (string.IsNullOrEmpty(applicationDirectory))
            return Array.Empty<LanguageInfo>();

        string localeDirectory = Path.Combine(applicationDirectory, _config.LocalizationPath);
        if (!Directory.Exists(localeDirectory))
            return Array.Empty<LanguageInfo>();

        string[] localeFiles = Directory.GetFiles(localeDirectory);

        var result = new List<LanguageInfo>();
        foreach (string localeFile in localeFiles)
        {
            string json = File.ReadAllText(localeFile);

            var entries = JsonSerializer.Deserialize(json, LocalizationJsonContext.Default.DictionaryStringString);
            if (entries == null || !entries.TryGetValue(NameValue_, out string? name))
                continue;

            string locale = Path.GetFileNameWithoutExtension(localeFile);
            result.Add(new LanguageInfo(locale, name, entries));
        }

        return result;
    }

    protected override string InitializeLocale()
    {
        return _settingsProvider.Get("CfgBinEditor.Settings.Locale", string.Empty);
    }

    protected override void SetCurrentLocale(string locale)
    {
        base.SetCurrentLocale(locale);

        _settingsProvider.Set("CfgBinEditor.Settings.Locale", locale);
    }
}

[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true, ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true)]
internal partial class LocalizationJsonContext : JsonSerializerContext;
