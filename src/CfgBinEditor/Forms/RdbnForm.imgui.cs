using CfgBinEditor.Components;
using CfgBinEditor.InternalContract;
using CfgBinEditor.resources;
using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Models;
using ImGui.Forms.Support;
using Konnect.Contract.Management.Plugin;
using Logic.Domain.Level5Management.Contract.DataClasses;

namespace CfgBinEditor.Forms
{
    public partial class RdbnForm
    {
        private RdbnTreeViewForm _treeViewForm;

        private StackLayout _contentLayout;
        private Panel _contentPanel;

        private void InitializeComponent(Rdbn config, IFormFactory formFactory, IPluginManager pluginManager)
        {
            var fontPreview = new FontPreviewComponent(pluginManager) { Size = Size.Parent };

            var valueLayout = new StackLayout { Alignment = Alignment.Vertical, ItemSpacing = 5 };

            _contentLayout = new StackLayout { Alignment = Alignment.Horizontal, ItemSpacing = 5 };
            _contentPanel = new Panel();

            _treeViewForm = formFactory.CreateRdbnTreeViewForm(config);

            valueLayout.Items.Add(_contentPanel);
            valueLayout.Items.Add(new Expander(fontPreview) { WidthIndent = 0, Caption = LocalizationResources.TextPreviewCaption, Size = Size.Parent });

            _contentLayout.Items.Add(new StackItem(_treeViewForm) { Size = new Size(SizeValue.Relative(.4f), SizeValue.Parent) });
            _contentLayout.Items.Add(valueLayout);
        }

        public override Size GetSize()
        {
            return Size.Parent;
        }

        protected override void UpdateInternal(Rectangle contentRect)
        {
            _contentLayout.Update(contentRect);
        }
    }
}
