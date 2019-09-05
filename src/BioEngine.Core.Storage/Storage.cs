using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace BioEngine.Core.Storage
{
    public abstract class Storage : IStorage
    {
        private readonly StorageItemsRepository _repository;
        private readonly BioContext _dbContext;
        private readonly ILogger<Storage> _logger;
        private readonly StorageModuleConfig _options;
        private SHA256 Sha256 = SHA256.Create();

        protected Storage(StorageModuleConfig options,
            StorageItemsRepository repository,
            BioContext dbContext,
            ILogger<Storage> logger)
        {
            _repository = repository;
            _dbContext = dbContext;
            _logger = logger;
            _options = options;
        }

        public async Task<IEnumerable<StorageNode>> ListItemsAsync(string path, string root = "/")
        {
            var nodes = await GetNodesAsync(root);
            if (!nodes.Any())
            {
                return new List<StorageNode>();
            }

            var items = await GetNodesByPathAsync(nodes, $"{root}/{path}".Trim('/').Replace("//", "/"));
            return items.Select(i => new StorageNode(i, root)).OrderByDescending(i => i.IsDirectory)
                .ThenBy(i => i.Name);
        }

        private async Task<List<StorageNode>> GetNodesByPathAsync(List<StorageNode> nodes, string path)
        {
            var parts = path.Split('/');
            var currentNode = nodes.First();
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
                    currentNode = node;
                }
            }

            var nodesList = new List<StorageNode>();
            foreach (var folder in currentNode.Items)
            {
                nodesList.Add(new StorageNode(folder, path));
            }

            var items = await _dbContext.StorageItems.Where(s => s.Path == currentNode.Path).ToListAsync();
            foreach (var item in items)
            {
                nodesList.Add(new StorageNode(item));
            }

            return nodesList;
        }

        private async Task<List<StorageNode>> GetNodesAsync(string root)
        {
            var paths = await _dbContext.StorageItems.Where(s => s.Path.StartsWith(root)).Select(s => s.Path)
                .Distinct().ToArrayAsync();
            var rootNode = new StorageNode(root, root);
            foreach (var item in paths.OrderBy(s => s))
            {
                var parts = item.Split('/').Where(p => !string.IsNullOrEmpty(p)).ToArray();
                var currentRootNode = rootNode;
                foreach (var part in parts)
                {
                    if (part == parts.Last())
                    {
                        currentRootNode.Items.Add(new StorageNode(part, item));
                    }
                    else
                    {
                        var node = currentRootNode.Items.FirstOrDefault(i => i.Name == part);
                        if (node == null)
                        {
                            node = new StorageNode(part,
                                Path.Combine(currentRootNode.Path, part).Replace("\\", "/"));
                            currentRootNode.Items.Add(node);
                        }

                        currentRootNode = node;
                    }
                }
            }

            return rootNode.Items;
        }

        private static string HashBytesToString(byte[] bytes)
        {
            return bytes.Aggregate("", (current, b) => current + b.ToString("x2"));
        }

        [SuppressMessage("ReSharper", "RCS1198")]
        public async Task<StorageItem> SaveFileAsync(byte[] file, string fileName, string path, string root = "/")
        {
            var hash = HashBytesToString(Sha256.ComputeHash(file));
            var storageItem = await _repository.GetAsync(q => q.Where(i => i.Hash == hash));
            if (storageItem != null)
            {
                return storageItem;
            }

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


            storageItem = await _repository.NewAsync();
            storageItem.FileName = fileName;
            storageItem.FileSize = file.LongLength;
            storageItem.FilePath = destinationPath;
            storageItem.Path = Path.GetDirectoryName(destinationPath).Replace("\\", "/");
            storageItem.PublicUri = new Uri($"{_options.PublicUri}/{destinationPath}");
            storageItem.Hash = hash;

            await TryProcessImageAsync(storageItem, tmpPath, basePath);

            await DoSaveAsync(destinationPath, tmpPath);

            var result = await _repository.AddAsync(storageItem);
            if (!result.IsSuccess)
            {
                throw new Exception(result.ErrorsString);
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

            await _repository.DeleteAsync(item);

            return true;
        }

        public async Task<bool> DeleteAsync(IEnumerable<StorageItem> items)
        {
            _repository.BeginBatch();
            foreach (var item in items)
            {
                try
                {
                    await DeleteAsync(item);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Can't delete storage item {itemId}: {itemFilePath}: {errorMessage}",
                        item.Id,
                        item.FilePath, ex.ToString());
                }
            }

            await _repository.FinishBatchAsync();

            return true;
        }

        public void BeginBatch()
        {
            _repository.BeginBatch();
        }

        public async Task FinishBatchAsync()
        {
            await _repository.FinishBatchAsync();
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
                using (var image = Image.Load<Rgba32>(filePath))
                {
                    storageItem.Type = StorageItemType.Picture;
                    storageItem.PictureInfo = new StorageItemPictureInfo
                    {
                        VerticalResolution = image.Height,
                        HorizontalResolution = image.Width,
                        MediumThumbnail = await CreateThumbnailAsync(image,
                            _options.MediumThumbnailWidth,
                            _options.MediumThumbnailHeight, destinationPath, storageItem.StorageFileName),
                        SmallThumbnail = await CreateThumbnailAsync(image,
                            _options.SmallThumbnailWidth,
                            _options.SmallThumbnailHeight, destinationPath, storageItem.StorageFileName)
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("File is not image: {errorText}", ex.ToString());
            }
        }

        [SuppressMessage("ReSharper", "RCS1198")]
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

            return new StorageItemPictureThumbnail(new Uri($"{_options.PublicUri}/{thumbPath}"), thumbPath, thumb.Width,
                thumb.Height);
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
            Path = node.Path.Replace(root, "").Trim('/');
        }

        public string Name { get; }
        public string Path { get; }
        public bool IsDirectory { get; }
        public StorageItem? Item { get; }
        public List<StorageNode> Items { get; } = new List<StorageNode>();
    }
}
