using System;
using Microsoft.Extensions.DependencyInjection;

namespace BioEngine.Core.Storage.FileSystem
{
    public class FileStorageModule : StorageModule<FileStorageModuleConfig>
    {
        protected override void CheckConfig()
        {
            if (string.IsNullOrEmpty(Config.StoragePath))
            {
                throw new ArgumentException("File storage path is empty");
            }

            if (Config.PublicUri == null)
            {
                throw new ArgumentException("Storage url is empty");
            }
        }

        protected override void ConfigureStorage(IServiceCollection services)
        {
            services.AddSingleton(Config);
            services.AddScoped<IStorage, FileStorage>();
        }
    }
}