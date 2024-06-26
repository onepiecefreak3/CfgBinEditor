﻿using CrossCutting.Core.Contract.EventBrokerage;
using ImGui.Forms.Controls.Tree;
using Logic.Domain.Level5Management.Contract.DataClasses;
using System;
using System.Linq;
using System.Threading.Tasks;
using CfgBinEditor.resources;
using ImGui.Forms.Localization;
using ImGui.Forms.Modals.IO;
using CfgBinEditor.InternalContract.DataClasses;
using CfgBinEditor.Messages;

namespace CfgBinEditor.Forms
{
    public partial class RdbnTreeViewForm : BaseTreeViewForm<Rdbn, object>
    {
        private readonly Rdbn _config;
        private readonly IEventBroker _events;

        public RdbnTreeViewForm(Rdbn config, IEventBroker eventBroker) : base(config, eventBroker)
        {
            _config = config;
            _events = eventBroker;

            PopulateFullTreeView(config);
        }

        protected override bool CanAdd(TreeNode<object>? parentNode)
        {
            return parentNode == null || parentNode.Data is RdbnListEntry;
        }

        protected override async void AddNode(TreeNode<object>? parentNode)
        {
            if (parentNode == null)
            {
                // Select type
                RdbnTypeDeclaration? type = await SelectListType();
                if (type == null)
                {
                    RaiseErrorStatus(LocalizationResources.RdbnListAddTypeErrorCaption);
                    return;
                }

                // Set list name
                string listName = await InputBox.ShowAsync(LocalizationResources.RdbnListAddCaption, LocalizationResources.RdbnListAddNameDialogCaption);
                if (string.IsNullOrEmpty(listName))
                {
                    RaiseErrorStatus(LocalizationResources.RdbnListAddNameErrorCaption);
                    return;
                }

                // Add list
                RdbnListEntry[] lists = _config.Lists;
                Array.Resize(ref lists, lists.Length + 1);
                _config.Lists = lists;

                lists[^1] = new RdbnListEntry
                {
                    TypeIndex = Array.IndexOf(_config.Types, type),
                    Name = listName,
                    Values = Array.Empty<object[][]>()
                };

                TreeNode<object> newNode = CreateListNode(_config, lists[^1], ColorResources.TextSuccessful);
                GetRootNodes().Add(newNode);

                UpdateTreeView();
                RaiseTreeChanged();

                RaiseSuccessStatus();
                return;
            }

            switch (parentNode.Data)
            {
                case RdbnListEntry list:
                    RdbnTypeDeclaration type = _config.Types[list.TypeIndex];

                    object[][][] values = list.Values;
                    Array.Resize(ref values, values.Length + 1);
                    list.Values = values;

                    list.Values[^1] = new object[type.Fields.Length][];
                    for (var i = 0; i < type.Fields.Length; i++)
                    {
                        list.Values[^1][i] = new object[type.Fields[i].Count];
                        for (var j = 0; j < type.Fields[i].Count; j++)
                            list.Values[^1][i][j] = GetDefaultValue(type.Fields[i]);
                    }

                    TreeNode<object> newNode = CreateValueNode(type, list.Values[^1], parentNode.Nodes.Count + 1, ColorResources.TextSuccessful);
                    parentNode.Nodes.Add(newNode);

                    UpdateTreeView();
                    RaiseTreeChanged();

                    RaiseSuccessStatus();
                    break;
            }
        }

        protected override bool CanDuplicate(TreeNode<object> node)
        {
            return true;
        }

        protected override void DuplicateNode(TreeNode<object> node)
        {
            switch (node.Data)
            {
                case RdbnListEntry list:
                    RdbnListEntry duplicatedEntry = DuplicateListEntry(list);

                    RdbnListEntry[] newLists = _config.Lists;
                    Array.Resize(ref newLists, newLists.Length + 1);
                    newLists[^1] = duplicatedEntry;

                    _config.Lists = newLists;

                    GetRootNodes().Add(CreateListNode(_config, duplicatedEntry, ColorResources.TextSuccessful));
                    break;

                case (RdbnTypeDeclaration type, object[][] values):
                    object[][] duplicatedValues = DuplicateListValues(type, values);

                    object[][][] newValues = ((RdbnListEntry)node.Parent.Data).Values;
                    Array.Resize(ref newValues, newValues.Length + 1);
                    newValues[^1] = duplicatedValues;

                    ((RdbnListEntry)node.Parent.Data).Values = newValues;

                    node.Parent.Nodes.Add(CreateValueNode(type, duplicatedValues, newValues.Length, ColorResources.TextSuccessful));
                    break;
            }

            UpdateTreeView();
            RaiseTreeChanged();
        }

        protected override bool CanRemove(TreeNode<object> node)
        {
            return true;
        }

