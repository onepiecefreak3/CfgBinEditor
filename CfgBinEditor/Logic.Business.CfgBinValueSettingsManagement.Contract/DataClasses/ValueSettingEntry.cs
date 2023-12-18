namespace Logic.Business.CfgBinValueSettingsManagement.Contract.DataClasses
{
    public struct ValueSettingEntry
    {
        public static ValueSettingEntry Empty => new() { Name = string.Empty, IsHex = false };

        public string Name { get; set; }
        public bool IsHex { get; set; }
    }
}
