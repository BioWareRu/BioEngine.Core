using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BioEngine.Core.Storage
{
    [UsedImplicitly]
    public class FileStorage : Storage
    {
        private readonly FileStorageOptions _options;

        public FileStorage(IOptions<FileStorageOptions> options, ILogger<FileStorage> logger) : base(options, logger)
        {
            _options = options.Value;
        }

        public override Task<IEnumerable<StorageItem>> ListItemsAsync(string path)
        {
            throw new NotImplementedException();
        }

        public override Task<IEnumerable<string>> ListDirectoriesAsync(string path)
        {
            throw new NotImplementedException();
        }

        public override Task CreateDirectoryAsync(string path)
        {
            throw new NotImplementedException();
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

        public override Task<bool> DeleteFileAsync(string filePath)
        {
            var path = _options.StoragePath + '/' + filePath;
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            return Task.FromResult(true);
        }
    }

    public class FileStorageOptions : StorageOptions
    {
        public string StoragePath { get; set; }
    }
}