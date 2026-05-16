using CfgBinEditor;
using CfgBinEditor.Forms;
using CfgBinEditor.InternalContract;
using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.EventBrokerage;
using CrossCutting.Core.Contract.Messages;
using ImGui.Forms;
using ImGui.Forms.Localization;
using ImGui.Forms.Factories;

KernelLoader loader = new();
ICoCoKernel kernel = loader.Initialize();

var eventBroker = kernel.Get<IEventBroker>();
eventBroker.Raise(new InitializeApplicationMessage());

var localizer = kernel.Get<ILocalizer>();
var app = new Application(localizer);

FontFactory.RegisterFromResource("Roboto", "roboto.ttf");
FontFactory.RegisterFromResource("NotoJp", "notojp.ttf");
FontFactory.RegisterFromResource("NotoKr", "notokr.ttf");
FontFactory.RegisterFromResource("NotoZh", "notozhtc.ttf");

var formFactory = kernel.Get<IFormFactory>();

MainForm form = formFactory.CreateMainForm();
form.DefaultFont = FontFactory.GetDefault(13, FontFactory.Get("Roboto", 13, FontFactory.Get("NotoJp", 15, FontFactory.Get("NotoKr", 15, FontFactory.Get("NotoZh", 15)))));

app.Execute(form);
