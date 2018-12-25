using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BioEngine.Core.Storage;

namespace BioEngine.Core.Interfaces
{
    public interface IStorage
    {
        Task<IEnumerable<StorageItem>> ListItemsAsync(string path);
        Task<IEnumerable<string>> ListDirectoriesAsync(string path);

        Task CreateDirectoryAsync(string path);

        Task<StorageItem> SaveFileAsync(byte[] file, string fileName, string path);
        Task<bool> DeleteFileAsync(string filePath);
    }

    public interface IStorageOptions
    {
        Uri PublicUri { get; set; }
    }
}