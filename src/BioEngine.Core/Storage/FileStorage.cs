using System;
using System.IO;
using System.Threading.Tasks;
using BioEngine.Core.Interfaces;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;

namespace BioEngine.Core.Storage
{
    [UsedImplicitly]
    public class FileStorage : IStorage
    {
        private readonly FileStorageOptions _options;

        public FileStorage(IOptions<FileStorageOptions> options)
        {
            _options = options.Value;
        }

        public async Task<StorageItem> SaveFile(byte[] file, string fileName, string path)
        {
            var destinationPath = _options.StoragePath + "/" + path;
            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }

            var extension = fileName.Substring(fileName.LastIndexOf('.'));
            var destinationFileName = Guid.NewGuid() + extension;
            var filePath = destinationPath + "/" + destinationFileName;

            using (var sourceStream = new FileStream(filePath,
                FileMode.CreateNew, FileAccess.Write, FileShare.None,
                bufferSize: 4096, useAsync: true))
            {
                await sourceStream.WriteAsync(file, 0, file.Length);
            }

            return new StorageItem
            {
                FileName = fileName,
                FilePath = $"{path}/{destinationFileName}",
                FileSize = file.LongLength,
                PublicUri = new Uri($"{_options.PublicUri}/{path}/{destinationFileName}")
            };
        }

        public Task<bool> DeleteFile(string filePath)
        {
            var path = _options.StoragePath + '/' + filePath;
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            return Task.FromResult(true);
        }
    }

    public class FileStorageOptions : IStorageOptions
    {
        public Uri PublicUri { get; set; }
        public string StoragePath { get; set; }
    }
}