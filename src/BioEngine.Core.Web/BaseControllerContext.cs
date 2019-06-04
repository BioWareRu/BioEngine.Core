using BioEngine.Core.Abstractions;
using BioEngine.Core.Properties;
using BioEngine.Core.Storage;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace BioEngine.Core.Web
{
    public class BaseControllerContext
    {
        public readonly LinkGenerator LinkGenerator;
        public readonly IStorage Storage;
        public readonly PropertiesProvider PropertiesProvider;
        public ILogger Logger { get; }

        public BaseControllerContext(ILoggerFactory loggerFactory, IStorage storage,
            PropertiesProvider propertiesProvider, LinkGenerator linkGenerator)
        {
            LinkGenerator = linkGenerator;
            Storage = storage;
            PropertiesProvider = propertiesProvider;
            Logger = loggerFactory.CreateLogger(GetType());
        }
    }

    public class BaseControllerContext<TEntity, TRepository> : BaseControllerContext
        where TEntity : class, IEntity
        where TRepository : IBioRepository<TEntity>
    {
        public TRepository Repository { get; }

        public BaseControllerContext(ILoggerFactory loggerFactory, IStorage storage,
            PropertiesProvider propertiesProvider, LinkGenerator linkGenerator,
            TRepository repository) : base(loggerFactory, storage, propertiesProvider,
            linkGenerator)
        {
            Repository = repository;
        }
    }
}
