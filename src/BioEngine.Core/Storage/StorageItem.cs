using System;

namespace BioEngine.Core.Storage
{
    public class StorageItem
    {
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public Uri PublicUri { get; set; }
        public string FilePath { get; set; }
        public StorageItemType Type { get; set; } = StorageItemType.Other;
        public StorageItemPictureInfo PictureInfo { get; set; } = null;
    }

    public enum StorageItemType
    {
        Picure = 1,
        Other = 2
    }

    public class StorageItemPictureInfo
    {
        public double VerticalResolution { get; set; }
        public double HorizontalResolution { get; set; }
    }
}