using CfgBinEditor.resources;
using ImGui.Forms.Modals;
using ImGui.Forms.Modals.IO.Windows;
using ImGui.Forms.Resources;
using Kaligraphy.Contract.DataClasses.Parsing;
using Kaligraphy.Parsing;
using Konnect.Contract.Management.Plugin;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CfgBinEditor.Components
{
    internal partial class FontPreviewComponent
    {
        private IList<CharacterData>? _deserializedText;
        private Image<Rgba32>? _preview;

        public FontPreviewComponent(IPluginManager pluginManager)
        {
            InitializeComponent(pluginManager);

            _previewTextEditor.TextChanged += _previewTextEditor_TextChanged;
            _previewBox.SelectedItemChanged += _previewBox_SelectedItemChanged;

            _exportBtn.Clicked += _exportBtn_Clicked;

            _previewTextEditor.SetText(LocalizationResources.TextPreviewPlaceholder);
        }

        private async void _exportBtn_Clicked(object? sender, EventArgs e)
        {
            if (_preview is null)
                return;

            // Select file to save at
            var sfd = new WindowsSaveFileDialog
            {
                Title = LocalizationResources.TextPreviewExportPng,
                InitialFileName = "preview.png"
            };

            if (await sfd.ShowAsync() is DialogResult.Ok)
                await _preview.SaveAsPngAsync(sfd.Files[0]);
        }

        private async void _previewTextEditor_TextChanged(object? sender, string e)
        {
            _deserializedText = DeserializeText(_previewTextEditor.GetText());

            await UpdatePreview();
        }

        private async void _previewBox_SelectedItemChanged(object? sender, EventArgs e)
        {
            _deserializedText = DeserializeText(_previewTextEditor.GetText());

            await UpdatePreview();
        }

        private async Task UpdatePreview()
        {
            _preview = await CreatePreview();

            _exportBtn.Enabled = _preview is not null;
            _textPreview.SetImage(_preview is null ? null : ImageResource.FromImage(_preview));
        }

        private async Task<Image<Rgba32>?> CreatePreview()
        {
            if (_previewBox.SelectedItem?.Content is null)
                return null;

            _deserializedText ??= DeserializeText(_previewTextEditor.GetText());
            var preview = await _previewBox.SelectedItem.Content.RenderPreview(_deserializedText);

            return preview;
        }

        private IList<CharacterData> DeserializeText(string text)
        {
            var deserializer = _previewBox.SelectedItem?.Content?.Deserializer ?? new CharacterDeserializer();
            var characters = deserializer.Deserialize(text);

            return characters;
        }
    }
}
