using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using CfgBinEditor.InternalContract;
using CfgBinEditor.Messages;
using CfgBinEditor.resources;
using CrossCutting.Core.Contract.EventBrokerage;
using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Base;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Modals.IO;
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
        private readonly Configuration _config;
        private readonly IEventBroker _eventBroker;
        private readonly IConfigurationWriter _writer;
        private readonly IValueSettingsProvider _settingsProvider;

        public ConfigurationForm(Configuration config, IFormFactory formFactory, IEventBroker eventBroker, IConfigurationWriter writer, IValueSettingsProvider settingsProvider)
        {
            InitializeComponent(config, formFactory, settingsProvider);

            _config = config;
            _eventBroker = eventBroker;
            _writer = writer;
            _settingsProvider = settingsProvider;

            _gameComboBox.SelectedItemChanged += (s, e) => ChangeGame(_gameComboBox.SelectedItem.Content);
            _gameAddButton.Clicked += (s, e) => AddNewGame();

            _treeViewForm.EntryChanged += (s, e) => ChangeEntry(e.Entry);

            eventBroker.Subscribe<ValueSettingsChangedMessage>(ChangeValueSettings);
            eventBroker.Subscribe<FileSaveRequestMessage>(SaveFile);
            eventBroker.Subscribe<GameAddedMessage>(AddGame);

            if (_treeViewForm.SelectedEntry != null)
                ChangeEntry(_treeViewForm.SelectedEntry);

            _eventBroker.Subscribe<ConfigurationTreeChangedMessage>(msg =>
            {
                if (msg.TreeViewForm == _treeViewForm)
                    RaiseFileChanged();
            });
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

                SetValueText(valueTextBox, entryValue, settingEntry.IsHex);

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
            if (_treeViewForm.SelectedEntry != null)
                ChangeValueSettings(gameName, _treeViewForm.SelectedEntry.Name);

            _treeViewForm.GameName = gameName;
        }

        private void ChangeValueSettings(ValueSettingsChangedMessage message)
        {
            if (message.Sender == this)
                return;

            if (message.GameName != GetCurrentGame())
                return;

            if (message.EntryName != _treeViewForm.SelectedEntry.Name)
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

                var valueTextBox = layout.Rows[i].Cells[2].Content as TextBox;
                if (valueTextBox == null)
                    continue;

                var isHexCheckbox = layout.Rows[i].Cells[3].Content as CheckBox;
                if (isHexCheckbox == null)
                    continue;

                isHexCheckbox.Enabled = gameName != NoGame_;
                SetValueIsHex(isHexCheckbox, entrySettings.IsHex);
                SetValueText(valueTextBox, _treeViewForm.SelectedEntry.Values[i - 1], entrySettings.IsHex);
            }
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

            _treeViewForm.ResetNodeState();

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

            var configEntry = _treeViewForm.SelectedEntry;
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

            var configEntry = _treeViewForm.SelectedEntry;
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

            var configEntry = _treeViewForm.SelectedEntry;
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

            var configEntry = _treeViewForm.SelectedEntry;
            var settingsEntry = _settingsProvider.GetEntrySettings(GetCurrentGame(), configEntry.Name, rowIndex - 1);

            settingsEntry.IsHex = checkBox.Checked;

            _settingsProvider.SetEntrySettings(GetCurrentGame(), configEntry.Name, rowIndex - 1, settingsEntry);

            var valueTextBox = layout.Rows[rowIndex].Cells[2].Content as TextBox;

            if (configEntry.Values[rowIndex - 1].Type == ValueType.Int)
                SetValueText(valueTextBox, configEntry.Values[rowIndex - 1], checkBox.Checked);
        }

        private void SetValueNameText(TextBox nameTextBox, string text)
        {
            nameTextBox.TextChanged -= ValueNameTextBox_TextChanged;
            nameTextBox.Text = text;
            nameTextBox.TextChanged += ValueNameTextBox_TextChanged;
        }

        private void SetValueText(TextBox valueTextBox, ConfigurationEntryValue value, bool isHex)
        {
            SetValueText(valueTextBox, value.Type, value.Value, isHex);
        }

        private void SetValueText(TextBox valueTextBox, ValueType type, object value, bool isHex)
        {
            valueTextBox.TextChanged -= ValueTextBox_TextChanged;
            valueTextBox.Text = GetValueString(value, type, isHex);
            valueTextBox.TextChanged += ValueTextBox_TextChanged;
        }

        private void SetValueIsHex(CheckBox isHexCheckBox, bool isHex)
        {
            isHexCheckBox.CheckChanged -= ValueIsHexCheckbox_CheckChanged;
            isHexCheckBox.Checked = isHex;
            isHexCheckBox.CheckChanged += ValueIsHexCheckbox_CheckChanged;
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
            _eventBroker.Raise(new ValueSettingsChangedMessage(this, gameName, entryName));
        }

        private void RaiseFileChanged()
        {
            _eventBroker.Raise(new FileChangedMessage(this));
        }

        private void RaiseFileSaved(Exception e = null)
        {
            _eventBroker.Raise(new FileSavedMessage(this, e));
        }

        private void RaiseGameAdded(string game)
        {
            _eventBroker.Raise(new GameAddedMessage(this, game));
        }
    }
}
