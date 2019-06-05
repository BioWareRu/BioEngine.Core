using BioEngine.Core.Modules;

namespace BioEngine.Core.DB
{
    public abstract class DatabaseModule<T> : BaseBioEngineModule<T> where T : DatabaseModuleConfig
    {
    }

    public abstract class DatabaseModuleConfig
    {
    }
}
