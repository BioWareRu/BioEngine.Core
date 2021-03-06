﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BioEngine.Core.Entities;

namespace BioEngine.Core.Storage
{
    public interface IStorage
    {
        Task<IEnumerable<StorageNode>> ListItemsAsync(string path, string root = "/");

        Task<StorageItem> SaveFileAsync(Stream file, string fileName, string path, string root = "/");

        Task<bool> DeleteAsync(StorageItem item);
        Task<bool> DeleteAsync(string path, string root = "/");
        Task<bool> DeleteAsync(IEnumerable<StorageItem> items);
        
        void BeginBatch();
        Task FinishBatchAsync();
    }

    public interface IStorageOptions
    {
        Uri PublicUri { get; set; }
    }
}
