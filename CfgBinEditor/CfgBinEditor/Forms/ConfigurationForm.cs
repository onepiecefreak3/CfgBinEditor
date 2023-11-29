using System;
using System.Collections.Generic;
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
using ImGui.Forms.Controls.Tree;
using ImGui.Forms.Modals.IO;
using ImGui.Forms.Models;
using Logic.Business.CfgBinValueSettingsManagement.Contract;
using Logic.Business.CfgBinValueSettingsManagement.Contract.DataClasses;
using Logic.Domain.Level5.Contract;
using Logic.Domain.Level5.Contract.DataClasses;
using Veldrid.MetalBindings;
using ValueType = Logic.Domain.Level5.Contract.DataClasses.ValueType;

namespace CfgBinEditor.Forms
{
    public partial class ConfigurationForm : Component
    {
        private readonly Configuration _config;
        private readonly IEventBroker _events;
        private readonly IConfigurationWriter _writer;
        private readonly IValueSettingsProvider _settingsProvider;

        public ConfigurationForm(Configuration config, IEventBroker events, IConfigurationWriter writer, IValueSettingsProvider settingsProvider)
        {
            InitializeComponent(config, settingsProvider);

            _config = config;
            _events = events;
            _writer = writer;
            _settingsProvider = settingsProvider;

            _nestedTreeView.SelectedNodeChanged += (s, e) => ChangeEntry(_nestedTreeView.SelectedNode.Data);

            _gameComboBox.SelectedItemChanged += (s, e) => ChangeGame(_gameComboBox.SelectedItem.Content);
            _gameAddButton.Clicked += (s, e) => AddNewGame();

            _duplicateButton.Clicked += (s, e) => DuplicateNode(_nestedTreeView.SelectedNode);

            if (_nestedTreeView.Nodes.Count > 0)
                _nestedTreeView.SelectedNode = _nestedTreeView.Nodes[0];

            events.Subscribe<ValueSettingsChangedMessage>(ChangeValueSettings);
            events.Subscribe<FileSaveRequestMessage>(SaveFile);
            events.Subscribe<GameAddedMessage>(AddGame);
        }

        private void ChangeEntry(ConfigurationEntry entry)
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

