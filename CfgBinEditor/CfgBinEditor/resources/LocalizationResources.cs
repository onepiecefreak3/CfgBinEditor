using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGui.Forms.Localization;

namespace CfgBinEditor.resources
{
    internal class LocalizationResources
    {
        public static LocalizedString ApplicationTitle => new("Application.Title");

        public static LocalizedString MenuFileCaption => new("Menu.File.Caption");
        public static LocalizedString MenuFileOpenCaption => new("Menu.File.Open.Caption");

        public static LocalizedString MenuSettingsCaption => new("Menu.Settings.Caption");
        public static LocalizedString MenuSettingsLanguagesCaption => new("Menu.Settings.Languages.Caption");

        public static LocalizedString MenuInfoCaption => new("Menu.Info.Caption");

        public static LocalizedString FileOpenCaption => new("File.Open.Caption");
        public static LocalizedString FileOpenCfgBinFilterCaption => new("File.Open.CfgBinFilter.Caption");
        public static LocalizedString FileOpenCancel => new("File.Open.Cancel");

        public static LocalizedString FileCloseUnsavedChangesCaption => new("File.Close.UnsavedChanges.Caption");
        public static LocalizedString FileCloseUnsavedChangesText => new("File.Close.UnsavedChanges.Text");

        public static LocalizedString CfgBinEntryNameCaption => new("CfgBin.Entry.Name.Caption");
        public static LocalizedString CfgBinEntryTypeCaption => new("CfgBin.Entry.Type.Caption");
        public static LocalizedString CfgBinEntryValueCaption => new("CfgBin.Entry.Value.Caption");

        public static LocalizedString CfgBinEntryTypeStringCaption => new("CfgBin.Entry.Type.String.Caption");
        public static LocalizedString CfgBinEntryTypeUIntCaption => new("CfgBin.Entry.Type.UInt.Caption");
        public static LocalizedString CfgBinEntryTypeFloatCaption => new("CfgBin.Entry.Type.Float.Caption");

        public static LocalizedString CfgBinTagsLoadErrorCaption(string error) =>
            new("CfgBin.Tags.Load.Error.Caption", () => error);
    }
}
