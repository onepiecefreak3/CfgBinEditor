namespace Logic.Business.CfgBinEditorManagement.InternalContract;

public interface IGameSettingsReader<TEntry>
{
    IDictionary<string, IDictionary<string, IList<TEntry>>> Read();
}