using CrossCutting.Core.Contract.EventBrokerage;
using ImGui.Forms.Controls.Tree;
using Logic.Domain.Level5Management.Contract.DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CfgBinEditor.Messages;

namespace CfgBinEditor.Forms
{
    public partial class RdbnTreeViewForm : BaseTreeViewForm<Rdbn, object>
    {
        public RdbnTreeViewForm(Rdbn config, IEventBroker eventBroker) : base(config, eventBroker)
        {
        }

        protected override void DuplicateNode(TreeNode<object> node)
        {
            throw new NotImplementedException();
        }
    }
}
