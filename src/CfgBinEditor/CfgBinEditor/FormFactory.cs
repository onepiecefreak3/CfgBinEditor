using CfgBinEditor.Forms;
using CfgBinEditor.InternalContract;
using CfgBinEditor.InternalContract.DataClasses;
using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.DependencyInjection.DataClasses;
using Konnect.Contract.Management.Plugin;
using Logic.Domain.Level5Management.Contract.DataClasses;

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

        public T2bForm CreateT2bForm(T2bFile file, IPluginManager pluginManager)
        {
            return _kernel.Get<T2bForm>(
                new ConstructorParameter("file", file),
                new ConstructorParameter("pluginManager", pluginManager));
        }

        public RdbnForm CreateRdbnForm(Rdbn config)
        {
            return _kernel.Get<RdbnForm>(
                new ConstructorParameter("config", config));
        }

        public T2bTreeViewForm CreateT2bTreeViewForm(T2b config)
        {
            return _kernel.Get<T2bTreeViewForm>(
                new ConstructorParameter("config", config));
        }

        public RdbnTreeViewForm CreateRdbnTreeViewForm(Rdbn config)
        {
            return _kernel.Get<RdbnTreeViewForm>(
                new ConstructorParameter("config", config));
        }
    }
}