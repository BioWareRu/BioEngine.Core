using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Entities;
using BioEngine.Core.Interfaces;
using BioEngine.Core.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace BioEngine.Core.Storage
{
    public abstract class Storage : IStorage
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<Storage> _logger;
        private readonly StorageOptions _options;
        private List<StorageNode> _nodes;

        protected Storage(IOptions<StorageOptions> options, IServiceProvider serviceProvider,
            ILogger<Storage> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _options = options.Value;
        }

        protected StorageItemsRepository Repository => _serviceProvider.CreateScope().ServiceProvider
            .GetRequiredService<StorageItemsRepository>();

        public async Task<IEnumerable<StorageNode>> ListItemsAsync(string path, string root = "/")
        {
            var nodes = await GetNodesAsync();
            var items = GetNodesByPath(nodes, $"{root}/{path}".Trim('/').Replace("//", "/"));
            return items.Select(i => new StorageNode(i, root)).OrderByDescending(i => i.IsDirectory)
                .ThenBy(i => i.Name);
        }

        private List<StorageNode> GetNodesByPath(List<StorageNode> nodes, string path)
        {
            var parts = path?.Split('/');
            var currentLevel = nodes;
            if (parts != null && parts.Length > 0)
            {
                foreach (var part in parts)
                {
                    if (string.IsNullOrEmpty(part)) continue;
                    var node = currentLevel.FirstOrDefault(n => n.Name == part);
                    if (node == null)
                    {
                        return new List<StorageNode>();
                    }

                    currentLevel = node.Items;
                }
            }

            return currentLevel;
        }

        private async Task<List<StorageNode>> GetNodesAsync()
        {
            if (_nodes == null)
            {
                await GenerateNodesAsync();
            }

            return _nodes;
        }

        private async Task GenerateNodesAsync()
        {
            _nodes = new List<StorageNode>();

            var items = await Repository.GetAllAsync();
            var rootNode = new StorageNode("/", "/");
            foreach (var item in items.items.OrderBy(s => s.FilePath))
            {
                var parts = item.FilePath.Split('/').Where(p => !string.IsNullOrEmpty(p));
                var currentRootNode = rootNode;
                foreach (var part in parts)
                {
                    if (part == parts.Last())
                    {
                        currentRootNode.Items.Add(new StorageNode(item));
                    }
                    else
                    {
                        var node = currentRootNode.Items.FirstOrDefault(i => i.Name == part);
                        if (node == null)
                        {
                            node = new StorageNode(part, $"{currentRootNode.Path}/{part}");
                            currentRootNode.Items.Add(node);
                        }

                        currentRootNode = node;
                    }
                }
            }

            _nodes = rootNode.Items;
        }

        public async Task<StorageItem> SaveFileAsync(byte[] file, string fileName, string path, string root = "/")
        {
            var destinationName = GetStorageFileName(fileName);
            path = $"{(root != "/" ? root : "")}/{path}".Replace("//", "/");
            var destinationPath = $"{path}/{destinationName}";

            var tmpPath = $"{Path.GetTempPath()}/{fileName.ToLowerInvariant()}";

            using (var sourceStream = new FileStream(tmpPath,
                FileMode.OpenOrCreate, FileAccess.Write, FileShare.None,
                4096, true))
            {
                await sourceStream.WriteAsync(file, 0, file.Length);
            }

            var storageItem = new StorageItem
            {
                FileName = fileName,
                FileSize = file.LongLength,
                FilePath = destinationPath,
                PublicUri = new Uri($"{_options.PublicUri}/{destinationPath}"),
                IsPublished = true
            };

            await TryProcessImageAsync(storageItem, tmpPath, path);

            await DoSaveAsync(destinationPath, tmpPath);

            var result = await Repository.AddAsync(storageItem);
            if (!result.IsSuccess)
            {
                throw new Exception(result.ErrorsString);
            }

            await GenerateNodesAsync();
            return storageItem;
        }

        public Task<bool> DeleteAsync(StorageItem item)
        {
            throw new NotImplementedException();
        }

        protected abstract Task<bool> DoSaveAsync(string path, string tmpPath);

        private string GetStorageFileName(string fileName)
        {
            var extension = fileName.Substring(fileName.LastIndexOf('.'));
            return Guid.NewGuid() + extension;
        }

        private async Task<StorageItem> TryProcessImageAsync(StorageItem storageItem, string filePath,
            string destinationPath)
        {
            try
            {
                using (var image = Image.Load(filePath))
                {
                    storageItem.Type = StorageItemType.Picture;
                    storageItem.PictureInfo = new StorageItemPictureInfo
                    {
                        VerticalResolution = image.Height,
                        HorizontalResolution = image.Width,
                        MediumThumbnail = await CreateThumbnailAsync(image, _options.MediumThumbnailWidth,
                            _options.SmallThumbnailHeight, destinationPath, storageItem.StorageFileName),
                        SmallThumbnail = await CreateThumbnailAsync(image, _options.SmallThumbnailWidth,
                            _options.SmallThumbnailHeight, destinationPath, storageItem.StorageFileName)
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"File is not image: {ex.Message}");
            }

            return storageItem;
        }

        private async Task<StorageItemPictureThumbnail> CreateThumbnailAsync(Image<Rgba32> image, int maxWidth,
            int maxHeight, string destinationPath, string fileName)
        {
            var thumb = image.Clone();
            thumb.Mutate(i =>
                i.Resize(image.Width >= image.Height ? maxWidth : 0, image.Height > image.Width ? maxHeight : 0));
            var thumbFileName = $"{thumb.Width}_{thumb.Height}_{fileName}";
            var tmpPath = $"{Path.GetTempPath()}/{thumbFileName}";
            thumb.Save(tmpPath);
            var thumbPath = $"{destinationPath}/thumb/{thumbFileName}";
            await DoSaveAsync(thumbPath, tmpPath);

            return new StorageItemPictureThumbnail
            {
                FilePath = thumbPath,
                PublicUri = new Uri($"{_options.PublicUri}/{thumbPath}"),
                Width = thumb.Width,
                Height = thumb.Height
            };
        }
    }


    public class StorageNode
    {
        public StorageNode(string name, string path)
        {
            Name = name;
            Path = path.Trim('/');
            IsDirectory = true;
            Item = null;
        }

        public StorageNode(StorageItem item)
        {
            Name = item.FileName;
            IsDirectory = false;
            Item = item;
            Path = item.FilePath;
        }

        public StorageNode(StorageNode node, string root = "/")
        {
            Name = node.Name;
            IsDirectory = node.IsDirectory;
            Item = node.Item;
            Path = node.Path.Replace(root, "/").Replace("//", "/");
        }

        public string Name { get; }
        public string Path { get; }
        public bool IsDirectory { get; }
        public StorageItem Item { get; }
        public List<StorageNode> Items { get; } = new List<StorageNode>();
    }

    public class StorageOptions : IStorageOptions
    {
        public Uri PublicUri { get; set; }
        public int MediumThumbnailWidth { get; set; } = 300;
        public int MediumThumbnailHeight { get; set; } = 300;
        public int SmallThumbnailWidth { get; set; } = 100;
        public int SmallThumbnailHeight { get; set; } = 100;
    }
}