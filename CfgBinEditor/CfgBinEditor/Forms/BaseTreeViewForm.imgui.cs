using CfgBinEditor.resources;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Controls.Menu;
using ImGui.Forms.Controls.Tree;
using ImGui.Forms.Controls;
using System.Collections.Generic;
using System.Drawing;
using Rectangle = Veldrid.Rectangle;
using Size = ImGui.Forms.Models.Size;

namespace CfgBinEditor.Forms
{
    public abstract partial class BaseTreeViewForm<TConfig, TEntry>
    {
        private TextBox _searchTextBox;
        private ImageButton _clearSearchButton;

        private StackLayout _searchLayout;
        private StackLayout _mainLayout;

        private TreeView<TEntry> _fullTreeView;
        private TreeView<TreeNode<TEntry>> _filteredTreeView;

        private ContextMenu _treeViewContextMenu;
        private MenuBarButton _duplicateButton;
        private MenuBarButton _removeButton;

        private ContextMenu _filteredTreeViewContextMenu;
        private MenuBarButton _filteredDuplicateButton;
        private MenuBarButton _filteredRemoveButton;

        private void InitializeComponent(TConfig config)
        {
            _searchTextBox = new TextBox { Placeholder = LocalizationResources.CfgBinEntrySearchPlaceholderCaption };
            _clearSearchButton = new ImageButton { Image = ImageResources.Close };

            _fullTreeView = new TreeView<TEntry>();
            _filteredTreeView = new TreeView<TreeNode<TEntry>>();

            _duplicateButton = new MenuBarButton { Text = LocalizationResources.CfgBinEntryDuplicateCaption };
            _removeButton = new MenuBarButton { Text = LocalizationResources.CfgBinEntryRemoveCaption };
            _treeViewContextMenu = new ContextMenu
            {
                Items =
                {
                    _duplicateButton,
                    _removeButton
                }
            };

            _filteredDuplicateButton = new MenuBarButton { Text = LocalizationResources.CfgBinEntryDuplicateCaption };
            _filteredRemoveButton = new MenuBarButton { Text = LocalizationResources.CfgBinEntryRemoveCaption };
            _filteredTreeViewContextMenu = new ContextMenu
            {
                Items =
                {
                    _filteredDuplicateButton,
                    _filteredRemoveButton
                }
            };

            _fullTreeView.ContextMenu = _treeViewContextMenu;
            _filteredTreeView.ContextMenu = _filteredTreeViewContextMenu;

            _searchLayout = new StackLayout
            {
                Alignment = Alignment.Horizontal,
                ItemSpacing = 5,
                Size = Size.WidthAlign,
                Items =
                {
                    new StackItem(_searchTextBox){VerticalAlignment = VerticalAlignment.Center},
                    _clearSearchButton
                }
            };

            _mainLayout = new StackLayout
            {
                Alignment = Alignment.Vertical,
                ItemSpacing = 5,
                Items =
                {
                    _searchLayout,
                    _fullTreeView
                }
            };

            PopulateFullTreeView(config, _fullTreeView);
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

        private void ResetNodesChangeState(IEnumerable<TreeNode> nodes)
        {
            foreach (TreeNode node in nodes)
            {
                node.TextColor = Color.Empty;

                IEnumerable<TreeNode> childNodes = node.EnumerateNodes();
                ResetNodesChangeState(childNodes);
            }
        }

        protected abstract void PopulateFullTreeView(TConfig config, TreeView<TEntry> treeView);

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
