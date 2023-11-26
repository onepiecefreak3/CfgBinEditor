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
using Logic.Business.CfgBinValueSettingsManagement.Contract;
using Logic.Domain.Level5.Contract;
using Logic.Domain.Level5.Contract.DataClasses;

namespace CfgBinEditor.Forms
{
    public partial class MainForm : Form
    {
        private readonly IEventBroker _events;
        private readonly ILocalizer _localizer;
        private readonly IFormFactory _formFactory;
        private readonly IConfigurationReader _configReader;

        private readonly IDictionary<TabPage, ConfigurationForm> _pageConfigLookup;
        private readonly IDictionary<ConfigurationForm, TabPage> _configPageLookup;

        private readonly IDictionary<string, TabPage> _pathPageLookup;
        private readonly IDictionary<TabPage, string> _pagePathLookup;

        public MainForm(IEventBroker eventBroker, ILocalizer localizer, IFormFactory formFactory, IConfigurationReader configReader, IValueSettingsProvider settingsProvider)
        {
            InitializeComponent(localizer);

            _events = eventBroker;
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

            _tabControl.SelectedPageChanged += (s, e) => UpdateSaveButtons();

            _fileOpenMenuItem.Clicked += (s, e) => OpenFile();

            _saveBtn.Clicked += (s, e) => SaveTabPage(_tabControl.SelectedPage);
            _saveAllBtn.Clicked += (s, e) => SaveAllTabPages();

            Closing += (s, e) => CloseApplication(e);

            AllowDragDrop = true;
            DragDrop += (s, e) => OpenFile(e.File);

            _events.Subscribe<FileChangedMessage>(msg => MarkChangedFile(msg.ConfigForm, true));
            _events.Subscribe<FileSavedMessage>(FileSaved);

            if (settingsProvider.TryGetError(out Exception error))
            {
                error = GetInnermostException(error);
                if (error is not FileNotFoundException)
                    SetStatus(LocalizationResources.CfgBinTagsLoadErrorCaption(error.Message), LabelStatus.Error);
            }
        }

        private async Task CloseApplication(ClosingEventArgs e)
        {
            if (_tabControl.Pages.All(p => !p.HasChanges))
                return;

            DialogResult result = await MessageBox.ShowYesNoAsync(LocalizationResources.ApplicationCloseUnsavedChangesCaption, LocalizationResources.ApplicationCloseUnsavedChangesText);
            if(result == DialogResult.Yes)
                return;

            e.Cancel = true;
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


        private void SaveTabPage(TabPage page)
        {
            if (!_pageConfigLookup.TryGetValue(page, out ConfigurationForm configForm))
                return;

            if (!_pagePathLookup.TryGetValue(page, out string configPath))
                return;

            RaiseFileSaveRequest(new Dictionary<ConfigurationForm, string>
            {
                [configForm] = configPath
            });
        }

        private void SaveAllTabPages()
        {
            var configForms = new Dictionary<ConfigurationForm, string>();
            foreach (TabPage tabPage in _tabControl.Pages.Where(p => p.HasChanges))
            {
                if (!_pageConfigLookup.TryGetValue(tabPage, out ConfigurationForm configForm))
                    continue;

                if (!_pagePathLookup.TryGetValue(tabPage, out string configPath))
                    continue;

                configForms[configForm] = configPath;
            }

            RaiseFileSaveRequest(configForms);
        }

        private void FileSaved(FileSavedMessage msg)
        {
            MarkChangedFile(msg.ConfigForm, msg.Error != null);
            SetStatus(string.Empty, LabelStatus.None);

            if (msg.Error == null)
                return;

            if (!_configPageLookup.TryGetValue(msg.ConfigForm, out TabPage configPage))
                return;

            if (!_pagePathLookup.TryGetValue(configPage, out string configPath))
                return;

            SetStatus(LocalizationResources.FileSaveErrorCaption(Path.GetFileName(configPath), GetInnermostException(msg.Error).Message), LabelStatus.Error);
        }

        private void MarkChangedFile(ConfigurationForm configForm, bool hasChanges)
        {
            if (_configPageLookup.TryGetValue(configForm, out TabPage page))
                page.HasChanges = hasChanges;

            UpdateSaveButtons();
        }

        private void UpdateSaveButtons()
        {
            _saveBtn.Enabled = _tabControl.SelectedPage.HasChanges;
            _saveAllBtn.Enabled = _tabControl.Pages.Any(p => p.HasChanges);
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

            if (!TryLoadFile(filePath, out Configuration config))
                return false;

            ConfigurationForm configForm = _formFactory.CreateConfigurationForm(config);
            TabPage page = new(configForm) { Title = Path.GetFileName(filePath) };

            _tabControl.AddPage(page);

            _configPageLookup[configForm] = page;
            _pageConfigLookup[page] = configForm;

            _pathPageLookup[filePath] = page;
            _pagePathLookup[page] = filePath;

            _tabControl.SelectedPage = page;

            SetStatus(string.Empty, LabelStatus.None);

            return true;
        }

        private bool TryLoadFile(string filePath, out Configuration config)
        {
            config = null;

            try
            {
                using Stream fileStream = File.OpenRead(filePath);
                config = _configReader.Read(fileStream);
            }
            catch (Exception e)
            {
                SetStatus(LocalizationResources.FileOpenErrorCaption(Path.GetFileName(filePath), GetInnermostException(e).Message), LabelStatus.Error);
                return false;
            }

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

        private Exception GetInnermostException(Exception e)
        {
            while (e.InnerException != null)
                e = e.InnerException;

            return e;
        }

        private void RaiseFileSaveRequest(IDictionary<ConfigurationForm, string> configs)
        {
            _events.Raise(new FileSaveRequestMessage(configs));
        }
    }
}
