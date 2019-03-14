using BioEngine.Core.Interfaces;
using BioEngine.Core.Properties;
using Microsoft.Extensions.Logging;

namespace BioEngine.Core.Web
{
    public class BaseControllerContext
    {
        public readonly IStorage Storage;
        public readonly PropertiesProvider PropertiesProvider;
        public ILogger Logger { get; }

        public BaseControllerContext(ILoggerFactory loggerFactory, IStorage storage, PropertiesProvider propertiesProvider)
        {
            Storage = storage;
            PropertiesProvider = propertiesProvider;
            Logger = loggerFactory.CreateLogger(GetType());
        }
    }

    public class BaseControllerContext<TEntity> : BaseControllerContext
        where TEntity : class, IEntity
    {
        public IBioRepository<TEntity> Repository { get; }

        public BaseControllerContext(ILoggerFactory loggerFactory, IStorage storage,
            PropertiesProvider propertiesProvider,
            IBioRepository<TEntity> repository) : base(loggerFactory, storage, propertiesProvider)
        {
            Repository = repository;
        }
    }
}