            string currentGame = GetCurrentGame();
            for (var i = 0; i < entry.Values.Length; i++)
            {
                ConfigurationEntryValue entryValue = entry.Values[i];
                ValueSettingEntry settingEntry = _settingsProvider.GetEntrySettings(currentGame, entry.Name, i);

                var valueNameTextBox = new TextBox { Enabled = currentGame != NoGame_ };
                valueNameTextBox.TextChanged += ValueNameTextBox_TextChanged;

                SetValueNameText(valueNameTextBox, settingEntry.Name);

                var typeComboBox = new ComboBox<ValueType>
                {
                    Items =
                    {
                        new ComboBoxItem<ValueType>(ValueType.String, LocalizationResources.CfgBinEntryTypeStringCaption),
                        new ComboBoxItem<ValueType>(ValueType.Int, LocalizationResources.CfgBinEntryTypeIntCaption),
                        new ComboBoxItem<ValueType>(ValueType.Float, LocalizationResources.CfgBinEntryTypeFloatCaption)
                    }
                };
                typeComboBox.SelectedItem = typeComboBox.Items.FirstOrDefault(x => x.Content == entryValue.Type);
                typeComboBox.SelectedItemChanged += TypeComboBox_SelectedItemChanged;

                var valueTextBox = new TextBox();
                valueTextBox.TextChanged += ValueTextBox_TextChanged;

                SetValueText(valueTextBox, entryValue.Type, entryValue.Value, settingEntry.IsHex);

                var valueIsHexCheckbox = new CheckBox { Checked = settingEntry.IsHex, Enabled = currentGame != NoGame_ };
                valueIsHexCheckbox.CheckChanged += ValueIsHexCheckbox_CheckChanged;

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

        private void ChangeGame(string gameName)
        {
            ChangeValueSettings(gameName, _nestedTreeView.SelectedNode.Data.Name);
        }

        private void ChangeValueSettings(ValueSettingsChangedMessage message)
        {
            if (message.Sender == this)
                return;

            if (message.GameName != GetCurrentGame())
                return;

            if (message.EntryName != _nestedTreeView.SelectedNode.Data.Name)
                return;

            ChangeValueSettings(message.GameName, message.EntryName);
        }

        private void ChangeValueSettings(string gameName, string entryName)
        {
            var layout = _configContent.Content as TableLayout;
            if (layout == null)
                return;

            for (var i = 1; i < layout.Rows.Count; i++)
            {
                ValueSettingEntry entrySettings = _settingsProvider.GetEntrySettings(gameName, entryName, i - 1);

                var nameTextBox = layout.Rows[i].Cells[0].Content as TextBox;
                if (nameTextBox == null)
                    continue;

                nameTextBox.Enabled = gameName != NoGame_;
                SetValueNameText(nameTextBox, entrySettings.Name);

                var isHexCheckbox = layout.Rows[i].Cells[3].Content as CheckBox;
                if (isHexCheckbox == null)
                    continue;

                isHexCheckbox.Enabled = gameName != NoGame_;
            }
        }

        private void DuplicateNode(TreeNode<ConfigurationEntry> node)
        {
            TreeNode<ConfigurationEntry> lastNode = GetLastNode(node);

            int entryIndex = Array.IndexOf(_config.Entries, node.Data);
            int entryCount = CountEntries(node);

            int lastEntryIndex = lastNode == node ? entryIndex : Array.IndexOf(_config.Entries, lastNode.Data);
            int lastEntryCount = lastNode == node ? entryCount : CountEntries(lastNode);

            int newEntryIndex = lastEntryIndex + lastEntryCount;

            var newEntries = new ConfigurationEntry[_config.Entries.Length + entryCount];
            Array.Copy(_config.Entries, newEntries, newEntryIndex);
            Array.Copy(_config.Entries, newEntryIndex, newEntries, newEntryIndex + entryCount, _config.Entries.Length - newEntryIndex);

            for (var i = 0; i < entryCount; i++)
            {
                newEntries[newEntryIndex + i] = new ConfigurationEntry
                {
                    Name = _config.Entries[entryIndex + i].Name,
                    Values = new ConfigurationEntryValue[_config.Entries[entryIndex + i].Values.Length]
                };

                for (var j = 0; j < newEntries[newEntryIndex + i].Values.Length; j++)
                {
                    newEntries[newEntryIndex + i].Values[j] = new ConfigurationEntryValue
                    {
                        Type = _config.Entries[entryIndex + i].Values[j].Type,
                        Value = _config.Entries[entryIndex + i].Values[j].Value
                    };
                }
            }

            _config.Entries = newEntries;

            TreeNode<ConfigurationEntry> newNode = CreateNode(_config, ref newEntryIndex, ColorResources.TextSuccessful);

            IList<TreeNode<ConfigurationEntry>> nodes = GetNeighbourNodes(node);
            nodes.Add(newNode);

            RaiseFileChanged();
        }

        private async void AddNewGame()
        {
            string input = await InputBox.ShowAsync(LocalizationResources.GameAddDialogCaption,
                LocalizationResources.GameAddDialogText, string.Empty, LocalizationResources.GameAddDialogPlaceholder);

            if (string.IsNullOrEmpty(input))
                return;

            _settingsProvider.AddGame(input);

            RaiseGameAdded(input);
        }

        private void AddGame(GameAddedMessage msg)
        {
            _gameComboBox.Items.Add(msg.Game);

            if (msg.Sender != this)
                return;

            _gameComboBox.SelectedItem = _gameComboBox.Items[^1];

            ChangeGame(msg.Game);
        }

        private void SaveFile(FileSaveRequestMessage msg)
        {
            if (!msg.ConfigForms.TryGetValue(this, out string savePath))
                return;

            if (!TryWriteFile(savePath, out Exception e))
            {
                RaiseFileSaved(e);
                return;
            }

            ResetNodesChangeState(_nestedTreeView.Nodes);

            RaiseFileSaved();
        }

        private bool TryWriteFile(string savePath, out Exception ex)
        {
            ex = null;

            try
            {
                using Stream fileStream = _writer.Write(_config);
                using Stream targetFileStream = File.Create(savePath);

                fileStream.CopyTo(targetFileStream);
            }
            catch (Exception e)
            {
                ex = e;
                return false;
            }

            return true;
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
            var settingsEntry = _settingsProvider.GetEntrySettings(GetCurrentGame(), configEntry.Name, rowIndex - 1);

            settingsEntry.Name = textBox.Text.Replace(' ', '_');

            _settingsProvider.SetEntrySettings(GetCurrentGame(), configEntry.Name, rowIndex - 1, settingsEntry);

            for (var i = 1; i <= rowIndex; i++)
            {
                var nameTextBox = layout.Rows[i].Cells[0].Content as TextBox;
                if (nameTextBox == null || !string.IsNullOrEmpty(nameTextBox.Text))
                    continue;

                var valueName = _settingsProvider.GetEntrySettings(GetCurrentGame(), configEntry.Name, i - 1).Name;
                SetValueNameText(nameTextBox, valueName);
            }

            _settingsProvider.Persist();

            RaiseValueSettingsChanged(GetCurrentGame(), configEntry.Name);
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
            var settingsEntry = _settingsProvider.GetEntrySettings(GetCurrentGame(), configEntry.Name, rowIndex - 1);

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
            var settingsEntry = _settingsProvider.GetEntrySettings(GetCurrentGame(), configEntry.Name, rowIndex - 1);

            var valueType = configEntry.Values[rowIndex - 1].Type;
            switch (valueType)
            {
                case ValueType.String:
                    SetEntryValue(configEntry, rowIndex - 1, textBox.Text);
                    break;

                case ValueType.Int:
                    if (settingsEntry.IsHex && textBox.Text.StartsWith("0x"))
                    {
                        if (int.TryParse(textBox.Text[2..], NumberStyles.HexNumber, CultureInfo.CurrentCulture, out int iValue))
                            SetEntryValue(configEntry, rowIndex - 1, iValue);
                    }
                    else
                    {
                        if (int.TryParse(textBox.Text, out int iValue))
                            SetEntryValue(configEntry, rowIndex - 1, iValue);
                    }
                    break;

                case ValueType.Float:
                    if (float.TryParse(textBox.Text, out var fValue))
                        SetEntryValue(configEntry, rowIndex - 1, fValue);
                    break;
            }
        }

        private void ValueIsHexCheckbox_CheckChanged(object sender, EventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox == null)
                return;

            var layout = _configContent.Content as TableLayout;
            if (layout == null)
                return;

            var row = layout.Rows.FirstOrDefault(r => r.Cells[3].Content == checkBox);
            var rowIndex = layout.Rows.IndexOf(row);
            if (rowIndex < 0)
                return;

            var configEntry = _nestedTreeView.SelectedNode.Data;
            var settingsEntry = _settingsProvider.GetEntrySettings(GetCurrentGame(), configEntry.Name, rowIndex - 1);

            settingsEntry.IsHex = checkBox.Checked;

            _settingsProvider.SetEntrySettings(GetCurrentGame(), configEntry.Name, rowIndex - 1, settingsEntry);

            var valueType = configEntry.Values[rowIndex - 1].Type;
            var value = configEntry.Values[rowIndex - 1].Value;

            var valueTextBox = layout.Rows[rowIndex].Cells[2].Content as TextBox;

            if (valueType == ValueType.Int)
                SetValueText(valueTextBox, valueType, value, checkBox.Checked);
        }

