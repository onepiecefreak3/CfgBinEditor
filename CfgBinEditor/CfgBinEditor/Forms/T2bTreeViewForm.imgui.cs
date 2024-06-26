﻿using ImGui.Forms;
using ImGui.Forms.Controls.Tree;
using Logic.Business.CfgBinEditorManagement.Contract.DataClasses;
using Logic.Domain.Level5Management.Contract.DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using CfgBinEditor.InternalContract.DataClasses;
using ImGui.Forms.Localization;
using ValueType = Logic.Domain.Level5Management.Contract.DataClasses.ValueType;
using CfgBinEditor.resources;

namespace CfgBinEditor.Forms
{
    public partial class T2bTreeViewForm
    {
        private readonly IDictionary<T2bEntry, TreeNode<T2bEntry>> _entryNodeLookup =
            new Dictionary<T2bEntry, TreeNode<T2bEntry>>();

        #region Tree population

        protected override LocalizedString GetRootButtonCaption()
        {
            return LocalizationResources.CfgBinEntryAddRootCaption;
        }

        protected override void PopulateFullTreeViewInternal(T2b config, TreeView<T2bEntry> treeView)
        {
            var root = new TreeNode<T2bEntry>();

            _entryNodeLookup.Clear();
            PopulateNode(root, config, 0);

            treeView.Nodes.Clear();
            foreach (TreeNode<T2bEntry> node in root.Nodes)
                treeView.Nodes.Add(node);

            AdjustNodeNames(root.Nodes);
        }

        private int PopulateNode(TreeNode<T2bEntry> rootNode, T2b config, int index, string endNodeName = null, ThemedColor textColor = default)
        {
            for (; index < config.Entries.Length; index++)
            {
                if (config.Entries[index].Name == endNodeName)
                    break;

                TreeNode<T2bEntry> node = CreateNode(config, ref index, textColor);
                _entryNodeLookup[node.Data] = node;

                rootNode.Nodes.Add(node);
            }

            return index;
        }

        private void AdjustNodeNames(IList<TreeNode<T2bEntry>> nodes)
        {
            foreach (IGrouping<string, TreeNode<T2bEntry>> group in nodes.GroupBy(x => GetNodeName(x.Data), x => x))
            {
                TreeNode<T2bEntry>[] sameNameNodes = group.ToArray();
                if (sameNameNodes.Length <= 0)
                    continue;

                for (var i = 0; i < sameNameNodes.Length; i++)
                {
                    sameNameNodes[i].Text = group.Key;
                    if (sameNameNodes.Length > 1)
                        sameNameNodes[i].Text += $"_{i}";

                    AdjustNodeNames(sameNameNodes[i].Nodes);
                }
            }
        }

        private TreeNode<T2bEntry> CreateNode(T2b config, ref int index, ThemedColor textColor)
        {
            T2bEntry entry = config.Entries[index];

            var entryNode = new TreeNode<T2bEntry> { Data = entry, Text = entry.Name, TextColor = textColor };

            int beginIndex = GetBeginIndex(entry.Name);
            if (beginIndex < 0)
                return entryNode;

            entryNode.Text = beginIndex == 0 ? entry.Name : entry.Name[..beginIndex];
            string beginPart = beginIndex == 0 ? string.Empty : entry.Name[beginIndex..];

            index = PopulateNode(entryNode, config, index + 1, GetEndName(entryNode.Text, beginPart), textColor);

            return entryNode;
        }

        private string GetNodeName(T2bEntry entry)
        {
            int beginIndex = GetBeginIndex(entry.Name);
            if (beginIndex < 0)
            {
                if (_gameName == LocalizationResources.GameNoneCaption)
                    return entry.Name;

                if (entry.Values.Length <= 0 || entry.Values[0].Type != ValueType.Integer)
                    return entry.Name;

                if (_namesProvider.TryGetName(_gameName, entry, _config.ValueLength, out string? name))
                    return name!;

                return entry.Name;
            }

            return beginIndex == 0 ? entry.Name : entry.Name[..beginIndex];
        }

        private bool IsNestedNode(string name)
        {
            return GetBeginIndex(name) >= 0;
        }

        private int GetBeginIndex(string name)
        {
            if (name == "PTREE")
                return 0;

            int beginIndex = name.LastIndexOf("_BEGIN", StringComparison.Ordinal);
            int begIndex = name.LastIndexOf("_BEG", StringComparison.Ordinal);
            int bgnIndex = name.LastIndexOf("_BGN", StringComparison.Ordinal);
            int underScoreIndex = name.EndsWith("_", StringComparison.Ordinal) ? name.Length - 1 : -1;

            return beginIndex < 0 ? begIndex < 0 ? bgnIndex < 0 ? underScoreIndex : bgnIndex : begIndex : beginIndex;
        }

        private string GetEndName(string beginName, string beginPart)
        {
            if (beginName == "PTREE" || beginPart == "_")
                return "_" + beginName;

            return beginName + "_END";
        }

        #endregion

        protected override bool IsEntrySearched(T2bEntry entry, string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
                return true;

            for (var i = 0; i < entry.Values.Length; i++)
            {
                bool isHex;

                SearchComparisonType comparisonType = GetComparisonType();
                switch (comparisonType)
                {
                    case SearchComparisonType.Hex:
                        isHex = true;
                        break;

                    case SearchComparisonType.Dec:
                        isHex = false;
                        break;

                    case SearchComparisonType.Tags:
                        ValueSettingEntry settingEntry = _valueSettingsProvider.GetEntrySettings(GameName, entry.Name, i);
                        isHex = settingEntry.IsHex;
                        break;

                    default:
                        throw new InvalidOperationException($"Unknown value comparison type {comparisonType}.");
                }

                string valueString = GetValueString(entry.Values[i].Value, entry.Values[i].Type, isHex);

                if (valueString.Contains(searchText))
                    return true;
            }

            return false;
        }

        private string GetValueString(object value, ValueType type, bool isHex)
        {
            switch (type)
            {
                case ValueType.String:
                    return $"{value}";

                case ValueType.Integer:
                    if (!isHex)
                        return $"{value}";

                    switch (_config.ValueLength)
                    {
                        case ValueLength.Int:
                            return $"0x{value:X8}";

                        case ValueLength.Long:
                            return $"0x{value:X16}";

                        default:
                            throw new InvalidOperationException($"Unknown value length {_config.ValueLength}.");
                    }

                case ValueType.FloatingPoint:
                    if (!isHex)
                        return $"{value}";

                    switch (_config.ValueLength)
                    {
                        case ValueLength.Int:
                            int iValue = BitConverter.SingleToInt32Bits((float)value);
                            return $"0x{iValue:X8}";

                        case ValueLength.Long:
                            long lValue = BitConverter.DoubleToInt64Bits((double)value);
                            return $"0x{lValue:X16}";

                        default:
                            throw new InvalidOperationException($"Unknown value length {_config.ValueLength}.");
                    }

                default:
                    throw new InvalidOperationException($"Unknown value type {type}.");
            }
        }
    }
}
