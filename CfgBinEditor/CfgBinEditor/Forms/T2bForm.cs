using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using CfgBinEditor.InternalContract;
using CfgBinEditor.Messages;
using CfgBinEditor.resources;
using CrossCutting.Core.Contract.Configuration.DataClasses;
using CrossCutting.Core.Contract.EventBrokerage;
using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Base;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Modals.IO;
using ImGui.Forms.Models;
using Logic.Business.CfgBinValueSettingsManagement.Contract;
using Logic.Business.CfgBinValueSettingsManagement.Contract.DataClasses;
using Logic.Domain.Level5Management.Contract;
using Logic.Domain.Level5Management.Contract.DataClasses;
using Newtonsoft.Json.Linq;
using ValueType = Logic.Domain.Level5Management.Contract.DataClasses.ValueType;

namespace CfgBinEditor.Forms
{
    public partial class T2bForm : Component
    {
        private readonly T2b _config;
        private readonly IEventBroker _eventBroker;
        private readonly IT2bWriter _writer;
        private readonly IValueSettingsProvider _settingsProvider;

        public T2bForm(T2b config, IFormFactory formFactory, IEventBroker eventBroker, IT2bWriter writer, IValueSettingsProvider settingsProvider)
        {
            InitializeComponent(config, formFactory, settingsProvider);

            _config = config;
            _eventBroker = eventBroker;
            _writer = writer;
            _settingsProvider = settingsProvider;

            _gameComboBox.SelectedItemChanged += (s, e) => ChangeGame(_gameComboBox.SelectedItem.Content);
            _gameAddButton.Clicked += (s, e) => AddNewGame();

            eventBroker.Subscribe<ValueSettingsChangedMessage>(ChangeValueSettings);
            eventBroker.Subscribe<FileSaveRequestMessage>(SaveFile);
            eventBroker.Subscribe<GameAddedMessage>(AddGame);

            if (_treeViewForm.SelectedEntry != null)
                ChangeEntry(_treeViewForm.SelectedEntry);

            _eventBroker.Subscribe<TreeChangedMessage<T2b, T2bEntry>>(msg =>
            {
                if (msg.TreeViewForm == _treeViewForm)
                    RaiseFileChanged();
            });

            _eventBroker.Subscribe<TreeEntryChangedMessage<T2b, T2bEntry>>(msg =>
            {
                if (msg.TreeViewForm == _treeViewForm)
                    ChangeEntry(msg.Entry);
            });
        }

        private void ChangeEntry(T2bEntry? entry)
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

            if (entry == null)
            {
                _configContent.Content = layout;
                return;
            }

