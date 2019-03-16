using System;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace BioEngine.Core.Entities
{
    public class PostVersion : BaseEntity
    {
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
}
