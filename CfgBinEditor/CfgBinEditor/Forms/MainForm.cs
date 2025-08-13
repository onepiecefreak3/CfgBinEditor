using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CfgBinEditor.InternalContract;
using CfgBinEditor.InternalContract.DataClasses;
using CfgBinEditor.Messages;
using CfgBinEditor.resources;
using CrossCutting.Core.Contract.EventBrokerage;
using CrossCutting.Core.Contract.Settings;
using ImGui.Forms;
using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Base;
using ImGui.Forms.Controls.Menu;
using ImGui.Forms.Localization;
using ImGui.Forms.Modals;
using ImGui.Forms.Modals.IO;
using ImGui.Forms.Modals.IO.Windows;
using ImGui.Forms.Models;
using Logic.Business.CfgBinEditorManagement.Contract;
using Logic.Domain.Level5Management.Contract;
using Logic.Domain.Level5Management.Contract.DataClasses;
using Veldrid.Sdl2;

namespace CfgBinEditor.Forms
{
    public partial class MainForm : Form
    {
        private readonly IEventBroker _events;
        private readonly ILocalizer _localizer;
        private readonly IFormFactory _formFactory;
        private readonly IT2bReader _t2bReader;
        private readonly IRdbnReader _rdbnReader;
        private readonly ISettingsProvider _settingsProvider;

        private readonly IDictionary<TabPage, Component> _pageConfigLookup;
        private readonly IDictionary<Component, TabPage> _configPageLookup;

        private readonly IDictionary<string, TabPage> _pathPageLookup;
        private readonly IDictionary<TabPage, string> _pagePathLookup;

        public MainForm(IEventBroker eventBroker, ILocalizer localizer, IFormFactory formFactory, ISettingsProvider settingsProvider, IT2bReader t2BReader, IRdbnReader rdbnReader, IValueSettingsProvider valueSettingsProvider, IEntryNamesProvider entryNamesProvider)
        {
            InitializeComponent(localizer, settingsProvider);

            _events = eventBroker;
            _localizer = localizer;
            _formFactory = formFactory;
            _t2bReader = t2BReader;
            _rdbnReader = rdbnReader;
            _settingsProvider = settingsProvider;

            _pageConfigLookup = new Dictionary<TabPage, Component>();
            _configPageLookup = new Dictionary<Component, TabPage>();

            _pathPageLookup = new Dictionary<string, TabPage>();
            _pagePathLookup = new Dictionary<TabPage, string>();

            _settingsLanguageMenuItem.SelectedItemChanged +=
                (s, e) => ChangeLocale(_settingsLanguageMenuItem.SelectedItem);
            _settingsThemeMenuItem.SelectedItemChanged +=
                (s, e) => ChangeTheme(_settingsThemeMenuItem.SelectedItem);

            _tabControl.PageRemoving += async (s, e) => e.Cancel = !(await ShouldCloseTabPage(e.Page));
            _tabControl.PageRemoved += (s, e) => CloseTabPage(e.Page);

            _tabControl.SelectedPageChanged += (s, e) => UpdateSaveButtons();

            _fileOpenMenuItem.Clicked += (s, e) => OpenFile();

            _saveBtn.Clicked += (s, e) => SaveTabPage(_tabControl.SelectedPage);
            _saveAllBtn.Clicked += (s, e) => SaveAllTabPages();

            Closing += (s, e) => CloseApplication(e);

            AllowDragDrop = true;
            DragDrop += (s, e) => OpenFiles(e);

            _events.Subscribe<FileChangedMessage>(msg => MarkChangedFile(msg.Source, true));
            _events.Subscribe<FileSavedMessage>(FileSaved);
            _events.Subscribe<UpdateStatusMessage>(msg => SetStatus(msg.Text, msg.Status));

            if (valueSettingsProvider.TryGetError(out Exception? error))
            {
                error = GetInnermostException(error!);
                if (error is not FileNotFoundException)
                    SetStatus(LocalizationResources.CfgBinTagsLoadErrorCaption(error.Message), LabelStatus.Error);
            }

            if (entryNamesProvider.TryGetError(out error))
            {
                error = GetInnermostException(error!);
                if (error is not FileNotFoundException)
                    SetStatus(LocalizationResources.CfgBinIdsLoadErrorCaption(error.Message), LabelStatus.Error);
            }
        }

        private async Task CloseApplication(ClosingEventArgs e)
        {
            if (_tabControl.Pages.All(p => !p.HasChanges))
                return;

            DialogResult result = await MessageBox.ShowYesNoAsync(LocalizationResources.ApplicationCloseUnsavedChangesCaption, LocalizationResources.ApplicationCloseUnsavedChangesText);
            if (result == DialogResult.Yes)
                return;

            e.Cancel = true;
        }

        private void ChangeLocale(MenuBarCheckBox checkbox)
        {
            if (!_localeItems.TryGetValue(checkbox, out string locale))
                return;

            _localizer.ChangeLocale(locale);
        }

        private void ChangeTheme(MenuBarCheckBox checkbox)
        {
            if (!_themeItems.TryGetValue(checkbox, out Theme theme))
                return;

            Style.ChangeTheme(theme);

            SetThemeSetting(theme, _settingsProvider);
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
            if (_pageConfigLookup.TryGetValue(page, out Component configForm))
                _configPageLookup.Remove(configForm);

            if (_pagePathLookup.TryGetValue(page, out string filePath))
                _pathPageLookup.Remove(filePath);

            _pageConfigLookup.Remove(page);
            _pagePathLookup.Remove(page);

            UpdateSaveButtons();
        }


