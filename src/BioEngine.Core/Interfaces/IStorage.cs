using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BioEngine.Core.Entities;
using BioEngine.Core.Storage;

namespace BioEngine.Core.Interfaces
{
    public interface IStorage
    {
        Task<IEnumerable<StorageNode>> ListItemsAsync(string path, string root = "/");
        Task<StorageItem> SaveFileAsync(byte[] file, string fileName, string path, string root = "/");
        Task<bool> DeleteAsync(StorageItem item);
        Task<bool> DeleteAsync(IEnumerable<StorageItem> items);
    }

    public interface IStorageOptions
    {
        Uri PublicUri { get; set; }
    }
}
