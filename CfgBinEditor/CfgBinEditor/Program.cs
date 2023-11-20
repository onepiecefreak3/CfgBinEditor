using CfgBinEditor;
using CfgBinEditor.InternalContract;
using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.EventBrokerage;
using CrossCutting.Core.Contract.Messages;
using ImGui.Forms;
using ImGui.Forms.Localization;
using ImGui.Forms.Resources;

KernelLoader loader = new();
ICoCoKernel kernel = loader.Initialize();

var eventBroker = kernel.Get<IEventBroker>();
eventBroker.Raise(new InitializeApplicationMessage());

var localizer = kernel.Get<ILocalizer>();
var app = new Application(localizer);

FontResources.RegisterArial(15);

var formFactory = kernel.Get<IFormFactory>();
app.Execute(formFactory.CreateMainForm());
