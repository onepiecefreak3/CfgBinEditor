using System;
using System.Collections.Generic;
using System.IO;
using CfgBinEditor.InternalContract.DataClasses;
using CfgBinEditor.Messages;
using CfgBinEditor.resources;
using CrossCutting.Core.Contract.EventBrokerage;
using CrossCutting.Core.Contract.Serialization;
using CrossCutting.Core.Contract.Settings;
using ImGui.Forms.Controls.Tree;
using ImGui.Forms.Localization;
using ImGui.Forms.Modals;
using ImGui.Forms.Modals.IO;
using ImGui.Forms.Modals.IO.Windows;
using Logic.Business.CfgBinEditorManagement.Contract;
using Logic.Domain.Level5Management.Contract.DataClasses;
using Microsoft.VisualBasic.FileIO;
using ValueType = Logic.Domain.Level5Management.Contract.DataClasses.ValueType;

namespace CfgBinEditor.Forms
{
    public partial class T2bTreeViewForm : BaseTreeViewForm<T2b, T2bNode>
    {
        private readonly T2b _config;
        private readonly IEventBroker _events;
        private readonly ISettingsProvider _settingsProvider;
        private readonly ISerializer _serializer;
        private readonly IValueSettingsProvider _valueSettingsProvider;
        private readonly IEntryNamesProvider _namesProvider;

        private LocalizedString _gameName;

        public LocalizedString GameName
        {
            get => _gameName;
            set
            {
                _gameName = value;

                AdjustNodeNames(GetRootNodes());

                UpdateTreeView();
            }
        }

        public T2bTreeViewForm(T2b config, IEventBroker eventBroker, ISettingsProvider settingsProvider, ISerializer serializer,
            IValueSettingsProvider valueSettingsProvider, IEntryNamesProvider namesProvider) : base(eventBroker)
        {
            _config = config;
            _events = eventBroker;
            _gameName = LocalizationResources.GameNoneCaption;

            _settingsProvider = settingsProvider;
            _serializer = serializer;
            _valueSettingsProvider = valueSettingsProvider;
            _namesProvider = namesProvider;

            PopulateFullTreeView(config);
        }

        protected override bool HasSearchSettings()
        {
            return true;
        }

        protected override async void ShowSearchSettings()
        {
            // Show comparison type dialog
            LocalizedString[] items = new[]
            {
                LocalizationResources.CfgBinEntrySearchHex,
                LocalizationResources.CfgBinEntrySearchDec,
                LocalizationResources.CfgBinEntrySearchTags
            };
            SearchComparisonType comparisonType = GetComparisonType();

            LocalizedString? comparison = await ComboInputBox.ShowAsync(
                LocalizationResources.CfgBinEntrySearchDialogCaption,
                LocalizationResources.CfgBinEntrySearchComparisonCaption,
                items,
                items[(int)comparisonType]);
            if (!comparison.HasValue)
                return;

            // Set new comparison type
            var newComparisonType = (SearchComparisonType)Array.IndexOf(items, comparison);
            SetComparisonType(newComparisonType);

            // Re-filter tree view after search settings changed
            if (comparisonType != newComparisonType)
                UpdateTreeView();
        }

        protected override bool CanAdd(TreeNode<T2bNode>? parentNode)
        {
            return true;
        }

        protected override async void AddNode(TreeNode<T2bNode>? parentNode)
        {
            // Set new entry name
            string? entryName = await InputBox.ShowAsync(LocalizationResources.CfgBinEntryAddRootCaption, LocalizationResources.CfgBinEntryAddDialogCaption);
            if (string.IsNullOrEmpty(entryName))
            {
                RaiseErrorStatus(LocalizationResources.CfgBinEntryAddErrorCaption);
                return;
            }

            int newEntryIndex = AllocateEntries(parentNode, 1);

            _config.Entries[newEntryIndex] = new T2bEntry
            {
                Name = entryName,
                Values = []
            };

            // Add nodes
            AddNodesToTree(parentNode, newEntryIndex, 1);

            UpdateTreeView();
            RaiseTreeChanged();

            RaiseSuccessStatus();
        }