        private void SetValueNameText(TextBox nameTextBox, string text)
        {
            nameTextBox.TextChanged -= ValueNameTextBox_TextChanged;
            nameTextBox.Text = text;
            nameTextBox.TextChanged += ValueNameTextBox_TextChanged;
        }

        private void SetValueText(TextBox valueTextBox, ValueType type, object value, bool isHex)
        {
            valueTextBox.TextChanged -= ValueTextBox_TextChanged;

            switch (type)
            {
                case ValueType.String:
                    valueTextBox.Text = $"{value}";
                    break;

                case ValueType.Int:
                    valueTextBox.Text = isHex ? $"0x{value:X8}" : $"{value}";
                    break;

                case ValueType.Float:
                    valueTextBox.Text = $"{value}";
                    break;
            }

            valueTextBox.TextChanged += ValueTextBox_TextChanged;
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

        private string GetCurrentGame()
        {
            return _gameComboBox.SelectedItem.Content;
        }

        private void RaiseValueSettingsChanged(string gameName, string entryName)
        {
            _events.Raise(new ValueSettingsChangedMessage(this, gameName, entryName));
        }

        private void RaiseFileChanged()
        {
            _events.Raise(new FileChangedMessage(this));
        }

        private void RaiseFileSaved(Exception e = null)
        {
            _events.Raise(new FileSavedMessage(this, e));
        }

        private void RaiseGameAdded(string game)
        {
            _events.Raise(new GameAddedMessage(this, game));
        }
    }
}
