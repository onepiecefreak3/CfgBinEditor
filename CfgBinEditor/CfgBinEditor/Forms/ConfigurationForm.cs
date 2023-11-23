﻿using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using CfgBinEditor.Messages;
using CfgBinEditor.resources;
using CrossCutting.Core.Contract.EventBrokerage;
using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Base;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Models;
using Logic.Business.CfgBinValueSettingsManagement.Contract;
using Logic.Business.CfgBinValueSettingsManagement.Contract.DataClasses;
using Logic.Domain.Level5.Contract;
using Logic.Domain.Level5.Contract.DataClasses;
using ValueType = Logic.Domain.Level5.Contract.DataClasses.ValueType;

namespace CfgBinEditor.Forms
{
    public partial class ConfigurationForm : Component
    {
        private readonly string GameName_ = "YKW2";

        private readonly Configuration _config;
        private readonly IEventBroker _events;
        private readonly IConfigurationWriter _writer;
        private readonly IValueSettingsProvider _settingsProvider;

        public ConfigurationForm(Configuration config, IEventBroker events, IConfigurationWriter writer, IValueSettingsProvider settingsProvider)
        {
            InitializeComponent(config);

            _config = config;
            _events = events;
            _writer = writer;
            _settingsProvider = settingsProvider;

            _flatEntryTree.SelectedNodeChanged += (s, e) => ChangeSelectedEntry(_flatEntryTree.SelectedNode.Data);
            _nestedTreeView.SelectedNodeChanged += (s, e) => ChangeSelectedEntry(_nestedTreeView.SelectedNode.Data);

            if (_flatEntryTree.Nodes.Count > 0)
                _flatEntryTree.SelectedNode = _flatEntryTree.Nodes[0];
            if (_nestedTreeView.Nodes.Count > 0)
                _nestedTreeView.SelectedNode = _nestedTreeView.Nodes[0];

            events.Subscribe<ValueSettingsChangedMessage>(ChangeValueSettings);
            events.Subscribe<FileSaveRequestMessage>(SaveFile);
        }

        private void ChangeSelectedEntry(ConfigurationEntry entry)
        {
            var layout = new TableLayout { Size = new Size(SizeValue.Parent, SizeValue.Content), Spacing = new Vector2(5, 5) };

            var headerRow = new TableRow
            {
                Cells =
                {
                    new Label { Text = LocalizationResources.CfgBinEntryNameCaption },
                    new Label { Text = LocalizationResources.CfgBinEntryTypeCaption },
                    new Label { Text = LocalizationResources.CfgBinEntryValueCaption },
                    new Label { Text = LocalizationResources.CfgBinEntryIsHexCaption }
                }
            };
            layout.Rows.Add(headerRow);

            for (var i = 0; i < entry.Values.Length; i++)
            {
                ConfigurationEntryValue entryValue = entry.Values[i];
                ValueSettingEntry settingEntry = _settingsProvider.GetEntrySettings(GameName_, entry.Name, i);

                var valueNameTextBox = new TextBox();
                SetValueNameText(valueNameTextBox, settingEntry.Name);
                valueNameTextBox.TextChanged += ValueNameTextBox_TextChanged;

                var typeComboBox = new ComboBox<ValueType>
                {
                    Items =
                    {
                        new ComboBoxItem<ValueType>(ValueType.String, LocalizationResources.CfgBinEntryTypeStringCaption),
                        new ComboBoxItem<ValueType>(ValueType.UInt, LocalizationResources.CfgBinEntryTypeUIntCaption),
                        new ComboBoxItem<ValueType>(ValueType.Float, LocalizationResources.CfgBinEntryTypeFloatCaption)
                    }
                };
                typeComboBox.SelectedItem = typeComboBox.Items.FirstOrDefault(x => x.Content == entryValue.Type);
                typeComboBox.SelectedItemChanged += TypeComboBox_SelectedItemChanged;

                var valueTextBox = new TextBox();
                SetValueText(valueTextBox, entryValue.Type, entryValue.Value, settingEntry.IsHex);
                valueTextBox.TextChanged += ValueTextBox_TextChanged;

                var valueIsHexCheckbox = new CheckBox { Checked = settingEntry.IsHex };

                var valueRow = new TableRow
                {
                    Cells =
                    {
                        new TableCell(valueNameTextBox) { Size = new Size(SizeValue.Absolute(200), SizeValue.Content) },
                        typeComboBox,
                        valueTextBox,
                        new TableCell(valueIsHexCheckbox){HorizontalAlignment = HorizontalAlignment.Center}
                    }
                };
                layout.Rows.Add(valueRow);
            }

            _configContent.Content = layout;
        }

        private void ChangeValueSettings(ValueSettingsChangedMessage message)
        {
            if (message.Sender == this)
                return;

            if (message.GameName != GameName_)
                return;

            if (message.EntryName != _nestedTreeView.SelectedNode.Data.Name)
                return;

            var layout = _configContent.Content as TableLayout;
            if (layout == null)
                return;

            for (var i = 1; i < layout.Rows.Count; i++)
            {
                var entrySettings = _settingsProvider.GetEntrySettings(message.GameName, message.EntryName, i - 1);

                var nameTextBox = layout.Rows[i].Cells[0].Content as TextBox;
                if (nameTextBox == null)
                    continue;

                SetValueNameText(nameTextBox, entrySettings.Name);
            }
        }

