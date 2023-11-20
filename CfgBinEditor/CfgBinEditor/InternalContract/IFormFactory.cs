using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CfgBinEditor.Forms;
using CfgBinEditor.InternalContract.Exceptions;
using CrossCutting.Core.Contract.Aspects;
using Logic.Domain.Level5.Contract.DataClasses;

namespace CfgBinEditor.InternalContract
{
    [MapException(typeof(FormFactoryException))]
    public interface IFormFactory
    {
        MainForm CreateMainForm();
        ConfigurationForm CreateConfigurationForm(Configuration config);
    }
}
