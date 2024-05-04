using System;
using CfgBinEditor.InternalContract;
using CfgBinEditor.resources;
using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Models;
using Logic.Business.CfgBinValueSettingsManagement.Contract;
using Logic.Domain.Level5Management.Contract.DataClasses;
using Rectangle = Veldrid.Rectangle;
using Size = ImGui.Forms.Models.Size;
using ValueType = Logic.Domain.Level5Management.Contract.DataClasses.ValueType;

namespace CfgBinEditor.Forms
{
    public partial class T2bForm
    {
        private const string NoGame_ = "None";

        private StackLayout _contentLayout;
        
        private T2bTreeViewForm _treeViewForm;
        private StackLayout _gameLayout;
        private StackLayout _valuesLayout;

        private ComboBox<string> _gameComboBox;
        private Button _gameAddButton;

        private Panel _configContent;

        private void InitializeComponent(T2b config, IFormFactory formFactory, IValueSettingsProvider settingsProvider)
        {
            _contentLayout = new StackLayout { Alignment = Alignment.Horizontal, ItemSpacing = 5 };
            
            _gameLayout = new StackLayout { Alignment = Alignment.Horizontal, ItemSpacing = 5, Size = new Size(SizeValue.Parent, SizeValue.Content) };
            _valuesLayout = new StackLayout { Alignment = Alignment.Vertical, ItemSpacing = 5 };

            _treeViewForm = formFactory.CreateT2bTreeViewForm(config, NoGame_);

            _gameComboBox = new ComboBox<string>();
            _gameAddButton = new Button { Text = LocalizationResources.GameAddButtonCaption };

            _configContent = new Panel();

            _gameLayout.Items.Add(_gameComboBox);
            _gameLayout.Items.Add(_gameAddButton);

            _valuesLayout.Items.Add(_gameLayout);
            _valuesLayout.Items.Add(_configContent);

            _contentLayout.Items.Add(new StackItem(_treeViewForm) { Size = new Size(SizeValue.Relative(.4f), SizeValue.Parent) });
            _contentLayout.Items.Add(_valuesLayout);

            InitializeGames(settingsProvider);
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
            _gameComboBox.Items.Add(NoGame_);
            _gameComboBox.SelectedItem = _gameComboBox.Items[0];

            foreach (string game in settingsProvider.GetGames())
                _gameComboBox.Items.Add(game);
        }

        private string GetValueString(object value, ValueType type, bool isHex)
        {
            switch (type)
            {
                case ValueType.String:
                    return $"{value}";

                case ValueType.Long:
                    return isHex ? $"0x{value:X8}" : $"{value}";

                case ValueType.Double:
                    return $"{value}";

                default:
                    throw new InvalidOperationException($"Unknown value type {type}.");
            }
        }
    }
}
