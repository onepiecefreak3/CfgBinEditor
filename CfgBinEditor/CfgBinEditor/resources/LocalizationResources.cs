using System;
using ImGui.Forms.Localization;
using ImGui.Forms.Models;

namespace CfgBinEditor.resources
{
    internal class LocalizationResources
    {
        public static LocalizedString ApplicationTitle => LocalizedString.FromId("Application.Title");
        public static LocalizedString ApplicationCloseUnsavedChangesCaption => LocalizedString.FromId("Application.Close.UnsavedChanges.Caption");
        public static LocalizedString ApplicationCloseUnsavedChangesText => LocalizedString.FromId("Application.Close.UnsavedChanges.Text");

        public static LocalizedString MenuFileCaption => LocalizedString.FromId("Menu.File.Caption");
        public static LocalizedString MenuFileOpenCaption => LocalizedString.FromId("Menu.File.Open.Caption");

        public static LocalizedString MenuSettingsCaption => LocalizedString.FromId("Menu.Settings.Caption");
        public static LocalizedString MenuSettingsLanguagesCaption => LocalizedString.FromId("Menu.Settings.Languages.Caption");
        public static LocalizedString MenuSettingsThemesCaption => LocalizedString.FromId("Menu.Settings.Themes.Caption");

        public static LocalizedString MenuSettingsThemeCaption(Theme theme) =>
            LocalizedString.FromId($"Menu.Settings.Theme.{theme}.Caption");

        public static LocalizedString MenuInfoCaption => LocalizedString.FromId("Menu.Info.Caption");

        public static LocalizedString FileOpenCaption => LocalizedString.FromId("File.Open.Caption");
        public static LocalizedString FileOpenCfgBinFilterCaption => LocalizedString.FromId("File.Open.CfgBinFilter.Caption");
        public static LocalizedString FileOpenJsonFilterCaption => LocalizedString.FromId("File.Open.JsonFilter.Caption");
        public static LocalizedString FileOpenCancel => LocalizedString.FromId("File.Open.Cancel");
        public static LocalizedString FileOpenErrorCaption(string fileName, Func<string> message) =>
            LocalizedString.FromId("File.Open.Error.Caption", () => fileName, message);
        public static LocalizedString FileOpenUnsupportedFileType => LocalizedString.FromId("File.Open.Error.Type.Caption");

        public static LocalizedString FileSaveTooltipSingleCaption(Func<object> fileNameRetriever) =>
            LocalizedString.FromId("File.Save.Tooltip.Single.Caption", fileNameRetriever);
        public static LocalizedString FileSaveTooltipAllCaption => LocalizedString.FromId("File.Save.Tooltip.All.Caption");
        public static LocalizedString FileSaveErrorCaption(string fileName, string message) =>
            LocalizedString.FromId("File.Save.Error.Caption", () => fileName, () => message);

        public static LocalizedString FileCloseUnsavedChangesCaption => LocalizedString.FromId("File.Close.UnsavedChanges.Caption");
        public static LocalizedString FileCloseUnsavedChangesText => LocalizedString.FromId("File.Close.UnsavedChanges.Text");

        public static LocalizedString GameNoneCaption => LocalizedString.FromId("Game.None.Caption");
        public static LocalizedString GameAddButtonCaption => LocalizedString.FromId("Game.Add.Button.Caption");
        public static LocalizedString GameAddDialogCaption => LocalizedString.FromId("Game.Add.Dialog.Caption");
        public static LocalizedString GameAddDialogText => LocalizedString.FromId("Game.Add.Dialog.Text");
        public static LocalizedString GameAddDialogPlaceholder => LocalizedString.FromId("Game.Add.Dialog.Placeholder");
        public static LocalizedString GameAddDialogInvalidCharactersCaption => LocalizedString.FromId("Game.Add.Dialog.InvalidCharacters.Caption");
        public static LocalizedString GameAddDialogInvalidCharactersText=> LocalizedString.FromId("Game.Add.Dialog.InvalidCharacters.Text");

