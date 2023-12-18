using System;
using System.Collections.Generic;
using System.Numerics;
using CfgBinEditor.resources;
using CrossCutting.Core.Contract.Settings;
using ImGui.Forms;
using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Controls.Menu;
using ImGui.Forms.Localization;
using ImGui.Forms.Models;
using ImGui.Forms.Models.IO;
using ImGuiNET;
using Veldrid;

namespace CfgBinEditor.Forms
{
    public partial class MainForm
    {
        private MainMenuBar _mainMenuBar;
        private StackLayout _contentLayout;

        private StackLayout _operationBarLayout;
        private TabControl _tabControl;
        private Label _statusLabel;

        private MenuBarMenu _fileMenuItem;
        private MenuBarMenu _settingsMenuItem;
        private MenuBarButton _infoMenuButton;

        private MenuBarButton _fileOpenMenuItem;
        private MenuBarRadio _settingsLanguageMenuItem;
        private MenuBarRadio _settingsThemeMenuItem;

        private ImageButton _saveBtn;
        private ImageButton _saveAllBtn;

        private IDictionary<MenuBarCheckBox, string> _localeItems;
        private IDictionary<MenuBarCheckBox, Theme> _themeItems;

        private void InitializeComponent(ILocalizer localizer, ISettingsProvider settingsProvider)
        {
            _mainMenuBar = new MainMenuBar();
            _contentLayout = new StackLayout { Alignment = Alignment.Vertical, ItemSpacing = 5 };

            _operationBarLayout = new StackLayout { Size = ImGui.Forms.Models.Size.Content, Alignment = Alignment.Horizontal, ItemSpacing = 3 };

            _tabControl = new TabControl();
            _statusLabel = new Label();

            _fileMenuItem = new MenuBarMenu { Text = LocalizationResources.MenuFileCaption };
            _settingsMenuItem = new MenuBarMenu { Text = LocalizationResources.MenuSettingsCaption };
            _infoMenuButton = new MenuBarButton { Text = LocalizationResources.MenuInfoCaption };

            _fileOpenMenuItem = new MenuBarButton { Text = LocalizationResources.MenuFileOpenCaption, KeyAction = new KeyCommand(ModifierKeys.Control, Key.O) };
            _settingsLanguageMenuItem = new MenuBarRadio { Text = LocalizationResources.MenuSettingsLanguagesCaption };
            _settingsThemeMenuItem = new MenuBarRadio { Text = LocalizationResources.MenuSettingsThemesCaption };

            _saveBtn = new ImageButton
            {
                Image = ImageResources.Save,
                ImageSize = new Vector2(16, 16),
                Padding = new Vector2(3, 3),
                KeyAction = new KeyCommand(ModifierKeys.Control, Key.S),
                Tooltip = LocalizationResources.FileSaveTooltipSingleCaption(() => _tabControl?.SelectedPage?.Title),
                Enabled = false
            };
            _saveAllBtn = new ImageButton
            {
                Image = ImageResources.SaveAll,
                ImageSize = new Vector2(16, 16),
                Padding = new Vector2(3, 3),
                KeyAction = new KeyCommand(ModifierKeys.Control | ModifierKeys.Shift, Key.S),
                Tooltip = LocalizationResources.FileSaveTooltipAllCaption,
                Enabled = false
            };

            _fileMenuItem.Items.Add(_fileOpenMenuItem);
            _settingsMenuItem.Items.Add(_settingsLanguageMenuItem);
            _settingsMenuItem.Items.Add(_settingsThemeMenuItem);

            _mainMenuBar.Items.Add(_fileMenuItem);
            _mainMenuBar.Items.Add(_settingsMenuItem);
            _mainMenuBar.Items.Add(_infoMenuButton);

            _operationBarLayout.Items.Add(_saveBtn);
            _operationBarLayout.Items.Add(_saveAllBtn);

            _contentLayout.Items.Add(_operationBarLayout);
            _contentLayout.Items.Add(_tabControl);
            _contentLayout.Items.Add(_statusLabel);

            _localeItems = new Dictionary<MenuBarCheckBox, string>();
            _themeItems = new Dictionary<MenuBarCheckBox, Theme>();

            InitializeLanguages(localizer);
            InitializeThemes(settingsProvider);

            Size = new Vector2(1100, 700);
            Title = LocalizationResources.ApplicationTitle;
            Icon = ImageResources.Icon;

            Content = _contentLayout;
            MainMenuBar = _mainMenuBar;
        }

        private void InitializeLanguages(ILocalizer localizer)
        {
            foreach (string locale in localizer.GetLocales())
            {
                var localeItem = new MenuBarCheckBox
                {
                    Text = localizer.GetLanguageName(locale),
                    Checked = localizer.CurrentLocale == locale
                };

                _settingsLanguageMenuItem.CheckItems.Add(localeItem);
                _localeItems[localeItem] = locale;
            }
        }

        private void InitializeThemes(ISettingsProvider settingsProvider)
        {
            Theme themeSetting = GetThemeSetting(settingsProvider);
            Style.ChangeTheme(themeSetting);

            foreach (Theme theme in Enum.GetValues(typeof(Theme)))
            {
                var themeItem = new MenuBarCheckBox
                {
                    Text = LocalizationResources.MenuSettingsThemeCaption(theme),
                    Checked = themeSetting == theme
                };

                _settingsThemeMenuItem.CheckItems.Add(themeItem);
                _themeItems[themeItem] = theme;
            }
        }

        private Theme GetThemeSetting(ISettingsProvider settingsProvider)
        {
            return settingsProvider.Get("CfgBinEditor.Settings.Theme", Style.Theme);
        }

        private void SetThemeSetting(Theme theme, ISettingsProvider settingsProvider)
        {
            settingsProvider.Set("CfgBinEditor.Settings.Theme", theme);
        }
    }
}
