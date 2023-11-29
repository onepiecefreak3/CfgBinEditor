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
        public static LocalizedString ApplicationCloseUnsavedChangesCaption => new("Application.Close.UnsavedChanges.Caption");
        public static LocalizedString ApplicationCloseUnsavedChangesText => new("Application.Close.UnsavedChanges.Text");

        public static LocalizedString MenuFileCaption => new("Menu.File.Caption");
        public static LocalizedString MenuFileOpenCaption => new("Menu.File.Open.Caption");

        public static LocalizedString MenuSettingsCaption => new("Menu.Settings.Caption");
        public static LocalizedString MenuSettingsLanguagesCaption => new("Menu.Settings.Languages.Caption");

        public static LocalizedString MenuInfoCaption => new("Menu.Info.Caption");

        public static LocalizedString FileOpenCaption => new("File.Open.Caption");
        public static LocalizedString FileOpenCfgBinFilterCaption => new("File.Open.CfgBinFilter.Caption");
        public static LocalizedString FileOpenCancel => new("File.Open.Cancel");
        public static LocalizedString FileOpenErrorCaption(string fileName, string message) =>
            new("File.Open.Error.Caption", () => fileName, () => message);

        public static LocalizedString FileSaveTooltipSingleCaption(Func<object> fileNameRetriever) =>
            new("File.Save.Tooltip.Single.Caption", fileNameRetriever);
        public static LocalizedString FileSaveTooltipAllCaption => new("File.Save.Tooltip.All.Caption");
        public static LocalizedString FileSaveErrorCaption(string fileName, string message) =>
            new("File.Save.Error.Caption", () => fileName, () => message);

        public static LocalizedString FileCloseUnsavedChangesCaption => new("File.Close.UnsavedChanges.Caption");
        public static LocalizedString FileCloseUnsavedChangesText => new("File.Close.UnsavedChanges.Text");

        public static LocalizedString GameAddButtonCaption => new("Game.Add.Button.Caption");
        public static LocalizedString GameAddDialogCaption => new("Game.Add.Dialog.Caption");
        public static LocalizedString GameAddDialogText => new("Game.Add.Dialog.Text");
        public static LocalizedString GameAddDialogPlaceholder => new("Game.Add.Dialog.Placeholder");

        public static LocalizedString CfgBinEntryNameCaption => new("CfgBin.Entry.Name.Caption");
        public static LocalizedString CfgBinEntryTypeCaption => new("CfgBin.Entry.Type.Caption");
        public static LocalizedString CfgBinEntryValueCaption => new("CfgBin.Entry.Value.Caption");
        public static LocalizedString CfgBinEntryIsHexCaption => new("CfgBin.Entry.IsHex.Caption");

        public static LocalizedString CfgBinEntryTypeStringCaption => new("CfgBin.Entry.Type.String.Caption");
        public static LocalizedString CfgBinEntryTypeIntCaption => new("CfgBin.Entry.Type.Int.Caption");
        public static LocalizedString CfgBinEntryTypeFloatCaption => new("CfgBin.Entry.Type.Float.Caption");

        public static LocalizedString CfgBinEntrySearchPlaceholderCaption =>
            new("CfgBin.Entry.Search.Placeholder.Caption");

        public static LocalizedString CfgBinEntryDuplicateCaption => new("CfgBin.Entry.Duplicate.Caption");

        public static LocalizedString CfgBinTagsLoadErrorCaption(string error) =>
            new("CfgBin.Tags.Load.Error.Caption", () => error);
    }
}
