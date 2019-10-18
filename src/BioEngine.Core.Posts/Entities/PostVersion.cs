using System;
using System.ComponentModel.DataAnnotations.Schema;
using BioEngine.Core.Abstractions;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using Newtonsoft.Json;

namespace BioEngine.Core.Posts.Entities
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
    [Table("PostVersions")]
    [Entity("postversion")]
    public class PostVersion<TUserPk> : BaseEntity
    {
        public Guid ContentId { get; set; }
        [Column(TypeName = "jsonb")] public string Data { get; set; }

        public TUserPk ChangeAuthorId { get; set; }

        public void SetContent(IContentItem contentItem)
        {
            Data = JsonConvert.SerializeObject(contentItem,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore, TypeNameHandling = TypeNameHandling.Auto
                });
        }

        public IContentItem GetContent()
        {
            return JsonConvert.DeserializeObject<IContentItem>(Data,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead
                });
        }

        public T GetContent<T>() where T : Post<TUserPk>
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