        protected override void RemoveNode(TreeNode<object> node)
        {
            switch (node.Data)
            {
                case RdbnListEntry list:
                    var newLists = new RdbnListEntry[_config.Lists.Length - 1];
                    int listIndex = Array.IndexOf(_config.Lists, list);

                    Array.Copy(_config.Lists, newLists, listIndex);
                    Array.Copy(_config.Lists, listIndex + 1, newLists, listIndex, newLists.Length - listIndex);

                    _config.Lists = newLists;

                    GetRootNodes().Remove(node);
                    break;

                case (RdbnTypeDeclaration type, _):
                    int listIndex1 = Array.IndexOf(_config.Lists, (RdbnListEntry)node.Parent.Data);
                    object[][][] listValues = _config.Lists[listIndex1].Values;

                    int nodeIndex = node.Parent.Nodes.IndexOf(node);
                    var newValues = new object[listValues.Length - 1][][];

                    Array.Copy(listValues, newValues, nodeIndex);
                    Array.Copy(listValues, nodeIndex + 1, newValues, nodeIndex, newValues.Length - nodeIndex);

                    _config.Lists[listIndex1].Values = newValues;

                    for (int i = nodeIndex; i < node.Parent.Nodes.Count; i++)
                        node.Parent.Nodes[i].Text = GetNodeName(type, i);

                    node.Parent.Nodes.Remove(node);
                    break;
            }

            UpdateTreeView();
            RaiseTreeChanged();
        }

        protected override bool CanImport(TreeNode<object>? parentNode)
        {
            return false;
        }

        protected override void Import(TreeNode<object>? parentNode)
        {
            throw new NotImplementedException();
        }

        protected override bool CanExport(TreeNode<object>? node)
        {
            return false;
        }

        protected override void Export(TreeNode<object>? node)
        {
            throw new NotImplementedException();
        }

        private async Task<RdbnTypeDeclaration?> SelectListType()
        {
            LocalizedString[] types = _config.Types.Select(x => (LocalizedString)x.Name).ToArray();

            LocalizedString? selectedItem = await ComboInputBox.ShowAsync(
                LocalizationResources.RdbnListAddCaption,
                LocalizationResources.RdbnListAddTypeDialogCaption,
                types, types.FirstOrDefault());

            return _config.Types.FirstOrDefault(x => x.Name == selectedItem);
        }

        private RdbnListEntry DuplicateListEntry(RdbnListEntry list)
        {
            var values = new object[list.Values.Length][][];
            for (var i = 0; i < list.Values.Length; i++)
                values[i] = DuplicateListValues(_config.Types[list.TypeIndex], list.Values[i]);

            return new RdbnListEntry
            {
                Name = list.Name,
                TypeIndex = list.TypeIndex,
                Values = values
            };
        }

        private object[][] DuplicateListValues(RdbnTypeDeclaration type, object[][] values)
        {
            var newValues = new object[values.Length][];

            for (var i = 0; i < type.Fields.Length; i++)
            {
                RdbnFieldDeclaration field = type.Fields[i];
                object[] fieldValues = values[i];

                newValues[i] = new object[field.Count];
                for (var j = 0; j < field.Count; j++)
                    newValues[i][j] = DuplicateValue(fieldValues[j], field);
            }

            return newValues;
        }

        private object DuplicateValue(object value, RdbnFieldDeclaration field)
        {
            switch (field.FieldType)
            {
                case FieldType.AbilityData:
                case FieldType.EnhanceData:
                case FieldType.StatusRate:
                    var baValue = (byte[])value;
                    var newBaValue = new byte[baValue.Length];

                    Array.Copy(baValue, newBaValue, baValue.Length);

                    return newBaValue;

                case FieldType.Bool:
                    return (bool)value;

                case FieldType.Byte:
                    return (byte)value;

                case FieldType.Short:
                    return (short)value;

                case FieldType.Int:
                    return (int)value;

                case FieldType.ActType:
                    return (short)value;

                case FieldType.Flag:
                    return (int)value;

                case FieldType.Hash:
                    return (uint)value;

                case FieldType.Float:
                    return (float)value;

                case FieldType.RateMatrix:
                case FieldType.Position:
                    var ftValue = (float[])value;
                    var newFtValue = new float[ftValue.Length];

                    Array.Copy(ftValue, newFtValue, ftValue.Length);

                    return newFtValue;

                case FieldType.String:
                    if (value is string sValue)
                        return sValue;

                    return (uint)value;

                case FieldType.DataTuple:
                    var stValue = (short[])value;
                    var newStValue = new short[stValue.Length];

                    Array.Copy(stValue, newStValue, stValue.Length);

                    return newStValue;

                default:
                    throw new InvalidOperationException($"Unknown field type {field.FieldType}.");
            }
        }

        private object GetDefaultValue(RdbnFieldDeclaration field)
        {
            switch (field.FieldType)
            {
                case FieldType.AbilityData:
                case FieldType.EnhanceData:
                case FieldType.StatusRate:
                    return new byte[field.Size];

                case FieldType.Bool:
                    return false;

                case FieldType.Byte:
                    return (byte)0;

                case FieldType.Short:
                case FieldType.ActType:
                    return (short)0;

                case FieldType.Int:
                case FieldType.Flag:
                    return 0;

                case FieldType.Hash:
                    return 0u;

                case FieldType.Float:
                    return 0f;

                case FieldType.RateMatrix:
                case FieldType.Position:
                    return new float[] { 0, 0, 0, 0 };

                case FieldType.String:
                    return string.Empty;

                case FieldType.DataTuple:
                    return new short[] { 0, 0 };

                default:
                    throw new InvalidOperationException($"Unknown field type {field.FieldType}.");
            }
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
}
