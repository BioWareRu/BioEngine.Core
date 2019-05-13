using System;
using System.IO;
using System.Threading.Tasks;
using BioEngine.Core.DB;
using BioEngine.Core.Repository;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BioEngine.Core.Storage
{
    [UsedImplicitly]
    public class FileStorage : Storage
    {
        public FileStorage(FileStorageModuleConfig options, StorageItemsRepository repository,
            BioContext dbContext, ILogger<FileStorage> logger) : base(options, repository, dbContext, logger)
        {
        }

        protected override Task<bool> DoSaveAsync(string path, string tmpPath)
        {
            var dirPath = Path.GetDirectoryName(path);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath ?? throw new Exception($"Empty dir path in {path}"));
            }

            File.Move(tmpPath, path);
            return Task.FromResult(true);
        }

        protected override Task<bool> DoDeleteAsync(string path)
        {
            File.Delete(path);
            return Task.FromResult(true);
        }
    }

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

    public class FileStorageModuleConfig : StorageModuleConfig
    {
        public string StoragePath { get; set; } = "";
    }
}
