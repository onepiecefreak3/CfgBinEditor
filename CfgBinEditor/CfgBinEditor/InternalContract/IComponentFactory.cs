using CfgBinEditor.InternalContract.Exceptions;
using CrossCutting.Core.Contract.Aspects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CfgBinEditor.Components;
using Logic.Domain.Level5Management.Contract.DataClasses;
using CfgBinEditor.Forms;

namespace CfgBinEditor.InternalContract
{
    [MapException(typeof(ComponentFactoryException))]
    public interface IComponentFactory
    {
        RdbnValueComponent CreateRdbnValue(RdbnForm parentForm, object[] values, RdbnFieldDeclaration fieldDeclaration);
    }
}