        private void SaveFile(FileSaveRequestMessage msg)
        {
            if (!msg.ConfigForms.TryGetValue(this, out string savePath))
                return;

            using Stream fileStream = _writer.Write(_config);
            using Stream targetFileStream = File.Create(savePath);
            
            fileStream.CopyTo(targetFileStream);

            RaiseFileSaved();
        }

        private void ValueNameTextBox_TextChanged(object sender, EventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null)
                return;

            var layout = _configContent.Content as TableLayout;
            if (layout == null)
                return;

            var row = layout.Rows.FirstOrDefault(r => r.Cells[0].Content == textBox);
            var rowIndex = layout.Rows.IndexOf(row);
            if (rowIndex < 0)
                return;

            var configEntry = _nestedTreeView.SelectedNode.Data;
            var settingsEntry = _settingsProvider.GetEntrySettings(GameName_, configEntry.Name, rowIndex - 1);

            settingsEntry.Name = textBox.Text.Replace(' ', '_');

            _settingsProvider.SetEntrySettings(GameName_, configEntry.Name, rowIndex - 1, settingsEntry);

            for (var i = 1; i <= rowIndex; i++)
            {
                var nameTextBox = layout.Rows[i].Cells[0].Content as TextBox;
                if (nameTextBox == null || !string.IsNullOrEmpty(nameTextBox.Text))
                    continue;

                var valueName = _settingsProvider.GetEntrySettings(GameName_, configEntry.Name, i - 1).Name;
                SetValueNameText(nameTextBox, valueName);
            }

            _settingsProvider.Persist();

            RaiseValueSettingsChanged(GameName_, configEntry.Name);
        }

        private void TypeComboBox_SelectedItemChanged(object sender, EventArgs e)
        {
            var comboBox = sender as ComboBox<ValueType>;
            if (comboBox == null)
                return;

            var layout = _configContent.Content as TableLayout;
            if (layout == null)
                return;

            var row = layout.Rows.FirstOrDefault(r => r.Cells[1].Content == comboBox);
            var rowIndex = layout.Rows.IndexOf(row);
            if (rowIndex < 0)
                return;

            var configEntry = _nestedTreeView.SelectedNode.Data;
            var settingsEntry = _settingsProvider.GetEntrySettings(GameName_, configEntry.Name, rowIndex - 1);

            var newValueType = comboBox.SelectedItem.Content;
            object newValue = newValueType == ValueType.String ? string.Empty : 0;

            SetEntryType(configEntry, rowIndex - 1, newValueType);
            SetEntryValue(configEntry, rowIndex - 1, newValue);

            var valueTextBox = row!.Cells[2].Content as TextBox;
            SetValueText(valueTextBox, newValueType, newValue, settingsEntry.IsHex);
        }

        private void ValueTextBox_TextChanged(object sender, EventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null)
                return;

            var layout = _configContent.Content as TableLayout;
            if (layout == null)
                return;

            var row = layout.Rows.FirstOrDefault(r => r.Cells[2].Content == textBox);
            var rowIndex = layout.Rows.IndexOf(row);
            if (rowIndex < 0)
                return;

            var configEntry = _nestedTreeView.SelectedNode.Data;
            var settingsEntry = _settingsProvider.GetEntrySettings(GameName_, configEntry.Name, rowIndex - 1);

            var valueType = configEntry.Values[rowIndex - 1].Type;
            switch (valueType)
            {
                case ValueType.String:
                    SetEntryValue(configEntry, rowIndex - 1, textBox.Text);
                    break;

                case ValueType.UInt:
                    if (settingsEntry.IsHex && textBox.Text.StartsWith("0x"))
                    {
                        if (uint.TryParse(textBox.Text[2..], NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var uValue))
                            SetEntryValue(configEntry, rowIndex - 1, uValue);
                    }
                    else
                    {
                        if (uint.TryParse(textBox.Text, out var uValue))
                            SetEntryValue(configEntry, rowIndex - 1, uValue);
                    }
                    break;

                case ValueType.Float:
                    if (float.TryParse(textBox.Text, out var fValue))
                        SetEntryValue(configEntry, rowIndex - 1, fValue);
                    break;
            }
        }

        private void SetValueNameText(TextBox nameTextBox, string text)
        {
            nameTextBox.TextChanged -= ValueNameTextBox_TextChanged;
            nameTextBox.Text = text;
            nameTextBox.TextChanged += ValueNameTextBox_TextChanged;
        }

        private void SetValueText(TextBox valueTextBox, ValueType type, object value, bool isHex)
        {
            switch (type)
            {
                case ValueType.String:
                    valueTextBox.Text = $"{value}";
                    break;

                case ValueType.UInt:
                    valueTextBox.Text = isHex ? $"0x{value:X8}" : $"{value}";
                    break;

                case ValueType.Float:
                    valueTextBox.Text = $"{value}";
                    break;
            }
        }

        private void SetEntryType(ConfigurationEntry entry, int index, ValueType type)
        {
            entry.Values[index].Type = type;
            RaiseFileChanged();
        }

        private void SetEntryValue(ConfigurationEntry entry, int index, object value)
        {
            entry.Values[index].Value = value;
            RaiseFileChanged();
        }

        private void RaiseValueSettingsChanged(string gameName, string entryName)
        {
            _events.Raise(new ValueSettingsChangedMessage(this, gameName, entryName));
        }

        private void RaiseFileChanged()
        {
            _events.Raise(new FileChangedMessage(this));
        }

        private void RaiseFileSaved()
        {
            _events.Raise(new FileSavedMessage(this));
        }
    }
}
