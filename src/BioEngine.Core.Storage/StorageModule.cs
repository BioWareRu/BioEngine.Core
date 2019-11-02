using System;
using BioEngine.Core.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BioEngine.Core.Storage
{
    public abstract class StorageModule<T> : BaseBioEngineModule<T> where T : StorageModuleConfig
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            base.ConfigureServices(services, configuration, environment);
            ConfigureStorage(services);
        }

        protected abstract void ConfigureStorage(IServiceCollection services);
    }

    public abstract class StorageModuleConfig
    {
        protected StorageModuleConfig(Uri publicUri)
        {
            PublicUri = publicUri;
        }

        public Uri PublicUri { get; }

        public int LargeThumbnailWidth { get; set; } = 800;
        public int LargeThumbnailHeight { get; set; } = 800;
        public int MediumThumbnailWidth { get; set; } = 300;
        public int MediumThumbnailHeight { get; set; } = 300;
        public int SmallThumbnailWidth { get; set; } = 100;
        public int SmallThumbnailHeight { get; set; } = 100;
    }
}
