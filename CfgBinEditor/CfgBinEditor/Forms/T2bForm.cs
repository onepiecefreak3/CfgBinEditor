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
using ImGui.Forms.Controls.Text;
using ImGui.Forms.Localization;
using ImGui.Forms.Modals.IO;
using ImGui.Forms.Models;
using Logic.Business.CfgBinEditorManagement.Contract;
using Logic.Business.CfgBinEditorManagement.Contract.DataClasses;
using Logic.Domain.Level5Management.Contract;
using Logic.Domain.Level5Management.Contract.DataClasses;
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

            _valueAddButton.Clicked += (s, e) => AddEntryValue();

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
            _valueAddButton.Enabled = true;

            var layout = new TableLayout { Size = new Size(SizeValue.Parent, SizeValue.Content), Spacing = new Vector2(5, 5) };

            var headerRow = new TableRow
            {
                Cells =
                {
                    new Label { Text = LocalizationResources.CfgBinEntryNameCaption },
                    new Label { Text = LocalizationResources.CfgBinEntryTypeCaption },
                    new Label { Text = LocalizationResources.CfgBinEntryValueCaption },
                    new Label { Text = string.Empty },
                    new Label { Text = LocalizationResources.CfgBinEntryIsHexCaption }
                }
            };
            layout.Rows.Add(headerRow);

            if (entry == null)
            {
                _configContent.Content = layout;
                return;
            }

            for (var i = 0; i < entry.Values.Length; i++)
            {
                TableRow valueRow = CreateValueRow(entry, i);
                layout.Rows.Add(valueRow);
            }

            _configContent.Content = layout;
        }

        private void AddEntryValue()
        {
            T2bEntry? entry = _treeViewForm.SelectedEntry;
            if (entry == null)
                return;

            // Add value entry
            T2bEntryValue[] values = entry.Values;
            Array.Resize(ref values, values.Length + 1);
            entry.Values = values;

            entry.Values[^1] = new T2bEntryValue
            {
                Type = ValueType.Integer,
                Value = GetDefaultValue(ValueType.Integer)
            };

            // Add table row
            TableRow newValueRow = CreateValueRow(entry, entry.Values.Length - 1);
            ((TableLayout)_configContent.Content).Rows.Add(newValueRow);

            RaiseFileChanged();
        }

        private TableRow CreateValueRow(T2bEntry entry, int index)
        {
            string currentGame = GetCurrentGame();

            T2bEntryValue entryValue = entry.Values[index];
            ValueSettingEntry settingEntry = _settingsProvider.GetEntrySettings(currentGame, entry.Name, index);

            var valueNameTextBox = new TextBox { Enabled = currentGame != LocalizationResources.GameNoneCaption };
            valueNameTextBox.TextChanged += ValueNameTextBox_TextChanged;

            SetValueNameText(valueNameTextBox, settingEntry.Name);

            var typeComboBox = new ComboBox<ValueType>
            {
                Items =
                {
                    new DropDownItem<ValueType>(ValueType.String, LocalizationResources.CfgBinEntryTypeStringCaption),
                    new DropDownItem<ValueType>(ValueType.Integer, LocalizationResources.CfgBinEntryTypeIntCaption),
                    new DropDownItem<ValueType>(ValueType.FloatingPoint, LocalizationResources.CfgBinEntryTypeFloatCaption)
                }
            };
            typeComboBox.SelectedItem = typeComboBox.Items.FirstOrDefault(x => x.Content == entryValue.Type);
            typeComboBox.SelectedItemChanged += TypeComboBox_SelectedItemChanged;

            var valueTextBox = new TextBox();
            valueTextBox.TextChanged += ValueTextBox_TextChanged;

            SetValueText(valueTextBox, entryValue, settingEntry.IsHex);

            var actionButton = CreateValueActionButton(entryValue);

            var valueIsHexCheckbox = new CheckBox { Checked = settingEntry.IsHex, Enabled = currentGame != LocalizationResources.GameNoneCaption };
            valueIsHexCheckbox.CheckChanged += ValueIsHexCheckbox_CheckChanged;

            return new TableRow
            {
                Cells =
                {
                    new TableCell(valueNameTextBox) { Size = new Size(SizeValue.Absolute(200), SizeValue.Content) },
                    typeComboBox,
                    valueTextBox,
                    actionButton,
                    new TableCell(valueIsHexCheckbox){HorizontalAlignment = HorizontalAlignment.Center}
                }
            };
        }

        private Component CreateValueActionButton(T2bEntryValue entryValue)
        {
            switch (entryValue.Type)
            {
                case ValueType.Integer:
                case ValueType.FloatingPoint:
                    var randomButton = new ImageButton
                    {
                        Image = ImageResources.Random,
                        ImageSize = new Vector2(17, 17),
                        Padding = Vector2.One,
                        Tooltip = LocalizationResources.CfgBinEntryRandomTooltip
                    };
                    randomButton.Clicked += RandomButton_Clicked;

                    return randomButton;

                case ValueType.String:
                    var actionButton = new ImageButton
                    {
                        Image = ImageResources.Close,
                        ImageSize = new Vector2(17, 17),
                        Padding = Vector2.One,
                        Tooltip = LocalizationResources.CfgBinEntryEmptyTooltip
                    };
                    actionButton.Clicked += EmptyButton_Clicked;

                    return actionButton;

                default:
                    throw new InvalidOperationException($"Unknown value type {entryValue.Type}.");
            }
        }

        private void ChangeGame(LocalizedString gameName)
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

        private void ChangeValueSettings(LocalizedString gameName, string entryName)
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

                nameTextBox.Enabled = gameName != LocalizationResources.GameNoneCaption;
                SetValueNameText(nameTextBox, entrySettings.Name);

                var valueTextBox = layout.Rows[i].Cells[2].Content as TextBox;
                if (valueTextBox == null)
                    continue;

                var isHexCheckbox = layout.Rows[i].Cells[4].Content as CheckBox;
                if (isHexCheckbox == null)
                    continue;

                isHexCheckbox.Enabled = gameName != LocalizationResources.GameNoneCaption;
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
            _gameComboBox.Items.Add(new DropDownItem<LocalizedString>(msg.Game));

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

            var randomBtn = row.Cells[3].Content as ImageButton;
            if (randomBtn == null)
                return;

            var configEntry = _treeViewForm.SelectedEntry;
            var settingsEntry = _settingsProvider.GetEntrySettings(GetCurrentGame(), configEntry.Name, rowIndex - 1);

            ValueType newValueType = comboBox.SelectedItem.Content;
            object? newValue = ConvertValue(configEntry.Values[rowIndex - 1].Value, configEntry.Values[rowIndex - 1].Type, newValueType);

            SetEntryType(configEntry, rowIndex - 1, newValueType);
            SetEntryValue(configEntry, rowIndex - 1, newValue);

            SetValueText(valueTextBox, newValueType, newValue, settingsEntry.IsHex);

            row.Cells[3] = CreateValueActionButton(configEntry.Values[rowIndex - 1]);
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

        private void RandomButton_Clicked(object? sender, EventArgs e)
        {
            var randomBtn = sender as ImageButton;
            if (randomBtn == null)
                return;

            var layout = _configContent.Content as TableLayout;
            if (layout == null)
                return;

            var row = layout.Rows.FirstOrDefault(r => r.Cells[3].Content == randomBtn);
            var rowIndex = layout.Rows.IndexOf(row);
            if (rowIndex < 0)
                return;

            var valueTextBox = row.Cells[2].Content as TextBox;
            if (valueTextBox == null)
                return;

            var checkBox = row.Cells[4].Content as CheckBox;
            if (checkBox == null)
                return;

            var configEntry = _treeViewForm.SelectedEntry;
            ValueType valueType = configEntry.Values[rowIndex - 1].Type;

            var random = new Random();
            object randomValue;
            switch (valueType)
            {
                case ValueType.Integer:
                    switch (_config.ValueLength)
                    {
                        case ValueLength.Int:
                            randomValue = random.Next();
                            break;

                        case ValueLength.Long:
                            randomValue = random.NextInt64();
                            break;

                        default:
                            throw new InvalidOperationException($"Unknown value length {_config.ValueLength}.");
                    }
                    break;

                case ValueType.FloatingPoint:
                    switch (_config.ValueLength)
                    {
                        case ValueLength.Int:
                            randomValue = random.NextSingle();
                            break;

                        case ValueLength.Long:
                            randomValue = random.NextDouble();
                            break;

                        default:
                            throw new InvalidOperationException($"Unknown value length {_config.ValueLength}.");
                    }
                    break;

                default:
                    throw new InvalidOperationException($"Unknown value type {valueType}.");
            }

            SetEntryValue(configEntry, rowIndex - 1, randomValue);
            SetValueText(valueTextBox, configEntry.Values[rowIndex - 1], checkBox.Checked);
        }

        private void EmptyButton_Clicked(object? sender, EventArgs e)
        {
            var emptyBtn = sender as ImageButton;
            if (emptyBtn == null)
                return;

            var layout = _configContent.Content as TableLayout;
            if (layout == null)
                return;

            var row = layout.Rows.FirstOrDefault(r => r.Cells[3].Content == emptyBtn);
            var rowIndex = layout.Rows.IndexOf(row);
            if (rowIndex < 0)
                return;

            var valueTextBox = row.Cells[2].Content as TextBox;
            if (valueTextBox == null)
                return;

            var configEntry = _treeViewForm.SelectedEntry;

            SetEntryValue(configEntry, rowIndex - 1, null);
            SetValueText(valueTextBox, configEntry.Values[rowIndex - 1], false);
        }

        private void ValueIsHexCheckbox_CheckChanged(object sender, EventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox == null)
                return;

            var layout = _configContent.Content as TableLayout;
            if (layout == null)
                return;

            var row = layout.Rows.FirstOrDefault(r => r.Cells[4].Content == checkBox);
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

        private void SetValueText(TextBox valueTextBox, ValueType type, object? value, bool isHex)
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

        private void SetEntryValue(T2bEntry entry, int index, object? value)
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
                    NumberStyles styles = isHex ? NumberStyles.HexNumber : NumberStyles.AllowLeadingSign;
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

        private object? ConvertValue(object? value, ValueType sourceType, ValueType targetType)
        {
            if (sourceType == targetType)
                return value;

            switch (sourceType)
            {
                case ValueType.String:
                    var sValue = (string?)value;
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
                                    return (float)(int)value!;

                                case ValueLength.Long:
                                    return (double)(long)value!;

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
                                    return (int)Math.Round((float)value!);

                                case ValueLength.Long:
                                    return (long)Math.Round((double)value!);

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

        private string GetValueString(object? value, ValueType type, bool isHex)
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
                            int iValue = BitConverter.SingleToInt32Bits((float)value!);
                            return $"0x{iValue:X8}";

                        case ValueLength.Long:
                            long lValue = BitConverter.DoubleToInt64Bits((double)value!);
                            return $"0x{lValue:X16}";

                        default:
                            throw new InvalidOperationException($"Unknown value length {_config.ValueLength}.");
                    }

                default:
                    throw new InvalidOperationException($"Unknown value type {type}.");
            }
        }

        private LocalizedString GetCurrentGame()
        {
            return _gameComboBox.SelectedItem.Content;
        }

        private void RaiseValueSettingsChanged(LocalizedString gameName, string entryName)
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
