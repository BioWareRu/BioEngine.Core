using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Entities;
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
        private bool _batchMode;

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
                            node = new StorageNode(part, Path.Combine(currentRootNode.Path, part).Replace("\\", "/"));
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
            if (path.StartsWith("/")) path = path.Substring(1);
            var basePath = path;
            if (root != "/")
            {
                basePath = path != "/" ? Path.Combine(root, path) : root;
            }

            var destinationPath = Path.Combine(basePath, destinationName).Replace("\\", "/");
            if (destinationPath.StartsWith("/")) destinationPath = destinationPath.Substring(1);
            var tmpPath = Path.Combine(Path.GetTempPath(), fileName.ToLowerInvariant());

            using (var sourceStream = new FileStream(tmpPath,
                FileMode.OpenOrCreate, FileAccess.Write, FileShare.None,
                4096, true))
            {
                await sourceStream.WriteAsync(file, 0, file.Length);
            }

            var storageItem = await Repository.NewAsync();
            storageItem.FileName = fileName;
            storageItem.FileSize = file.LongLength;
            storageItem.FilePath = destinationPath;
            storageItem.PublicUri = new Uri($"{_options.PublicUri}/{destinationPath}");
            storageItem.IsPublished = true;

            await TryProcessImageAsync(storageItem, tmpPath, basePath);

            await DoSaveAsync(destinationPath, tmpPath);

            var result = await Repository.AddAsync(storageItem);
            if (!result.IsSuccess)
            {
                throw new Exception(result.ErrorsString);
            }

            if (!_batchMode)
            {
                await GenerateNodesAsync();
            }

            return storageItem;
        }

        public async Task<bool> DeleteAsync(StorageItem item)
        {
            // TODO: Cleanup usages

            await DoDeleteAsync(item.FilePath);
            if (item.Type == StorageItemType.Picture && item.PictureInfo != null)
            {
                if (item.PictureInfo.SmallThumbnail != null)
                {
                    await DoDeleteAsync(item.PictureInfo.SmallThumbnail.FilePath);
                }

                if (item.PictureInfo.MediumThumbnail != null)
                {
                    await DoDeleteAsync(item.PictureInfo.MediumThumbnail.FilePath);
                }
            }

            await Repository.DeleteAsync(item);
            return true;
        }

        public async Task<bool> DeleteAsync(IEnumerable<StorageItem> items)
        {
            Repository.BeginBatch();
            foreach (var item in items)
            {
                try
                {
                    await DeleteAsync(item);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Can't delete storage item {itemId}: {itemFilePath}: {errorMessage}", item.Id,
                        item.FilePath, ex.ToString());
                }
            }

            await Repository.FinishBatchAsync();
            return true;
        }

        public void BeginBatch()
        {
            _batchMode = true;
            Repository.BeginBatch();
        }

        public async Task FinishBatchAsync()
        {
            _batchMode = false;
            await GenerateNodesAsync();
            await Repository.FinishBatchAsync();
        }

        protected abstract Task<bool> DoSaveAsync(string path, string tmpPath);
        protected abstract Task<bool> DoDeleteAsync(string path);

        private string GetStorageFileName(string fileName)
        {
            var extension = fileName.Substring(fileName.LastIndexOf('.'));
            return $"{Guid.NewGuid().ToString()}{extension}";
        }

        private async Task TryProcessImageAsync(StorageItem storageItem, string filePath,
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
                _logger.LogInformation("File is not image: {errorText}", ex.ToString());
            }
        }

        private async Task<StorageItemPictureThumbnail> CreateThumbnailAsync(Image<Rgba32> image, int maxWidth,
            int maxHeight, string destinationPath, string fileName)
        {
            var thumb = image.Clone();
            thumb.Mutate(i =>
                i.Resize(image.Width >= image.Height ? maxWidth : 0, image.Height > image.Width ? maxHeight : 0));
            var thumbFileName = $"{thumb.Width.ToString()}_{thumb.Height.ToString()}_{fileName}";
            var tmpPath = Path.Combine(Path.GetTempPath(), thumbFileName);
            thumb.Save(tmpPath);
            var thumbPath = Path.Combine(destinationPath, "thumb", thumbFileName).Replace("\\", "/");
            if (thumbPath.StartsWith("/")) thumbPath = thumbPath.Substring(1);
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
