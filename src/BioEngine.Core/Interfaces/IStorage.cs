using System;
using System.Threading.Tasks;
using BioEngine.Core.Storage;

namespace BioEngine.Core.Interfaces
{
    public interface IStorage
    {
        Task<StorageItem> SaveFileAsync(byte[] file, string fileName, string path);
        Task<bool> DeleteFileAsync(string filePath);
    }

    public interface IStorageOptions
    {
        Uri PublicUri { get; set; }
    }
}