            string currentGame = GetCurrentGame();
            for (var i = 0; i < entry.Values.Length; i++)
            {
                T2bEntryValue entryValue = entry.Values[i];
                ValueSettingEntry settingEntry = _settingsProvider.GetEntrySettings(currentGame, entry.Name, i);

                var valueNameTextBox = new TextBox { Enabled = currentGame != NoGame_ };
                valueNameTextBox.TextChanged += ValueNameTextBox_TextChanged;

                SetValueNameText(valueNameTextBox, settingEntry.Name);

                var typeComboBox = new ComboBox<ValueType>
                {
                    Items =
                    {
                        new ComboBoxItem<ValueType>(ValueType.String, LocalizationResources.CfgBinEntryTypeStringCaption),
                        new ComboBoxItem<ValueType>(ValueType.Integer, LocalizationResources.CfgBinEntryTypeIntCaption),
                        new ComboBoxItem<ValueType>(ValueType.FloatingPoint, LocalizationResources.CfgBinEntryTypeFloatCaption)
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
            if (!msg.ConfigForms.TryGetValue(this, out string? savePath))
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

            var valueTextBox = row!.Cells[2].Content as TextBox;
            if (valueTextBox == null)
                return;

            var configEntry = _treeViewForm.SelectedEntry;
            var settingsEntry = _settingsProvider.GetEntrySettings(GetCurrentGame(), configEntry.Name, rowIndex - 1);

            ValueType newValueType = comboBox.SelectedItem.Content;
            object newValue = ConvertValue(configEntry.Values[rowIndex - 1].Value, configEntry.Values[rowIndex - 1].Type, newValueType);

            SetEntryType(configEntry, rowIndex - 1, newValueType);
            SetEntryValue(configEntry, rowIndex - 1, newValue);

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

            ValueType valueType = configEntry.Values[rowIndex - 1].Type;
            if (TryParseValue(textBox.Text, valueType, settingsEntry.IsHex, out object? parsedValue))
                SetEntryValue(configEntry, rowIndex - 1, parsedValue!);
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

            if (configEntry.Values[rowIndex - 1].Type is ValueType.Integer or ValueType.FloatingPoint)
                SetValueText(valueTextBox, configEntry.Values[rowIndex - 1], checkBox.Checked);
        }

        private void SetValueNameText(TextBox nameTextBox, string text)
        {
            nameTextBox.TextChanged -= ValueNameTextBox_TextChanged;
            nameTextBox.Text = text;
            nameTextBox.TextChanged += ValueNameTextBox_TextChanged;
        }

        private void SetValueText(TextBox valueTextBox, T2bEntryValue value, bool isHex)
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

        private void SetEntryType(T2bEntry entry, int index, ValueType type)
        {
            entry.Values[index].Type = type;
            RaiseFileChanged();
        }

        private void SetEntryValue(T2bEntry entry, int index, object value)
        {
            entry.Values[index].Value = value;
            RaiseFileChanged();
        }

        private bool TryParseValue(string text, ValueType type, bool isHex, out object? parsedValue)
        {
            parsedValue = null;

            switch (type)
            {
                case ValueType.String:
                    parsedValue = text;
                    return true;

                case ValueType.Integer:
                    NumberStyles styles = isHex ? NumberStyles.HexNumber : NumberStyles.None;
                    text = isHex ? text.StartsWith("0x") ? text[2..] : text : text;

                    switch (_config.ValueLength)
                    {
                        case ValueLength.Int:
                            if (!int.TryParse(text, styles, CultureInfo.InvariantCulture, out int iValue))
                                return false;

                            parsedValue = iValue;
                            return true;

                        case ValueLength.Long:
                            if (!long.TryParse(text, styles, CultureInfo.InvariantCulture, out long lValue))
                                return false;

                            parsedValue = lValue;
                            return true;

                        default:
                            throw new InvalidOperationException($"Unknown value length {_config.ValueLength}.");
                    }

                case ValueType.FloatingPoint:
                    NumberStyles styles1 = isHex ? NumberStyles.HexNumber : NumberStyles.None;
                    text = isHex ? text.StartsWith("0x") ? text[2..] : text : text;

                    switch (_config.ValueLength)
                    {
                        case ValueLength.Int:
                            if (!float.TryParse(text, styles1, CultureInfo.InvariantCulture, out float fValue))
                                return false;

                            parsedValue = fValue;
                            return true;

                        case ValueLength.Long:
                            if (!double.TryParse(text, styles1, CultureInfo.InvariantCulture, out double dValue))
                                return false;

                            parsedValue = dValue;
                            return true;

                        default:
                            throw new InvalidOperationException($"Unknown value length {_config.ValueLength}.");
                    }

                default:
                    throw new InvalidOperationException($"Unknown value type {type}.");
            }
        }

        private object ConvertValue(object value, ValueType sourceType, ValueType targetType)
        {
            if (sourceType == targetType)
                return value;

            switch (sourceType)
            {
                case ValueType.String:
                    var sValue = (string)value;
                    switch (targetType)
                    {
                        case ValueType.Integer:
                            switch (_config.ValueLength)
                            {
                                case ValueLength.Int:
                                    if (int.TryParse(sValue, out int iValue))
                                        return iValue;

                                    if (float.TryParse(sValue, out float fValue))
                                        return (int)Math.Round(fValue);

                                    return GetDefaultValue(targetType);

                                case ValueLength.Long:
                                    if (long.TryParse(sValue, out long lValue))
                                        return lValue;

                                    if (double.TryParse(sValue, out double dValue))
                                        return (long)Math.Round(dValue);

                                    return GetDefaultValue(targetType);

                                default:
                                    throw new InvalidOperationException($"Unknown value length {_config.ValueLength}.");
                            }

                        case ValueType.FloatingPoint:
                            switch (_config.ValueLength)
                            {
                                case ValueLength.Int:
                                    if (float.TryParse(sValue, out float fValue))
                                        return fValue;

                                    return GetDefaultValue(targetType);

                                case ValueLength.Long:
                                    if (double.TryParse(sValue, out double dValue))
                                        return dValue;

                                    return GetDefaultValue(targetType);

                                default:
                                    throw new InvalidOperationException($"Unknown value length {_config.ValueLength}.");
                            }

                        default:
                            throw new InvalidOperationException($"Unknown value type {targetType}.");
                    }

                case ValueType.Integer:
                    switch (targetType)
                    {
                        case ValueType.String:
                            return $"{value}";

                        case ValueType.FloatingPoint:
                            switch (_config.ValueLength)
                            {
                                case ValueLength.Int:
                                    return (float)(int)value;

                                case ValueLength.Long:
                                    return (double)(long)value;

                                default:
                                    throw new InvalidOperationException($"Unknown value length {_config.ValueLength}.");
                            }

                        default:
                            throw new InvalidOperationException($"Unknown value type {targetType}.");
                    }

                case ValueType.FloatingPoint:
                    switch (targetType)
                    {
                        case ValueType.String:
                            return $"{value}";

                        case ValueType.Integer:
                            switch (_config.ValueLength)
                            {
                                case ValueLength.Int:
                                    return (int)Math.Round((float)value);

                                case ValueLength.Long:
                                    return (long)Math.Round((double)value);

                                default:
                                    throw new InvalidOperationException($"Unknown value length {_config.ValueLength}.");
                            }

                        default:
                            throw new InvalidOperationException($"Unknown value type {targetType}.");
                    }

                default:
                    throw new InvalidOperationException($"Unknown value type {targetType}.");
            }
        }

        private object GetDefaultValue(ValueType type)
        {
            switch (type)
            {
                case ValueType.String:
                    return string.Empty;

                case ValueType.Integer:
                    switch (_config.ValueLength)
                    {
                        case ValueLength.Int:
                            return 0;

                        case ValueLength.Long:
                            return (long)0;

                        default:
                            throw new InvalidOperationException($"Unknown value length {_config.ValueLength}.");
                    }

                case ValueType.FloatingPoint:
                    switch (_config.ValueLength)
                    {
                        case ValueLength.Int:
                            return .0f;

                        case ValueLength.Long:
                            return .0d;

                        default:
                            throw new InvalidOperationException($"Unknown value length {_config.ValueLength}.");
                    }

                default:
                    throw new InvalidOperationException($"Unknown value type {type}.");
            }
        }

        private string GetValueString(object value, ValueType type, bool isHex)
        {
            switch (type)
            {
                case ValueType.String:
                    return $"{value}";

                case ValueType.Integer:
                    if (!isHex)
                        return $"{value}";

                    switch (_config.ValueLength)
                    {
                        case ValueLength.Int:
                            return $"0x{value:X8}";

                        case ValueLength.Long:
                            return $"0x{value:X16}";

                        default:
                            throw new InvalidOperationException($"Unknown value length {_config.ValueLength}.");
                    }

                case ValueType.FloatingPoint:
                    if (!isHex)
                        return $"{value}";

                    switch (_config.ValueLength)
                    {
                        case ValueLength.Int:
                            int iValue = BitConverter.SingleToInt32Bits((float)value);
                            return $"0x{iValue:X8}";

                        case ValueLength.Long:
                            long lValue = BitConverter.DoubleToInt64Bits((double)value);
                            return $"0x{lValue:X16}";

                        default:
                            throw new InvalidOperationException($"Unknown value length {_config.ValueLength}.");
                    }

                default:
                    throw new InvalidOperationException($"Unknown value type {type}.");
            }
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
