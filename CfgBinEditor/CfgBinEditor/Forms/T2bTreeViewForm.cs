using System;
using System.Collections.Generic;
using CfgBinEditor.resources;
using CrossCutting.Core.Contract.EventBrokerage;
using ImGui.Forms.Controls.Tree;
using Logic.Business.CfgBinValueSettingsManagement.Contract;
using Logic.Domain.Level5Management.Contract.DataClasses;

namespace CfgBinEditor.Forms
{
    public partial class T2bTreeViewForm : BaseTreeViewForm<T2b, T2bEntry>
    {
        private readonly T2b _config;
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

        public T2bTreeViewForm(T2b config, string gameName, IEventBroker eventBroker, IValueSettingsProvider settingsProvider) : base(config, eventBroker)
        {
            _config = config;
            _gameName = gameName;

            _settingsProvider = settingsProvider;
        }

        protected override bool CanDuplicate(TreeNode<T2bEntry> node)
        {
            return true;
        }

        protected override void DuplicateNode(TreeNode<T2bEntry> node)
        {
            int duplicatedEntryIndex = DuplicateEntry(node.Data);

            // Add duplicated entries to tree
            TreeNode<T2bEntry> newNode = CreateNode(_config, ref duplicatedEntryIndex, ColorResources.TextSuccessful);
            _entryNodeLookup[newNode.Data] = newNode;

            IList<TreeNode<T2bEntry>> nodes = GetNeighbourNodes(node);
            nodes.Add(newNode);

            AdjustNodeNames(nodes);

            UpdateTreeView();
            RaiseTreeChanged();
        }

        protected override bool CanRemove(TreeNode<T2bEntry> node)
        {
            return true;
        }

        protected override void RemoveNode(TreeNode<T2bEntry> node)
        {
            RemoveEntry(node.Data);

            // Remove entries from tree
            IList<TreeNode<T2bEntry>> nodes = GetNeighbourNodes(node);
            nodes.Remove(node);

            AdjustNodeNames(nodes);

            UpdateTreeView();
            RaiseTreeChanged();
        }

        private int DuplicateEntry(T2bEntry entry)
        {
            // Re-allocate new entry array
            TreeNode<T2bEntry> node = _entryNodeLookup[entry];
            TreeNode<T2bEntry> lastNode = GetNeighbourNodes(node)[^1];

            int entryIndex = Array.IndexOf(_config.Entries, entry);
            int entryCount = CountEntries(node);

            int lastEntryIndex = lastNode == node ? entryIndex : Array.IndexOf(_config.Entries, lastNode.Data);
            int lastEntryCount = lastNode == node ? entryCount : CountEntries(lastNode);

            int newEntryIndex = lastEntryIndex + lastEntryCount;

            var newEntries = new T2bEntry[_config.Entries.Length + entryCount];
            Array.Copy(_config.Entries, newEntries, newEntryIndex);
            Array.Copy(_config.Entries, newEntryIndex, newEntries, newEntryIndex + entryCount, _config.Entries.Length - newEntryIndex);

            // Create new entries
            for (var i = 0; i < entryCount; i++)
            {
                newEntries[newEntryIndex + i] = new T2bEntry
                {
                    Name = _config.Entries[entryIndex + i].Name,
                    Values = new T2bEntryValue[_config.Entries[entryIndex + i].Values.Length]
                };

                for (var j = 0; j < newEntries[newEntryIndex + i].Values.Length; j++)
                {
                    newEntries[newEntryIndex + i].Values[j] = new T2bEntryValue
                    {
                        Type = _config.Entries[entryIndex + i].Values[j].Type,
                        Value = _config.Entries[entryIndex + i].Values[j].Value
                    };
                }
            }

            // Update entry array
            _config.Entries = newEntries;

            return newEntryIndex;
        }

        private void RemoveEntry(T2bEntry entry)
        {
            // Re-allocate new entry array
            TreeNode<T2bEntry> node = _entryNodeLookup[entry];
            TreeNode<T2bEntry> lastNode = GetNeighbourNodes(node)[^1];

            int entryIndex = Array.IndexOf(_config.Entries, entry);
            int entryCount = CountEntries(node);

            var newEntries = new T2bEntry[_config.Entries.Length - entryCount];
            Array.Copy(_config.Entries, newEntries, entryIndex);
            Array.Copy(_config.Entries, entryIndex + entryCount, newEntries, entryIndex, newEntries.Length - entryIndex);

            // Update entry array
            _config.Entries = newEntries;
        }

        private IList<TreeNode<T2bEntry>> GetNeighbourNodes(TreeNode<T2bEntry> node)
        {
            return node.Parent?.Nodes ?? GetRootNodes();
        }

        private int CountEntries(TreeNode<T2bEntry> node)
        {
            if (node.Nodes.Count <= 0)
                return 1;

            var count = 1;
            foreach (TreeNode<T2bEntry> child in node.Nodes)
                count += CountEntries(child);

            return count + 1;
        }
    }
}
