using System;
using ImGui.Forms.Controls.Base;
using ImGui.Forms.Models;
using Logic.Domain.Level5.Contract.DataClasses;
using CfgBinEditor.Events;
using CrossCutting.Core.Contract.EventBrokerage;
using Veldrid;
using ImGui.Forms.Controls.Tree;
using System.Collections.Generic;
using Logic.Business.CfgBinValueSettingsManagement.Contract;
using CfgBinEditor.Messages;
using CfgBinEditor.resources;

namespace CfgBinEditor.Forms
{
    public partial class ConfigurationTreeViewForm : Component
    {
        private readonly Configuration _config;
        private readonly IEventBroker _eventBroker;
        private readonly IValueSettingsProvider _settingsProvider;

        private string _gameName;

        public string GameName
        {
            get => _gameName;
            set
            {
                _gameName = value;
                UpdateTreeView();
            }
        }
        public ConfigurationEntry SelectedEntry { get; private set; }

        public event EventHandler<ConfigurationEntryChangedEventArgs> EntryChanged;

        public ConfigurationTreeViewForm(Configuration config, string gameName, IEventBroker eventBroker, IValueSettingsProvider settingsProvider)
        {
            InitializeComponent(config);

            _config = config;
            _gameName = gameName;
            _eventBroker = eventBroker;
            _settingsProvider = settingsProvider;

            _fullTreeView.SelectedNodeChanged += (s, e) => ChangeEntry(_fullTreeView.SelectedNode.Data);
            _filteredTreeView.SelectedNodeChanged += (s, e) => ChangeEntry(_filteredTreeView.SelectedNode.Data);

            _duplicateButton.Clicked += (s, e) => DuplicateNode(_fullTreeView.SelectedNode);
            _filteredDuplicateButton.Clicked += (s, e) => DuplicateNode(_entryNodeLookup[_filteredTreeView.SelectedNode.Data]);

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

        private void ChangeEntry(ConfigurationEntry entry)
        {
            SelectedEntry = entry;
            OnEntryChanged(entry);
        }

        private void DuplicateNode(TreeNode<ConfigurationEntry> node)
        {
            int duplicatedEntryIndex = DuplicateEntry(node.Data);

            TreeNode<ConfigurationEntry> newNode = CreateNode(_config, ref duplicatedEntryIndex, ColorResources.TextSuccessful);
            _entryNodeLookup[newNode.Data] = newNode;

            IList<TreeNode<ConfigurationEntry>> nodes = GetNeighbourNodes(node);
            nodes.Add(newNode);

            AdjustNodeNames(nodes);

            UpdateTreeView();

            RaiseConfigurationTreeChanged();
        }

        private int DuplicateEntry(ConfigurationEntry entry)
        {
            TreeNode<ConfigurationEntry> node = _entryNodeLookup[entry];
            TreeNode<ConfigurationEntry> lastNode = GetNeighbourNodes(node)[^1];

            int entryIndex = Array.IndexOf(_config.Entries, entry);
            int entryCount = CountEntries(node);

            int lastEntryIndex = lastNode == node ? entryIndex : Array.IndexOf(_config.Entries, lastNode.Data);
            int lastEntryCount = lastNode == node ? entryCount : CountEntries(lastNode);

            int newEntryIndex = lastEntryIndex + lastEntryCount;

            var newEntries = new ConfigurationEntry[_config.Entries.Length + entryCount];
            Array.Copy(_config.Entries, newEntries, newEntryIndex);
            Array.Copy(_config.Entries, newEntryIndex, newEntries, newEntryIndex + entryCount, _config.Entries.Length - newEntryIndex);

            for (var i = 0; i < entryCount; i++)
            {
                newEntries[newEntryIndex + i] = new ConfigurationEntry
                {
                    Name = _config.Entries[entryIndex + i].Name,
                    Values = new ConfigurationEntryValue[_config.Entries[entryIndex + i].Values.Length]
                };

                for (var j = 0; j < newEntries[newEntryIndex + i].Values.Length; j++)
                {
                    newEntries[newEntryIndex + i].Values[j] = new ConfigurationEntryValue
                    {
                        Type = _config.Entries[entryIndex + i].Values[j].Type,
                        Value = _config.Entries[entryIndex + i].Values[j].Value
                    };
                }
            }

            _config.Entries = newEntries;

            return newEntryIndex;
        }

        private void UpdateTreeView()
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
            IList<TreeNode<ConfigurationEntry>> filteredNodes = FilterNodes(_fullTreeView.Nodes, _searchTextBox.Text);

            _filteredTreeView.Nodes.Clear();
            foreach (TreeNode<ConfigurationEntry> node in filteredNodes)
                _filteredTreeView.Nodes.Add(node);
        }

        private void ClearSearch()
        {
            _searchTextBox.Text = string.Empty;
        }

        private void OnEntryChanged(ConfigurationEntry entry)
        {
            EntryChanged?.Invoke(this, new ConfigurationEntryChangedEventArgs(entry));
        }

        private void RaiseConfigurationTreeChanged()
        {
            _eventBroker.Raise(new ConfigurationTreeChangedMessage(this));
        }
    }
}
