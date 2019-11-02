using System;
using System.IO;
using System.Threading.Tasks;
using BioEngine.Core.DB;
using BioEngine.Core.Repository;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace BioEngine.Core.Storage.FileSystem
{
    [UsedImplicitly]
    public class FileStorage : Storage
    {
        private readonly string _storagePath;

        public FileStorage(FileStorageModuleConfig options, StorageItemsRepository repository,
            BioContext dbContext, ILogger<FileStorage> logger) : base(options, repository, dbContext, logger)
        {
            _storagePath = options.StoragePath;
        }

        protected override async Task<bool> DoSaveAsync(string path, Stream file)
        {
            var dirPath = Path.Combine(_storagePath, Path.GetDirectoryName(path));
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath ?? throw new Exception($"Empty dir path in {path}"));
            }

            await using var fileStream = File.Create(path);
            file.Seek(0, SeekOrigin.Begin);
            await file.CopyToAsync(fileStream);
            return true;
        }

        protected override Task<bool> DoDeleteAsync(string path)
        {
            File.Delete(path);
            return Task.FromResult(true);
        }
    }
}