        private void AddNodesToTree(TreeNode<T2bNode>? parentNode, int index, int count)
        {
            int endIndex = index + count;
            while (index < endIndex)
            {
                TreeNode<T2bNode> newNode = CreateNode(_config, ref index, ColorResources.TextSuccessful);
                _entryNodeLookup[newNode.Data.Entry] = newNode;

                index++;

                IList<TreeNode<T2bNode>> nodes;
                if (parentNode == null)
                {
                    nodes = GetRootNodes();
                    nodes.Add(newNode);
                }
                else
                {
                    bool isNested = IsNestedNode(parentNode.Data.Entry.Name);
                    nodes = isNested ? parentNode.Nodes : GetNeighbourNodes(parentNode);

                    if (isNested)
                    {
                        nodes.Add(newNode);
                        parentNode.IsExpanded = true;
                    }
                    else
                        nodes.Insert(nodes.IndexOf(parentNode) + 1, newNode);
                }

                AdjustNodeNames(nodes);
            }
        }

        protected override bool CanDuplicate(TreeNode<T2bNode> node)
        {
            return true;
        }

        protected override void DuplicateNode(TreeNode<T2bNode> node)
        {
            int duplicatedEntryIndex = DuplicateEntry(node.Data);

            // Add duplicated entries to tree
            TreeNode<T2bNode> newNode = CreateNode(_config, ref duplicatedEntryIndex, ColorResources.TextSuccessful);
            _entryNodeLookup[newNode.Data.Entry] = newNode;

            IList<TreeNode<T2bNode>> nodes = GetNeighbourNodes(node);
            nodes.Add(newNode);

            AdjustNodeNames(nodes);

            UpdateTreeView();
            RaiseTreeChanged();
        }

        protected override bool CanRemove(TreeNode<T2bNode> node)
        {
            return true;
        }

        protected override void RemoveNode(TreeNode<T2bNode> node)
        {
            RemoveEntry(node.Data.Entry);

            // Remove entries from tree
            IList<TreeNode<T2bNode>> nodes = GetNeighbourNodes(node);
            nodes.Remove(node);

            AdjustNodeNames(nodes);

            UpdateTreeView();
            RaiseTreeChanged();
        }

        protected override bool CanImport(TreeNode<T2bNode>? parentNode)
        {
            return true;
        }

        protected override async void Import(TreeNode<T2bNode>? parentNode)
        {
            // Select json file
            var ofd = new WindowsOpenFileDialog
            {
                InitialDirectory = GetImportDirectory(),
                Filters =
                {
                    new FileFilter(LocalizationResources.FileOpenJsonFilterCaption, "json")
                }
            };

            DialogResult result = await ofd.ShowAsync();
            if (result != DialogResult.Ok)
                return;

            // Read entries from file
            await using Stream importStream = File.OpenRead(ofd.Files[0]);
            using StreamReader reader = new StreamReader(importStream);

            string importJson = await reader.ReadToEndAsync();
            T2bEntry[] entries = _serializer.Deserialize<T2bEntry[]>(importJson);

            // Add entries to config
            int newEntryIndex = AllocateEntries(parentNode, entries.Length);
            for (var i = 0; i < entries.Length; i++)
            {
                foreach (T2bEntryValue value in entries[i].Values)
                {
                    switch (_config.ValueLength)
                    {
                        case ValueLength.Int:
                            switch (value.Type)
                            {
                                case ValueType.Integer:
                                    value.Value = (int)(long)value.Value;
                                    break;

                                case ValueType.FloatingPoint:
                                    value.Value = (float)(double)value.Value;
                                    break;

                                case ValueType.String:
                                    break;

                                default:
                                    throw new InvalidOperationException($"Unknown value type {value.Type}.");
                            }
                            break;

                        case ValueLength.Long:
                            switch (value.Type)
                            {
                                case ValueType.Integer:
                                    value.Value = (long)value.Value;
                                    break;

                                case ValueType.FloatingPoint:
                                    value.Value = (double)value.Value;
                                    break;

                                case ValueType.String:
                                    break;

                                default:
                                    throw new InvalidOperationException($"Unknown value type {value.Type}.");
                            }
                            break;

                        default:
                            throw new InvalidOperationException($"Unknown value length {_config.ValueLength}.");
                    }
                }

                _config.Entries[newEntryIndex + i] = entries[i];
            }

            // Add nodes
            AddNodesToTree(parentNode, newEntryIndex, entries.Length);

            UpdateTreeView();
            RaiseTreeChanged();

            RaiseSuccessStatus();
        }

        protected override bool CanExport(TreeNode<T2bNode>? node)
        {
            return true;
        }

