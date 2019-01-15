using System;
using System.IO;
using System.Threading.Tasks;
using BioEngine.Core.Repository;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BioEngine.Core.Storage
{
    [UsedImplicitly]
    public class FileStorage : Storage
    {
        private readonly FileStorageOptions _options;

        public FileStorage(IOptions<FileStorageOptions> options, IServiceProvider serviceProvider, ILogger<FileStorage> logger) : base(options, serviceProvider, logger)
        {
            _options = options.Value;
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
    }

    public class FileStorageOptions : StorageOptions
    {
        public string StoragePath { get; set; }
    }
}