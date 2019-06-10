using System;
using System.ComponentModel.DataAnnotations.Schema;
using BioEngine.Core.Abstractions;
using Newtonsoft.Json;

namespace BioEngine.Core.Entities
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
    [Table("ContentVersions")]public class ContentVersion : BaseEntity
    {
        [NotMapped] public override string Title { get; set; }
        [NotMapped] public override string Url { get; set; }
        public Guid ContentId { get; set; }
        [Column(TypeName = "jsonb")] public string Data { get; set; }

        public int ChangeAuthorId { get; set; }

        public void SetContent(ContentItem contentItem)
        {
            Data = JsonConvert.SerializeObject(contentItem,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore, TypeNameHandling = TypeNameHandling.Auto
                });
        }

        public ContentItem GetContent()
        {
            return JsonConvert.DeserializeObject<ContentItem>(Data,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead
                });
        }

        public T GetContent<T, TData>() where T : ContentItem<TData> where TData : ITypedData, new()
        {
            return JsonConvert.DeserializeObject<T>(Data,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead
                });
        }
    }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
}