        private void SaveTabPage(TabPage page)
        {
            if (!_pageConfigLookup.TryGetValue(page, out Component configForm))
                return;

            if (!_pagePathLookup.TryGetValue(page, out string configPath))
                return;

            RaiseFileSaveRequest(new Dictionary<Component, string>
            {
                [configForm] = configPath
            });
        }

        private void SaveAllTabPages()
        {
            var configForms = new Dictionary<Component, string>();
            foreach (TabPage tabPage in _tabControl.Pages.Where(p => p.HasChanges))
            {
                if (!_pageConfigLookup.TryGetValue(tabPage, out Component configForm))
                    continue;

                if (!_pagePathLookup.TryGetValue(tabPage, out string configPath))
                    continue;

                configForms[configForm] = configPath;
            }

            RaiseFileSaveRequest(configForms);
        }

        private void FileSaved(FileSavedMessage msg)
        {
            MarkChangedFile(msg.Source, msg.Error != null);
            SetStatus(string.Empty, LabelStatus.None);

            if (msg.Error == null)
                return;

            if (!_configPageLookup.TryGetValue(msg.Source, out TabPage configPage))
                return;

            if (!_pagePathLookup.TryGetValue(configPage, out string configPath))
                return;

            SetStatus(LocalizationResources.FileSaveErrorCaption(Path.GetFileName(configPath), GetInnermostException(msg.Error).Message), LabelStatus.Error);
        }

        private void MarkChangedFile(Component sourceForm, bool hasChanges)
        {
            if (_configPageLookup.TryGetValue(sourceForm, out TabPage page))
                page.HasChanges = hasChanges;

            UpdateSaveButtons();
        }

        private void UpdateSaveButtons()
        {
            _saveBtn.Enabled = _tabControl.SelectedPage?.HasChanges ?? false;
            _saveAllBtn.Enabled = _tabControl.Pages.Any(p => p.HasChanges);
        }

        private async void OpenFile()
        {
            var ofd = new WindowsOpenFileDialog
            {
                Title = LocalizationResources.FileOpenCaption,
                InitialDirectory = GetLoadDirectory(_settingsProvider),
                Filters = 
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

            SetLoadDirectory(Path.GetDirectoryName(ofd.Files[0]), _settingsProvider);

            bool wasOpened = OpenFile(ofd.Files[0]);
            if (wasOpened)
                SetStatus(string.Empty, LabelStatus.None);
        }

        private void OpenFiles(DragDropEvent[] events)
        {
            foreach (DragDropEvent e in events)
                OpenFile(e.File);
        }

        private bool OpenFile(string filePath)
        {
            if (_pathPageLookup.TryGetValue(filePath, out TabPage openedPage))
            {
                _tabControl.SelectedPage = openedPage;
                return true;
            }

            Component configForm;
            TabPage page;

            if (TryLoadT2bFile(filePath, out T2b t2b))
            {
                configForm = _formFactory.CreateT2bForm(t2b);
            }
            else if (TryLoadRdbnFile(filePath, out Rdbn rdbn))
            {
                configForm = _formFactory.CreateRdbnForm(rdbn);
            }
            else
                return false;

            page = new(configForm) { Title = Path.GetFileName(filePath) };

            _tabControl.AddPage(page);

            _configPageLookup[configForm] = page;
            _pageConfigLookup[page] = configForm;

            _pathPageLookup[filePath] = page;
            _pagePathLookup[page] = filePath;

            _tabControl.SelectedPage = page;

            SetStatus(string.Empty, LabelStatus.None);

            return true;
        }

        private bool TryLoadT2bFile(string filePath, out T2b config)
        {
            config = null;

            try
            {
                using Stream fileStream = File.OpenRead(filePath);
                config = _t2bReader.Read(fileStream);

                if (config == null)
                    SetStatus(LocalizationResources.FileOpenErrorCaption(Path.GetFileName(filePath), () => LocalizationResources.FileOpenUnsupportedFileType), LabelStatus.Error);
            }
            catch (Exception e)
            {
                SetStatus(LocalizationResources.FileOpenErrorCaption(Path.GetFileName(filePath), () => GetInnermostException(e).Message), LabelStatus.Error);
            }

            return config != null;
        }

        private bool TryLoadRdbnFile(string filePath, out Rdbn config)
        {
            config = null;

            try
            {
                using Stream fileStream = File.OpenRead(filePath);
                config = _rdbnReader.Read(fileStream);

                if (config == null)
                    SetStatus(LocalizationResources.FileOpenErrorCaption(Path.GetFileName(filePath), () => LocalizationResources.FileOpenUnsupportedFileType), LabelStatus.Error);
            }
            catch (Exception e)
            {
                SetStatus(LocalizationResources.FileOpenErrorCaption(Path.GetFileName(filePath), () => GetInnermostException(e).Message), LabelStatus.Error);
            }

            return config != null;
        }

        private void SetStatus(LocalizedString text, LabelStatus status)
        {
            _statusLabel.Text = text;

            switch (status)
            {
                case LabelStatus.None:
                    _statusLabel.TextColor = ColorResources.TextDefault;
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

        private void RaiseFileSaveRequest(IDictionary<Component, string> configs)
        {
            _events.Raise(new FileSaveRequestMessage(configs));
        }
    }
}
