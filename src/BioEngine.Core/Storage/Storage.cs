using System;
using System.IO;
using System.Threading.Tasks;
using BioEngine.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;

namespace BioEngine.Core.Storage
{
    public abstract class Storage : IStorage
    {
        private readonly ILogger<Storage> _logger;
        private readonly StorageOptions _options;

        protected Storage(IOptions<StorageOptions> options, ILogger<Storage> logger)
        {
            _logger = logger;
            _options = options.Value;
        }

        public async Task<StorageItem> SaveFile(byte[] file, string fileName, string path)
        {
            var destinationName = GetStorageFileName(fileName);
            var destinationPath = $"{path}/{destinationName}";

            var tmpPath = Path.GetTempFileName();

            using (var sourceStream = new FileStream(tmpPath,
                FileMode.OpenOrCreate, FileAccess.Write, FileShare.None,
                bufferSize: 4096, useAsync: true))
            {
                await sourceStream.WriteAsync(file, 0, file.Length);
            }

            var storageItem = new StorageItem
            {
                FileName = fileName,
                FileSize = file.LongLength,
                FilePath = destinationPath,
                PublicUri = new Uri($"{_options.PublicUri}/{destinationPath}")
            };

            TryProcessImage(storageItem, tmpPath);

            await DoSave(destinationPath, tmpPath);

            return storageItem;
        }

        protected abstract Task<bool> DoSave(string path, string tmpPath);


        public abstract Task<bool> DeleteFile(string filePath);

        protected string GetStorageFileName(string fileName)
        {
            var extension = fileName.Substring(fileName.LastIndexOf('.'));
            return Guid.NewGuid() + extension;
        }

        public StorageItem TryProcessImage(StorageItem storageItem, string filePath)
        {
            try
            {
                using (var image = Image.Load(filePath))
                {
                    storageItem.Type = StorageItemType.Picure;
                    storageItem.PictureInfo = new StorageItemPictureInfo
                    {
                        VerticalResolution = image.Height,
                        HorizontalResolution = image.Width
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"File is not image: {ex.Message}");
            }

            return storageItem;
        }
    }

    public class StorageOptions : IStorageOptions
    {
        public Uri PublicUri { get; set; }
    }
}