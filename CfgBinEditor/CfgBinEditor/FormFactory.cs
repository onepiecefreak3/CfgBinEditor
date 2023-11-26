using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using CfgBinEditor.Forms;
using CfgBinEditor.InternalContract;
using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.DependencyInjection.DataClasses;
using Logic.Domain.Level5.Contract.DataClasses;

namespace CfgBinEditor
{
    internal class FormFactory : IFormFactory
    {
        private readonly ICoCoKernel _kernel;

        public FormFactory(ICoCoKernel kernel)
        {
            _kernel = kernel;
        }

        public MainForm CreateMainForm()
        {
            return _kernel.Get<MainForm>();
        }

        public ConfigurationForm CreateConfigurationForm(Configuration config)
        {
            return _kernel.Get<ConfigurationForm>(
                new ConstructorParameter("config", config));
        }
    }
}
