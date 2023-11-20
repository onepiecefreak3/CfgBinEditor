using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CfgBinEditor.InternalContract;
using CfgBinEditor.InternalContract.DataClasses;
using CfgBinEditor.Messages;
using CfgBinEditor.resources;
using CrossCutting.Core.Contract.EventBrokerage;
using ImGui.Forms;
using ImGui.Forms.Controls;
using ImGui.Forms.Localization;
using ImGui.Forms.Modals;
using ImGui.Forms.Modals.IO;
using ImGuiNET;
using Logic.Domain.Level5.Contract;
using Logic.Domain.Level5.Contract.DataClasses;

namespace CfgBinEditor.Forms
{
    public partial class MainForm : Form
    {
        private readonly ILocalizer _localizer;
        private readonly IFormFactory _formFactory;
        private readonly IConfigurationReader _configReader;

        private readonly IDictionary<TabPage, ConfigurationForm> _pageConfigLookup;
        private readonly IDictionary<ConfigurationForm, TabPage> _configPageLookup;

        private readonly IDictionary<string, TabPage> _pathPageLookup;
        private readonly IDictionary<TabPage, string> _pagePathLookup;

        public MainForm(IEventBroker eventBroker, ILocalizer localizer, IFormFactory formFactory, IConfigurationReader configReader, IValueSettings settings)
        {
            InitializeComponent(localizer);

            _localizer = localizer;
            _formFactory = formFactory;
            _configReader = configReader;

            _pageConfigLookup = new Dictionary<TabPage, ConfigurationForm>();
            _configPageLookup = new Dictionary<ConfigurationForm, TabPage>();

            _pathPageLookup = new Dictionary<string, TabPage>();
            _pagePathLookup = new Dictionary<TabPage, string>();

            _settingsLanguageMenuItem.SelectedItemChanged +=
                (s, e) => ChangeLocale(_settingsLanguageMenuItem.SelectedItem.Text);

            _tabControl.PageRemoving += async (s, e) => e.Cancel = !(await ShouldCloseTabPage(e.Page));
            _tabControl.PageRemoved += (s, e) => CloseTabPage(e.Page);

            _fileOpenMenuItem.Clicked += (s, e) => OpenFile();

            AllowDragDrop = true;
            DragDrop += (s, e) => OpenFile(e.File);

            eventBroker.Subscribe<FileChangedMessage>(msg => MarkChangedFile(msg.ConfigForm));

            if (settings.HasError)
                SetStatus(LocalizationResources.CfgBinTagsLoadErrorCaption(settings.ReadError.Message), LabelStatus.Error);
        }

        private void ChangeLocale(string language)
        {
            _localizer.ChangeLocale(_localizer.GetLocaleByName(language));
        }

        private async Task<bool> ShouldCloseTabPage(TabPage page)
        {
            if (!_pageConfigLookup.ContainsKey(page))
                return true;

            if (!page.HasChanges)
                return true;

            DialogResult result = await MessageBox.ShowYesNoAsync(LocalizationResources.FileCloseUnsavedChangesCaption, LocalizationResources.FileCloseUnsavedChangesText);
            return result == DialogResult.Yes;
        }

        private void CloseTabPage(TabPage page)
        {
            if (_pageConfigLookup.TryGetValue(page, out ConfigurationForm configForm))
                _configPageLookup.Remove(configForm);

            if (_pagePathLookup.TryGetValue(page, out string filePath))
                _pathPageLookup.Remove(filePath);

            _pageConfigLookup.Remove(page);
            _pagePathLookup.Remove(page);
        }

        private void MarkChangedFile(ConfigurationForm configForm)
        {
            if (_configPageLookup.TryGetValue(configForm, out TabPage page))
                page.HasChanges = true;
        }

        private async void OpenFile()
        {
            var ofd = new OpenFileDialog
            {
                Caption = LocalizationResources.FileOpenCaption,
                FileFilters =
                {
                    new FileFilter(LocalizationResources.FileOpenCfgBinFilterCaption, "cfg.bin")
                }
            };

            DialogResult result = await ofd.ShowAsync();
            if (result != DialogResult.Ok)
            {
                SetStatus(LocalizationResources.FileOpenCancel, LabelStatus.Error);
                return;
            }

            bool wasOpened = OpenFile(ofd.SelectedPath);
            if (wasOpened)
                SetStatus(string.Empty, LabelStatus.None);
        }

        private bool OpenFile(string filePath)
        {
            if (_pathPageLookup.TryGetValue(filePath, out TabPage openedPage))
            {
                _tabControl.SelectedPage = openedPage;
                return true;
            }

            using Stream fileStream = File.OpenRead(filePath);
            Configuration config = _configReader.Read(fileStream);

            ConfigurationForm configForm = _formFactory.CreateConfigurationForm(config);
            TabPage page = new(configForm) { Title = Path.GetFileName(filePath) };

            _tabControl.AddPage(page);

            _configPageLookup[configForm] = page;
            _pageConfigLookup[page] = configForm;

            _pathPageLookup[filePath] = page;
            _pagePathLookup[page] = filePath;

            _tabControl.SelectedPage = page;

            return true;
        }

        private void SetStatus(LocalizedString text, LabelStatus status)
        {
            _statusLabel.Text = text;

            switch (status)
            {
                case LabelStatus.None:
                    _statusLabel.TextColor = Style.GetColor(ImGuiCol.Text);
                    break;

                case LabelStatus.Error:
                    _statusLabel.TextColor = ColorResources.TextError;
                    break;
            }
        }
    }
}
