using CfgBinEditor.Components;
using CfgBinEditor.InternalContract;
using CfgBinEditor.Messages;
using CrossCutting.Core.Contract.EventBrokerage;
using ImGui.Forms.Controls.Base;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Controls.Lists;
using Logic.Domain.Level5Management.Contract.DataClasses;
using System;
using System.IO;
using Logic.Domain.Level5Management.Contract;

namespace CfgBinEditor.Forms
{
    public partial class RdbnForm : Component
    {
        private readonly Rdbn _config;
        private readonly IComponentFactory _componentFactory;
        private readonly IEventBroker _eventBroker;
        private readonly IRdbnWriter _writer;

        public RdbnForm(Rdbn config, IFormFactory formFactory, IComponentFactory componentFactory, IEventBroker eventBroker, IRdbnWriter writer)
        {
            InitializeComponent(config, formFactory);

            _config = config;
            _componentFactory = componentFactory;
            _eventBroker = eventBroker;
            _writer = writer;

            eventBroker.Subscribe<FileSaveRequestMessage>(SaveFile);

            if (_treeViewForm!.SelectedEntry != null)
                ChangeEntry(_treeViewForm.SelectedEntry);

            _eventBroker.Subscribe<TreeChangedMessage<Rdbn, object>>(msg =>
            {
                if (msg.TreeViewForm == _treeViewForm)
                    RaiseFileChanged();
            });

            eventBroker.Subscribe<TreeEntryChangedMessage<Rdbn, object>>(msg =>
            {
                if (msg.TreeViewForm == _treeViewForm)
                    ChangeEntry(msg.Entry);
            });
        }

        private void SaveFile(FileSaveRequestMessage msg)
        {
            if (!msg.ConfigForms.TryGetValue(this, out string? savePath))
                return;

            if (!TryWriteFile(savePath, out Exception? e))
            {
                RaiseFileSaved(e);
                return;
            }

            _treeViewForm.ResetNodeState();

            RaiseFileSaved();
        }

        private bool TryWriteFile(string savePath, out Exception? ex)
        {
            ex = null;

            try
            {
                using Stream fileStream = _writer.Write(_config);
                using Stream targetFileStream = File.Create(savePath);

                fileStream.CopyTo(targetFileStream);
            }
            catch (Exception e)
            {
                ex = e;
                return false;
            }

            return true;
        }

        private void ChangeEntry(object? obj)
        {
            var list = new List<RdbnValueComponent>
            {
                Alignment = Alignment.Vertical,
                ItemSpacing = 5
            };

            _contentPanel.ShowBorder = false;

            if (obj is not (RdbnTypeDeclaration type, object[][] values))
            {
                _contentPanel.Content = list;
                return;
            }

            for (var i = 0; i < type.Fields.Length; i++)
            {
                RdbnValueComponent value = _componentFactory.CreateRdbnValue(this, values[i], type.Fields[i]);
                list.Items.Add(value);
            }

            _contentPanel.Content = list;
        }

        private void RaiseFileChanged()
        {
            _eventBroker.Raise(new FileChangedMessage(this));
        }

        private void RaiseFileSaved(Exception? e = null)
        {
            _eventBroker.Raise(new FileSavedMessage(this, e));
        }
    }
}
