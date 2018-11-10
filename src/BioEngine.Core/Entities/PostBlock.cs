using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BioEngine.Core.Interfaces;
using Newtonsoft.Json;

namespace BioEngine.Core.Entities
{
    [Table("PostBlocks")]
    public abstract class PostBlock : ITypedEntity
    {
        [Key] public int Id { get; set; }
        [Required] private int PostId { get; set; }
        [Required] public string Type { get; set; }
        [NotMapped]
        public abstract string TypeTitle { get; set; }

        [ForeignKey(nameof(PostId))]
        public Post Post { get; set; }

        public abstract object GetData();
        public abstract void SetData(object data);
    }

    public abstract class PostBlock<T> : PostBlock, ITypedEntity<T> where T : ContentBlockData, new()
    {
        public T Data { get; set; }

        public override object GetData()
        {
            return Data;
        }

        public override void SetData(object data)
        {
            try
            {
                var typedData = JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(data));
                if (typedData != null)
                {
                    Data = typedData;
                }
                else
                {
                    throw new Exception($"Can't assign empty data to type {typeof(T)}");
                }
            }
            catch (JsonException)
            {
                throw new Exception($"Can't convert object {data} to type {typeof(T)}");
            }
        }
    }

    public abstract class ContentBlockData : TypedData
    {
    }
}