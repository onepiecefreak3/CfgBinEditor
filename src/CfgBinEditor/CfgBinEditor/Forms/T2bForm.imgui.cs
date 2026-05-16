using CfgBinEditor.InternalContract;
using CfgBinEditor.InternalContract.DataClasses;
using CfgBinEditor.resources;
using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Controls.Text.Editor;
using ImGui.Forms.Localization;
using ImGui.Forms.Models;
using ImGui.Forms.Support;
using Konnect.Contract.Management.Plugin;
using Konnect.Contract.Plugin.Game;
using Logic.Business.CfgBinEditorManagement.Contract;
using System.Numerics;
using Logic.Foundation.PreviewManagement.Abstract;
using Size = ImGui.Forms.Models.Size;

namespace CfgBinEditor.Forms
{
    public partial class T2bForm
    {
        private StackLayout _contentLayout;

        private T2bTreeViewForm _treeViewForm;
        private StackLayout _gameLayout;
        private StackLayout _valuesLayout;

        private ComboBox<LocalizedString> _gameComboBox;
        private Button _gameAddButton;

        private Button _valueAddButton;

        private Panel _configContent;

        private ComboBox<IPreviewPlugin?> _previewBox;
        private ImageButton _exportBtn;
        private ImageButton _settingsBtn;

        private TextEditor _previewTextEditor;
        private ZoomablePictureBox _textPreview;

        private void InitializeComponent(T2bFile file, IPluginManager pluginManager, IFormFactory formFactory, IValueSettingsProvider settingsProvider)
        {
            _contentLayout = new StackLayout { Alignment = Alignment.Horizontal, ItemSpacing = 5 };

            _gameLayout = new StackLayout { Alignment = Alignment.Horizontal, ItemSpacing = 5, Size = Size.WidthAlign };
            _valuesLayout = new StackLayout { Alignment = Alignment.Vertical, ItemSpacing = 5 };

            var textPreviewSettingsLayout = new StackLayout { Alignment = Alignment.Horizontal, ItemSpacing = 5, Size = Size.WidthAlign };
            var textPreviewLayout = new StackLayout { Alignment = Alignment.Horizontal, ItemSpacing = 5, Size = new Size(SizeValue.Parent, SizeValue.Relative(.5f)) };

            _treeViewForm = formFactory.CreateT2bTreeViewForm(file.Data);

            _gameComboBox = new ComboBox<LocalizedString>();
            _gameAddButton = new Button { Text = LocalizationResources.GameAddButtonCaption };

            _valueAddButton = new Button { Text = LocalizationResources.CfgBinEntryAddValueCaption, Padding = new Vector2(15, 2), Enabled = false };

            _configContent = new Panel{Size = new Size(SizeValue.Parent, SizeValue.Relative(.5f)) };

            _previewTextEditor = new TextEditor();
            _textPreview = new ZoomablePictureBox { ShowBorder = true };

            _previewBox = new ComboBox<IPreviewPlugin?>();
            _exportBtn = new ImageButton { Image = ImageResources.ImageExport, Tooltip = LocalizationResources.FontPreviewExport, ImageSize = new Vector2(16, 16), Padding = new Vector2(5, 5) };
            _settingsBtn = new ImageButton { Image = ImageResources.Settings, Tooltip = LocalizationResources.FontPreviewSettings, ImageSize = new Vector2(16, 16), Padding = new Vector2(5, 5) };

            _gameLayout.Items.Add(_gameComboBox);
            _gameLayout.Items.Add(_gameAddButton);
            _gameLayout.Items.Add(new StackItem(_valueAddButton) { HorizontalAlignment = HorizontalAlignment.Right, Size = Size.WidthAlign });

            _valuesLayout.Items.Add(_gameLayout);
            _valuesLayout.Items.Add(_configContent);
            _valuesLayout.Items.Add(textPreviewSettingsLayout);
            _valuesLayout.Items.Add(textPreviewLayout);

            _contentLayout.Items.Add(new StackItem(_treeViewForm) { Size = new Size(SizeValue.Relative(.4f), SizeValue.Parent) });
            _contentLayout.Items.Add(_valuesLayout);

            textPreviewSettingsLayout.Items.Add(_previewBox);
            textPreviewSettingsLayout.Items.Add(new StackItem(_exportBtn) { Size = Size.WidthAlign, HorizontalAlignment = HorizontalAlignment.Right });
            textPreviewSettingsLayout.Items.Add(_settingsBtn);

            textPreviewLayout.Items.Add(_previewTextEditor);
            textPreviewLayout.Items.Add(_textPreview);

            InitializeGames(settingsProvider);
            InitializePreviewPlugins(pluginManager);
        }

        public override Size GetSize()
        {
            return Size.Parent;
        }

        protected override void UpdateInternal(Rectangle contentRect)
        {
            _contentLayout.Update(contentRect);
        }

        private void InitializeGames(IValueSettingsProvider settingsProvider)
        {
            _gameComboBox.Items.Add(LocalizationResources.GameNoneCaption);
            _gameComboBox.SelectedItem = _gameComboBox.Items[0];

            foreach (string game in settingsProvider.GetGames())
                _gameComboBox.Items.Add(new DropDownItem<LocalizedString>(game));
        }

        private void InitializePreviewPlugins(IPluginManager pluginManager)
        {
            IPreviewPlugin[] gamePlugins = [.. pluginManager.GetPlugins<IPreviewPlugin>()];

            _previewBox.Items.Add(new DropDownItem<IPreviewPlugin?>(null, LocalizationResources.TextPreviewDefault));

            foreach (IPreviewPlugin gamePlugin in gamePlugins)
                _previewBox.Items.Add(new DropDownItem<IPreviewPlugin?>(gamePlugin, gamePlugin.Metadata.Name));

            if (_previewBox.Items.Count > 0)
            {
                _previewBox.SelectedItem = _previewBox.Items[0];
                _selectedPreviewPlugin = _previewBox.SelectedItem.Content;
            }
        }
    }
}
