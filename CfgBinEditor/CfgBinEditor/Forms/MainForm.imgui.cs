using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using CfgBinEditor.resources;
using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Controls.Menu;
using ImGui.Forms.Localization;
using ImGui.Forms.Models.IO;
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

        private ImageButton _saveBtn;
        private ImageButton _saveAllBtn;

        private IDictionary<string, MenuBarCheckBox> _localeItems;

        private void InitializeComponent(ILocalizer localizer)
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

            _mainMenuBar.Items.Add(_fileMenuItem);
            _mainMenuBar.Items.Add(_settingsMenuItem);
            _mainMenuBar.Items.Add(_infoMenuButton);

            _operationBarLayout.Items.Add(_saveBtn);
            _operationBarLayout.Items.Add(_saveAllBtn);

            _contentLayout.Items.Add(_operationBarLayout);
            _contentLayout.Items.Add(_tabControl);
            _contentLayout.Items.Add(_statusLabel);

            InitializeLanguages(localizer);

            Size = new Vector2(1100, 700);
            Title = LocalizationResources.ApplicationTitle;
            Icon = ImageResources.Icon;

            Content = _contentLayout;
            MainMenuBar = _mainMenuBar;
        }

        private void InitializeLanguages(ILocalizer localizer)
        {
            _localeItems = new Dictionary<string, MenuBarCheckBox>();

            foreach (string locale in localizer.GetLocales())
            {
                var localeItem = new MenuBarCheckBox { Text = localizer.GetLanguageName(locale), Checked = localizer.CurrentLocale == locale };

                _settingsLanguageMenuItem.CheckItems.Add(localeItem);
                _localeItems[locale] = localeItem;
            }
        }
    }
}