        public static LocalizedString CfgBinEntryNameCaption => LocalizedString.FromId("CfgBin.Entry.Name.Caption");
        public static LocalizedString CfgBinEntryTypeCaption => LocalizedString.FromId("CfgBin.Entry.Type.Caption");
        public static LocalizedString CfgBinEntryValueCaption => LocalizedString.FromId("CfgBin.Entry.Value.Caption");
        public static LocalizedString CfgBinEntryIsHexCaption => LocalizedString.FromId("CfgBin.Entry.IsHex.Caption");

        public static LocalizedString CfgBinEntryTypeStringCaption => LocalizedString.FromId("CfgBin.Entry.Type.String.Caption");
        public static LocalizedString CfgBinEntryTypeIntCaption => LocalizedString.FromId("CfgBin.Entry.Type.Int.Caption");
        public static LocalizedString CfgBinEntryTypeFloatCaption => LocalizedString.FromId("CfgBin.Entry.Type.Float.Caption");

        public static LocalizedString CfgBinEntrySearchPlaceholderCaption =>
            LocalizedString.FromId("CfgBin.Entry.Search.Placeholder.Caption");

        public static LocalizedString CfgBinEntrySearchDialogCaption => LocalizedString.FromId("CfgBin.Entry.Search.Comparison.Dialog.Caption");
        public static LocalizedString CfgBinEntrySearchComparisonCaption => LocalizedString.FromId("CfgBin.Entry.Search.Comparison.Caption");

        public static LocalizedString CfgBinEntrySearchHex => LocalizedString.FromId("CfgBin.Entry.Search.Comparison.Hex");
        public static LocalizedString CfgBinEntrySearchDec => LocalizedString.FromId("CfgBin.Entry.Search.Comparison.Dec");
        public static LocalizedString CfgBinEntrySearchTags => LocalizedString.FromId("CfgBin.Entry.Search.Comparison.Tags");

        public static LocalizedString CfgBinEntryDuplicateCaption => LocalizedString.FromId("CfgBin.Entry.Duplicate.Caption");
        public static LocalizedString CfgBinEntryRemoveCaption => LocalizedString.FromId("CfgBin.Entry.Remove.Caption");
        public static LocalizedString CfgBinEntryImportCaption => LocalizedString.FromId("CfgBin.Entry.Import.Caption");
        public static LocalizedString CfgBinEntryExportCaption => LocalizedString.FromId("CfgBin.Entry.Export.Caption");
        public static LocalizedString CfgBinEntryAddCaption => LocalizedString.FromId("CfgBin.Entry.Add.Caption");
        public static LocalizedString CfgBinEntryAddValueCaption => LocalizedString.FromId("CfgBin.Entry.Add.Value.Caption");
        public static LocalizedString CfgBinEntryAddRootCaption => LocalizedString.FromId("CfgBin.Entry.Add.Root.Caption");
        public static LocalizedString CfgBinEntryAddDialogCaption => LocalizedString.FromId("CfgBin.Entry.Add.Dialog.Caption");
        public static LocalizedString CfgBinEntryAddErrorCaption => LocalizedString.FromId("CfgBin.Entry.Add.Error.Caption");

        public static LocalizedString RdbnListAddCaption => LocalizedString.FromId("Rdbn.List.Add.Caption");
        public static LocalizedString RdbnListAddTypeDialogCaption => LocalizedString.FromId("Rdbn.List.Add.Type.Dialog.Caption");
        public static LocalizedString RdbnListAddTypeErrorCaption => LocalizedString.FromId("Rdbn.List.Add.Type.Error.Caption");
        public static LocalizedString RdbnListAddNameDialogCaption => LocalizedString.FromId("Rdbn.List.Add.Name.Dialog.Caption");
        public static LocalizedString RdbnListAddNameErrorCaption => LocalizedString.FromId("Rdbn.List.Add.Name.Error.Caption");

        public static LocalizedString CfgBinEntryRandomTooltip => LocalizedString.FromId("CfgBin.Entry.Random.Tooltip");
        public static LocalizedString CfgBinEntryEmptyTooltip => LocalizedString.FromId("CfgBin.Entry.Empty.Tooltip");

        public static LocalizedString CfgBinTagsLoadErrorCaption(string error) =>
            LocalizedString.FromId("CfgBin.Tags.Load.Error.Caption", () => error);
        public static LocalizedString CfgBinIdsLoadErrorCaption(string error) =>
            LocalizedString.FromId("CfgBin.Ids.Load.Error.Caption", () => error);
    }
}
