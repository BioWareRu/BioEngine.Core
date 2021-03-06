﻿using System;
using System.ComponentModel.DataAnnotations.Schema;
using BioEngine.Core.DB;
using JetBrains.Annotations;

namespace BioEngine.Core.Entities
{
    [Table("StorageItems")]
    [Entity("storageitem")]
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
    public class StorageItem : BaseEntity
    {
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public Uri PublicUri { get; set; }
        public string FilePath { get; set; }
        public string Path { get; set; }
        public StorageItemType Type { get; set; } = StorageItemType.Other;
        [Column(TypeName = "jsonb")]
        public StorageItemPictureInfo? PictureInfo { get; set; }
        public string? Hash { get; set; }

        public string StorageFileName => FilePath.Substring(FilePath.LastIndexOf('/') + 1);
        private readonly string[] _units = {"bytes", "KB", "MB", "GB", "TB", "PB"};

        public string HumanSize
        {
            get
            {
                if (FileSize < 1)
                {
                    return "-";
                }

                var unit = 0;

                double size = FileSize;
                while (size >= 1024)
                {
                    size /= 1024;
                    unit++;
                }

                return Math.Round(size, 2) + ' ' + _units[unit];
            }
        }
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

        public StorageItemPictureThumbnail? LargeThumbnail { get; set; }
        public StorageItemPictureThumbnail? MediumThumbnail { get; set; }
        public StorageItemPictureThumbnail? SmallThumbnail { get; set; }
    }

    public class StorageItemPictureThumbnail
    {
        public Uri PublicUri { get; set; }
        public string FilePath { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

        [UsedImplicitly]
        public StorageItemPictureThumbnail()
        {
        }

        public StorageItemPictureThumbnail(Uri publicUri, string filePath, int width, int height)
        {
            PublicUri = publicUri;
            FilePath = filePath;
            Width = width;
            Height = height;
        }
    }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
}
