using BioEngine.Core.Modules;

namespace BioEngine.Core.DB
{
    public abstract class DatabaseModule<T> : BioEngineModule<T> where T : DatabaseModuleConfig, new()
    {
    }

    public abstract class DatabaseModuleConfig
    {
    }
}
