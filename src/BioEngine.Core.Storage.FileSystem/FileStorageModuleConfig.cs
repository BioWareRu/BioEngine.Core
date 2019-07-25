using System;

namespace BioEngine.Core.Storage.FileSystem
{
    public class FileStorageModuleConfig : StorageModuleConfig
    {
        public string StoragePath { get; }

        public FileStorageModuleConfig(Uri publicUri, string storagePath) : base(publicUri)
        {
            StoragePath = storagePath;
        }
    }
}