        protected override async void Export(TreeNode<T2bNode>? node)
        {
            // Select save path
            var sfd = new WindowsSaveFileDialog
            {
                InitialDirectory = GetExportDirectory(),
                Filters =
                {
                    new FileFilter(LocalizationResources.FileOpenJsonFilterCaption, "json")
                }
            };

            DialogResult result = await sfd.ShowAsync();
            if (result != DialogResult.Ok)
                return;

            SetExportDirectory(Path.GetDirectoryName(sfd.Files[0])!);

            // Choose entries to export
            int entryIndex = node == null ? 0 : Array.IndexOf(_config.Entries, node.Data);
            int entryCount = node == null ? _config.Entries.Length : CountEntries(node);

            string entryJson = _serializer.Serialize(_config.Entries[entryIndex..(entryIndex + entryCount)]);

            // Write entries to file
            await using Stream exportStream = File.Create(sfd.Files[0]);
            await using StreamWriter writer = new StreamWriter(exportStream);

            await writer.WriteAsync(entryJson);
        }

        private int AllocateEntries(TreeNode<T2bNode>? node, int count)
        {
            // Allocate new entries
            int newEntryIndex = node is null ? _config.Entries.Length :
                node.Nodes.Count <= 0 ? Array.IndexOf(_config.Entries, node.Data.Entry) + 1 :
                node.Data.EndEntry is null ? Array.IndexOf(_config.Entries, node.Data.Entry) + 1 : Array.IndexOf(_config.Entries, node.Data.EndEntry);

            var newEntries = new T2bEntry[_config.Entries.Length + count];
            Array.Copy(_config.Entries, newEntries, newEntryIndex);
            Array.Copy(_config.Entries, newEntryIndex, newEntries, newEntryIndex + count, _config.Entries.Length - newEntryIndex);

            // Update entry array
            _config.Entries = newEntries;

            return newEntryIndex;
        }

        private int DuplicateEntry(T2bNode node)
        {
            // Allocate new entries
            int entryIndex = Array.IndexOf(_config.Entries, node.Entry);
            int newEntryIndex = node.EndEntry is null ? _config.Entries.Length : Array.IndexOf(_config.Entries, node.EndEntry) + 1;

            int entryCount = newEntryIndex - entryIndex;

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
            TreeNode<T2bNode> node = _entryNodeLookup[entry];

            // Allocate new entries
            int entryIndex = Array.IndexOf(_config.Entries, entry);
            int newEntryIndex = node.Data.EndEntry is null ? _config.Entries.Length : Array.IndexOf(_config.Entries, node.Data.EndEntry) + 1;

            int entryCount = newEntryIndex - entryIndex;

            var newEntries = new T2bEntry[_config.Entries.Length - entryCount];
            Array.Copy(_config.Entries, newEntries, entryIndex);
            Array.Copy(_config.Entries, newEntryIndex, newEntries, entryIndex, _config.Entries.Length - newEntryIndex);

            // Update entry array
            _config.Entries = newEntries;
        }

        private IList<TreeNode<T2bNode>> GetNeighbourNodes(TreeNode<T2bNode>? node)
        {
            return node?.Parent?.Nodes ?? GetRootNodes();
        }

        private int CountEntries(TreeNode<T2bNode> node)
        {
            if (node.Nodes.Count <= 0)
                return 1;

            var count = 1;
            foreach (TreeNode<T2bNode> child in node.Nodes)
                count += CountEntries(child);

            return count + 1;
        }

        private SearchComparisonType GetComparisonType()
        {
            return _settingsProvider.Get("CfgBinEditor.Settings.Search.ValueFormat", SearchComparisonType.Tags);
        }

        private void SetComparisonType(SearchComparisonType comparison)
        {
            _settingsProvider.Set("CfgBinEditor.Settings.Search.ValueFormat", comparison);
        }

        private string GetExportDirectory()
        {
            return _settingsProvider.Get("CfgBinEditor.Settings.ExportDirectory", SpecialDirectories.Desktop);
        }

        private void SetExportDirectory(string directoryPath)
        {
            _settingsProvider.Set("CfgBinEditor.Settings.ExportDirectory", directoryPath);
        }

        private string GetImportDirectory()
        {
            return _settingsProvider.Get("CfgBinEditor.Settings.ImportDirectory", SpecialDirectories.Desktop);
        }

        private void SetImportDirectory(string directoryPath)
        {
            _settingsProvider.Set("CfgBinEditor.Settings.ImportDirectory", directoryPath);
        }

        private void RaiseErrorStatus(LocalizedString text)
        {
            _events.Raise(new UpdateStatusMessage(text, LabelStatus.Error));
        }

        private void RaiseSuccessStatus()
        {
            _events.Raise(new UpdateStatusMessage(string.Empty, LabelStatus.None));
        }
    }

    public record T2bNode(T2bEntry Entry, T2bEntry? EndEntry);
}
