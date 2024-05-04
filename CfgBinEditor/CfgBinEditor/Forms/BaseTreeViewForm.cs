using System;
using ImGui.Forms.Controls.Base;
using ImGui.Forms.Models;
using CrossCutting.Core.Contract.EventBrokerage;
using Veldrid;
using ImGui.Forms.Controls.Tree;
using System.Collections.Generic;
using CfgBinEditor.Messages;

namespace CfgBinEditor.Forms
{
    public abstract partial class BaseTreeViewForm<TConfig, TEntry> : Component
    {
        private readonly IEventBroker _eventBroker;

        public TEntry? SelectedEntry { get; private set; }

        public BaseTreeViewForm(TConfig config, IEventBroker eventBroker)
        {
            InitializeComponent(config);

            _eventBroker = eventBroker;

            _fullTreeView.SelectedNodeChanged += (s, e) => ChangeEntry(_fullTreeView.SelectedNode.Data);
            _filteredTreeView.SelectedNodeChanged += (s, e) => ChangeEntry(_filteredTreeView.SelectedNode.Data.Data);

            _duplicateButton.Clicked += (s, e) => DuplicateNode(_fullTreeView.SelectedNode);
            _filteredDuplicateButton.Clicked += (s, e) => DuplicateNode(_filteredTreeView.SelectedNode.Data);

            _searchTextBox.TextChanged += (s, e) => UpdateTreeView();
            _clearSearchButton.Clicked += (s, e) => ClearSearch();

            if (_fullTreeView.Nodes.Count > 0)
                _fullTreeView.SelectedNode = _fullTreeView.Nodes[0];
        }

        public void ResetNodeState()
        {
            ResetNodesChangeState(_fullTreeView.Nodes);
            ResetNodesChangeState(_filteredTreeView.Nodes);
        }

        public override Size GetSize()
        {
            return Size.Parent;
        }

        protected override void UpdateInternal(Rectangle contentRect)
        {
            _mainLayout.Update(contentRect);
        }

        private void ChangeEntry(TEntry entry)
        {
            SelectedEntry = entry;
            RaiseTreeEntryChanged(entry);
        }

        protected abstract void DuplicateNode(TreeNode<TEntry> node);

        protected void UpdateTreeView()
        {
            if (string.IsNullOrEmpty(_searchTextBox.Text))
            {
                _mainLayout.Items[1] = _fullTreeView;
                return;
            }

            FilterTreeView();

            _mainLayout.Items[1] = _filteredTreeView;
        }

        private void FilterTreeView()
        {
            IList<TreeNode<TreeNode<TEntry>>> filteredNodes = FilterNodes(_fullTreeView.Nodes, _searchTextBox.Text);

            _filteredTreeView.Nodes.Clear();
            foreach (TreeNode<TreeNode<TEntry>> node in filteredNodes)
                _filteredTreeView.Nodes.Add(node);
        }

        private void ClearSearch()
        {
            _searchTextBox.Text = string.Empty;
        }

        protected IList<TreeNode<TEntry>> GetRootNodes()
        {
            return _fullTreeView.Nodes;
        }

        protected void RaiseTreeEntryChanged(TEntry entry)
        {
            _eventBroker.Raise(new TreeEntryChangedMessage<TConfig, TEntry>(this, entry));
        }

        protected void RaiseTreeChanged()
        {
            _eventBroker.Raise(new TreeChangedMessage<TConfig, TEntry>(this));
        }
    }
}
