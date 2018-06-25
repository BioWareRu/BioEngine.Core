using System;
using System.Threading.Tasks;
using BioEngine.Core.Storage;

namespace BioEngine.Core.Interfaces
{
    public interface IStorage
    {
        Task<StorageItem> SaveFile(byte[] file, string fileName, string path);
        Task<bool> DeleteFile(string filePath);
    }

    public interface IStorageOptions
    {
        Uri PublicUri { get; set; }
    }
}