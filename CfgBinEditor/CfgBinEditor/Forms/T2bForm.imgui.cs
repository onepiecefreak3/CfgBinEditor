using System.Numerics;
using CfgBinEditor.InternalContract;
using CfgBinEditor.resources;
using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Localization;
using ImGui.Forms.Models;
using Logic.Business.CfgBinEditorManagement.Contract;
using Logic.Domain.Level5Management.Contract.DataClasses;
using Rectangle = Veldrid.Rectangle;
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

        private void InitializeComponent(T2b config, IFormFactory formFactory, IValueSettingsProvider settingsProvider)
        {
            _contentLayout = new StackLayout { Alignment = Alignment.Horizontal, ItemSpacing = 5 };

            _gameLayout = new StackLayout { Alignment = Alignment.Horizontal, ItemSpacing = 5, Size = new Size(SizeValue.Parent, SizeValue.Content) };
            _valuesLayout = new StackLayout { Alignment = Alignment.Vertical, ItemSpacing = 5 };

            _treeViewForm = formFactory.CreateT2bTreeViewForm(config);

            _gameComboBox = new ComboBox<LocalizedString>();
            _gameAddButton = new Button { Text = LocalizationResources.GameAddButtonCaption };

            _valueAddButton = new Button { Text = LocalizationResources.CfgBinEntryAddValueCaption, Padding = new Vector2(15, 2), Enabled = false };

            _configContent = new Panel();

            _gameLayout.Items.Add(_gameComboBox);
            _gameLayout.Items.Add(_gameAddButton);
            _gameLayout.Items.Add(new StackItem(_valueAddButton) { HorizontalAlignment = HorizontalAlignment.Right, Size = Size.WidthAlign });

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

        protected override void SetTabInactiveCore()
        {
            _contentLayout.SetTabInactive();
        }

        private void InitializeGames(IValueSettingsProvider settingsProvider)
        {
            _gameComboBox.Items.Add(LocalizationResources.GameNoneCaption);
            _gameComboBox.SelectedItem = _gameComboBox.Items[0];

            foreach (string game in settingsProvider.GetGames())
                _gameComboBox.Items.Add(new ComboBoxItem<LocalizedString>(game));
        }
    }
}
