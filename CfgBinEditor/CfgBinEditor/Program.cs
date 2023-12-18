using CfgBinEditor;
using CfgBinEditor.InternalContract;
using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.EventBrokerage;
using CrossCutting.Core.Contract.Messages;
using ImGui.Forms;
using ImGui.Forms.Localization;
using ImGui.Forms.Models;
using System.Reflection;

KernelLoader loader = new();
ICoCoKernel kernel = loader.Initialize();

var eventBroker = kernel.Get<IEventBroker>();
eventBroker.Raise(new InitializeApplicationMessage());

var localizer = kernel.Get<ILocalizer>();
var app = new Application(localizer);

Application.FontFactory.RegisterFromResource(Assembly.GetExecutingAssembly(), "notojp.ttf", 15, FontGlyphRange.Japanese, "…");

var formFactory = kernel.Get<IFormFactory>();
app.Execute(formFactory.CreateMainForm());
