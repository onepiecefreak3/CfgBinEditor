using CfgBinEditor.InternalContract;
using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Models;
using Logic.Domain.Level5Management.Contract.DataClasses;
using Veldrid;

namespace CfgBinEditor.Forms
{
    public partial class RdbnForm
    {
        private RdbnTreeViewForm _treeViewForm;

        private StackLayout _contentLayout;
        private Panel _contentPanel;

        private void InitializeComponent(Rdbn config, IFormFactory formFactory)
        {
            _contentLayout = new StackLayout { Alignment = Alignment.Horizontal, ItemSpacing = 5 };
            _contentPanel = new Panel();

            _treeViewForm = formFactory.CreateRdbnTreeViewForm(config);

            _contentLayout.Items.Add(new StackItem(_treeViewForm) { Size = new Size(SizeValue.Relative(.4f), SizeValue.Parent) });
            _contentLayout.Items.Add(_contentPanel);
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
    }
}
