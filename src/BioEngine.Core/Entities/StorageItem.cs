using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BioEngine.Core.Entities
{
    [Table("StorageItems")]
    public class StorageItem : BaseEntity
    {
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public Uri PublicUri { get; set; }
        public string FilePath { get; set; }
        public StorageItemType Type { get; set; } = StorageItemType.Other;
        public StorageItemPictureInfo PictureInfo { get; set; }

        public string StorageFileName => FilePath.Substring(FilePath.LastIndexOf('/') + 1);
    }

    public enum StorageItemType
    {
        Picture = 1,
        Other = 2
    }

    public class StorageItemPictureInfo
    {
        public double VerticalResolution { get; set; }
        public double HorizontalResolution { get; set; }

        public StorageItemPictureThumbnail MediumThumbnail { get; set; }
        public StorageItemPictureThumbnail SmallThumbnail { get; set; }
    }

    public class StorageItemPictureThumbnail
    {
        public Uri PublicUri { get; set; }
        public string FilePath { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }
    }
}
