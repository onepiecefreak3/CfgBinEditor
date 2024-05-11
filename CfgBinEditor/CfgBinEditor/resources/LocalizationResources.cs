using System;
using ImGui.Forms.Localization;
using ImGui.Forms.Models;

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
        public static LocalizedString MenuSettingsThemesCaption => new("Menu.Settings.Themes.Caption");

        public static LocalizedString MenuSettingsThemeCaption(Theme theme) =>
            new($"Menu.Settings.Theme.{theme}.Caption");

        public static LocalizedString MenuInfoCaption => new("Menu.Info.Caption");

        public static LocalizedString FileOpenCaption => new("File.Open.Caption");
        public static LocalizedString FileOpenCfgBinFilterCaption => new("File.Open.CfgBinFilter.Caption");
        public static LocalizedString FileOpenCancel => new("File.Open.Cancel");
        public static LocalizedString FileOpenErrorCaption(string fileName, Func<string> message) =>
            new("File.Open.Error.Caption", () => fileName, message);
        public static LocalizedString FileOpenUnsupportedFileType => new("File.Open.Error.Type.Caption");

        public static LocalizedString FileSaveTooltipSingleCaption(Func<object> fileNameRetriever) =>
            new("File.Save.Tooltip.Single.Caption", fileNameRetriever);
        public static LocalizedString FileSaveTooltipAllCaption => new("File.Save.Tooltip.All.Caption");
        public static LocalizedString FileSaveErrorCaption(string fileName, string message) =>
            new("File.Save.Error.Caption", () => fileName, () => message);

        public static LocalizedString FileCloseUnsavedChangesCaption => new("File.Close.UnsavedChanges.Caption");
        public static LocalizedString FileCloseUnsavedChangesText => new("File.Close.UnsavedChanges.Text");

        public static LocalizedString GameNoneCaption => new("Game.None.Caption");
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

        public static LocalizedString CfgBinEntrySearchDialogCaption => new("CfgBin.Entry.Search.Comparison.Dialog.Caption");
        public static LocalizedString CfgBinEntrySearchComparisonCaption => new("CfgBin.Entry.Search.Comparison.Caption");

        public static LocalizedString CfgBinEntrySearchHex => new("CfgBin.Entry.Search.Comparison.Hex");
        public static LocalizedString CfgBinEntrySearchDec => new("CfgBin.Entry.Search.Comparison.Dec");
        public static LocalizedString CfgBinEntrySearchTags => new("CfgBin.Entry.Search.Comparison.Tags");

        public static LocalizedString CfgBinEntryDuplicateCaption => new("CfgBin.Entry.Duplicate.Caption");
        public static LocalizedString CfgBinEntryRemoveCaption => new("CfgBin.Entry.Remove.Caption");
        public static LocalizedString CfgBinEntryImportCaption => new("CfgBin.Entry.Import.Caption");
        public static LocalizedString CfgBinEntryExportCaption => new("CfgBin.Entry.Export.Caption");
        public static LocalizedString CfgBinEntryAddCaption => new("CfgBin.Entry.Add.Caption");
        public static LocalizedString CfgBinEntryAddValueCaption => new("CfgBin.Entry.Add.Value.Caption");
        public static LocalizedString CfgBinEntryAddRootCaption => new("CfgBin.Entry.Add.Root.Caption");
        public static LocalizedString CfgBinEntryAddDialogCaption => new("CfgBin.Entry.Add.Dialog.Caption");
        public static LocalizedString CfgBinEntryAddErrorCaption => new("CfgBin.Entry.Add.Error.Caption");

        public static LocalizedString RdbnListAddCaption => new("Rdbn.List.Add.Caption");
        public static LocalizedString RdbnListAddTypeDialogCaption => new("Rdbn.List.Add.Type.Dialog.Caption");
        public static LocalizedString RdbnListAddTypeErrorCaption => new("Rdbn.List.Add.Type.Error.Caption");
        public static LocalizedString RdbnListAddNameDialogCaption => new("Rdbn.List.Add.Name.Dialog.Caption");
        public static LocalizedString RdbnListAddNameErrorCaption => new("Rdbn.List.Add.Name.Error.Caption");

        public static LocalizedString CfgBinEntryRandomTooltip => new("CfgBin.Entry.Random.Tooltip");

        public static LocalizedString CfgBinTagsLoadErrorCaption(string error) =>
            new("CfgBin.Tags.Load.Error.Caption", () => error);
        public static LocalizedString CfgBinIdsLoadErrorCaption(string error) =>
            new("CfgBin.Ids.Load.Error.Caption", () => error);
    }
}
