using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using CfgBinEditor.InternalContract;
using CfgBinEditor.InternalContract.DataClasses;
using CfgBinEditor.resources;
using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Base;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Models;
using Logic.Domain.Level5.Contract.DataClasses;
using ValueType = Logic.Domain.Level5.Contract.DataClasses.ValueType;

namespace CfgBinEditor.Forms
{
    public partial class ConfigurationForm : Component
    {
        private readonly Configuration _config;
        private readonly IValueSettings _settings;

        public ConfigurationForm(Configuration config, IValueSettings settings)
        {
            InitializeComponent(config);

            _config = config;
            _settings = settings;

            _flatEntryTree.SelectedNodeChanged += (s, e) => ChangeSelectedEntry(_flatEntryTree.SelectedNode.Data);
            _nestedTreeView.SelectedNodeChanged += (s, e) => ChangeSelectedEntry(_nestedTreeView.SelectedNode.Data);

            if (_flatEntryTree.Nodes.Count > 0)
                _flatEntryTree.SelectedNode = _flatEntryTree.Nodes[0];
            if (_nestedTreeView.Nodes.Count > 0)
                _nestedTreeView.SelectedNode = _nestedTreeView.Nodes[0];
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
                    new Label { Text = LocalizationResources.CfgBinEntryValueCaption }
                }
            };
            layout.Rows.Add(headerRow);

            for (var i = 0; i < entry.Values.Length; i++)
            {
                ConfigurationEntryValue entryValue = entry.Values[i];
                ValueSettingEntry settingEntry = _settings.GetEntrySettings("YKW2", entry.Name, i);

                var valueNameTextBox = new TextBox { Text = settingEntry.Name };

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

                var valueTextBox = new TextBox { Text = entryValue.Value.ToString() };

                var valueRow = new TableRow
                {
                    Cells =
                    {
                        new TableCell(valueNameTextBox) { Size = new Size(SizeValue.Absolute(200), SizeValue.Content) },
                        typeComboBox,
                        valueTextBox
                    }
                };
                layout.Rows.Add(valueRow);
            }

            _configContent.Content = layout;
        }
    }
}
