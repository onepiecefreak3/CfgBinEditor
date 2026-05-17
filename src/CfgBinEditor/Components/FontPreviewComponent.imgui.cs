using CfgBinEditor.resources;
using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Base;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Controls.Text.Editor;
using ImGui.Forms.Models;
using ImGui.Forms.Support;
using Konnect.Contract.Management.Plugin;
using Logic.Foundation.PreviewManagement.Abstract;
using System.Numerics;

namespace CfgBinEditor.Components
{
    internal partial class FontPreviewComponent : Component
    {
        private StackLayout _mainLayout;

        private ComboBox<IPreviewPlugin?> _previewBox;
        private ImageButton _exportBtn;

        private TextEditor _previewTextEditor;
        private ZoomablePictureBox _textPreview;

        public Size Size { get; set; } = Size.Parent;

        public override Size GetSize() => Size;

        protected override void UpdateInternal(Rectangle contentRect)
        {
            _mainLayout.Update(contentRect);
        }

        private void InitializeComponent(IPluginManager pluginManager)
        {
            _previewTextEditor = new TextEditor();
            _textPreview = new ZoomablePictureBox { ShowBorder = true };

            _previewBox = new ComboBox<IPreviewPlugin?>();
            _exportBtn = new ImageButton
            {
                Image = ImageResources.ImageExport,
                Tooltip = LocalizationResources.TextPreviewExport,
                ImageSize = new Vector2(16, 16),
                Padding = new Vector2(5, 5),
                Enabled = false
            };

            _mainLayout = new StackLayout { Alignment = Alignment.Vertical, ItemSpacing = 5, Size = Size.Parent };
            var textPreviewSettingsLayout = new StackLayout { Alignment = Alignment.Horizontal, ItemSpacing = 5, Size = Size.WidthAlign };
            var textPreviewLayout = new StackLayout { Alignment = Alignment.Horizontal, ItemSpacing = 5, Size = Size.HeightAlign };

            textPreviewSettingsLayout.Items.Add(_previewBox);
            textPreviewSettingsLayout.Items.Add(new StackItem(_exportBtn) { Size = Size.WidthAlign, HorizontalAlignment = HorizontalAlignment.Right });

            textPreviewLayout.Items.Add(_previewTextEditor);
            textPreviewLayout.Items.Add(_textPreview);

            _mainLayout.Items.Add(textPreviewSettingsLayout);
            _mainLayout.Items.Add(textPreviewLayout);

            InitializePreviewPlugins(pluginManager);
        }

        private void InitializePreviewPlugins(IPluginManager pluginManager)
        {
            IPreviewPlugin[] gamePlugins = [.. pluginManager.GetPlugins<IPreviewPlugin>()];

            _previewBox.Items.Add(new DropDownItem<IPreviewPlugin?>(null, LocalizationResources.TextPreviewDefault));

            foreach (IPreviewPlugin gamePlugin in gamePlugins)
                _previewBox.Items.Add(new DropDownItem<IPreviewPlugin?>(gamePlugin, gamePlugin.Metadata.Name));

            if (_previewBox.Items.Count > 0)
                _previewBox.SelectedItem = _previewBox.Items[0];
        }
    }
}
