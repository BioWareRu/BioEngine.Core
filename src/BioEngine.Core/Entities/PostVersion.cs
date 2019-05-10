using System;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace BioEngine.Core.Entities
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
    public class PostVersion : BaseEntity
    {
        [NotMapped] public override string Title { get; set; }
        [NotMapped] public override string Url { get; set; }
        public Guid PostId { get; set; }
        [Column(TypeName = "jsonb")] public string Data { get; set; }

        public int ChangeAuthorId { get; set; }

        public void SetPost(Post post)
        {
            Data = JsonConvert.SerializeObject(post,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore, TypeNameHandling = TypeNameHandling.Auto
                });
        }

        public Post GetPost()
        {
            return JsonConvert.DeserializeObject<Post>(Data,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead
                });
        }
    }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
}
