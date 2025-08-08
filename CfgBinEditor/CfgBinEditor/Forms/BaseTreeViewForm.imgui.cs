using CfgBinEditor.resources;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Controls.Menu;
using ImGui.Forms.Controls.Tree;
using ImGui.Forms.Controls;
using System.Collections.Generic;
using System.Numerics;
using ImGui.Forms.Controls.Text;
using ImGui.Forms.Localization;
using ImGui.Forms.Models;
using SixLabors.ImageSharp;
using Rectangle = Veldrid.Rectangle;
using Size = ImGui.Forms.Models.Size;

namespace CfgBinEditor.Forms
{
    public abstract partial class BaseTreeViewForm<TConfig, TEntry>
    {
        private TextBox _searchTextBox;
        private Button _searchSettingsButton;
        private ImageButton _clearSearchButton;

        private Button _addRootButton;
        private Button _importRootButton;
        private Button _exportRootButton;

        private StackLayout _searchLayout;
        private StackLayout _mainLayout;

        private TreeView<TEntry> _fullTreeView;
        private TreeView<TreeNode<TEntry>> _filteredTreeView;

        private ContextMenu _treeViewContextMenu;
        private MenuBarButton _addButton;
        private MenuBarButton _duplicateButton;
        private MenuBarButton _removeButton;
        private MenuBarButton _importButton;
        private MenuBarButton _exportButton;

        private ContextMenu _filteredTreeViewContextMenu;
        private MenuBarButton _filteredAddButton;
        private MenuBarButton _filteredDuplicateButton;
        private MenuBarButton _filteredRemoveButton;
        private MenuBarButton _filteredImportButton;
        private MenuBarButton _filteredExportButton;

        private MenuBarSplitter _splitter;

        private void InitializeComponent(TConfig config)
        {
            _searchTextBox = new TextBox { Placeholder = LocalizationResources.CfgBinEntrySearchPlaceholderCaption };
            _searchSettingsButton = new Button { Text = "...", Padding = new Vector2(2, 3) };
            _clearSearchButton = new ImageButton { Image = ImageResources.Close };

            _addRootButton = new Button { Text = GetRootButtonCaption(), Width = SizeValue.Parent };
            _importRootButton = new Button { Text = LocalizationResources.CfgBinEntryImportCaption, Width = SizeValue.Parent };
            _exportRootButton = new Button { Text = LocalizationResources.CfgBinEntryExportCaption, Width = SizeValue.Parent };

            _fullTreeView = new TreeView<TEntry>();
            _filteredTreeView = new TreeView<TreeNode<TEntry>>();

            _splitter = new MenuBarSplitter();

            _addButton = new MenuBarButton { Text = LocalizationResources.CfgBinEntryAddCaption };
            _duplicateButton = new MenuBarButton { Text = LocalizationResources.CfgBinEntryDuplicateCaption };
            _removeButton = new MenuBarButton { Text = LocalizationResources.CfgBinEntryRemoveCaption };
            _importButton = new MenuBarButton { Text = LocalizationResources.CfgBinEntryImportCaption };
            _exportButton = new MenuBarButton { Text = LocalizationResources.CfgBinEntryExportCaption };
            _treeViewContextMenu = new ContextMenu
            {
                Items =
                {
                    _addButton,
                    _duplicateButton,
                    _removeButton,
                    _splitter,
                    _importButton,
                    _exportButton
                }
            };

            _filteredAddButton = new MenuBarButton { Text = LocalizationResources.CfgBinEntryAddCaption };
            _filteredDuplicateButton = new MenuBarButton { Text = LocalizationResources.CfgBinEntryDuplicateCaption };
            _filteredRemoveButton = new MenuBarButton { Text = LocalizationResources.CfgBinEntryRemoveCaption };
            _filteredImportButton = new MenuBarButton { Text = LocalizationResources.CfgBinEntryImportCaption };
            _filteredExportButton = new MenuBarButton { Text = LocalizationResources.CfgBinEntryExportCaption };
            _filteredTreeViewContextMenu = new ContextMenu
            {
                Items =
                {
                    _filteredAddButton,
                    _filteredDuplicateButton,
                    _filteredRemoveButton,
                    _filteredImportButton,
                    _filteredExportButton
                }
            };

            _fullTreeView.ContextMenu = _treeViewContextMenu;
            _filteredTreeView.ContextMenu = _filteredTreeViewContextMenu;

            _searchLayout = new StackLayout
            {
                Alignment = Alignment.Horizontal,
                ItemSpacing = 5,
                Size = Size.WidthAlign
            };

            _searchLayout.Items.Add(new StackItem(_searchTextBox) { VerticalAlignment = VerticalAlignment.Center });
            if (HasSearchSettings())
                _searchLayout.Items.Add(_searchSettingsButton);
            _searchLayout.Items.Add(_clearSearchButton);

            _mainLayout = new StackLayout
            {
                Alignment = Alignment.Vertical,
                ItemSpacing = 5,
                Items =
                {
                    _searchLayout,
                    _addRootButton,
                    new StackLayout
                    {
                        Alignment = Alignment.Horizontal,
                        ItemSpacing = 5,
                        Size = Size.WidthAlign,
                        Items =
                        {
                            _importRootButton,
                            _exportRootButton
                        }
                    },
                    _fullTreeView
                }
            };
        }

        public override Size GetSize()
        {
            return Size.Parent;
        }

        protected override void UpdateInternal(Rectangle contentRect)
        {
            _mainLayout.Update(contentRect);
        }

        protected override void SetTabInactiveCore()
        {
            _mainLayout.SetTabInactive();
        }

        protected virtual bool HasSearchSettings() => false;

        protected abstract LocalizedString GetRootButtonCaption();

        private void ResetNodesChangeState(IEnumerable<TreeNode> nodes)
        {
            foreach (TreeNode node in nodes)
            {
                node.TextColor = ColorResources.TextDefault;

                IEnumerable<TreeNode> childNodes = node.EnumerateNodes();
                ResetNodesChangeState(childNodes);
            }
        }

        protected void PopulateFullTreeView(TConfig config)
        {
            PopulateFullTreeViewInternal(config, _fullTreeView);
        }

        protected abstract void PopulateFullTreeViewInternal(TConfig config, TreeView<TEntry> treeView);

        private IList<TreeNode<TreeNode<TEntry>>> FilterNodes(IList<TreeNode<TEntry>> nodes, string searchText)
        {
            var result = new List<TreeNode<TreeNode<TEntry>>>();

            foreach (TreeNode<TEntry> node in nodes)
            {
                IList<TreeNode<TreeNode<TEntry>>> filteredChildNodes = FilterNodes(node.Nodes, searchText);
                if (!IsEntrySearched(node.Data, searchText) && filteredChildNodes.Count <= 0)
                    continue;

                var filteredNode = new TreeNode<TreeNode<TEntry>>
                {
                    Font = node.Font,
                    Text = node.Text,
                    TextColor = node.TextColor,
                    IsExpanded = node.IsExpanded,
                    Data = node,
                };

                foreach (TreeNode<TreeNode<TEntry>> filteredChildNode in filteredChildNodes)
                    filteredNode.Nodes.Add(filteredChildNode);

                result.Add(filteredNode);
            }

            return result;
        }

        protected abstract bool IsEntrySearched(TEntry entry, string searchText);
    }
}
