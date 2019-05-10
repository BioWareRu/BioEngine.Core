using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace BioEngine.Core.Entities
{
    [Table("ContentBlocks")]
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
    public abstract class ContentBlock : BaseEntity, ITypedEntity

    {
        [NotMapped] public override string Title => TypeTitle;
        [NotMapped] public override string Url { get; set; }
        [Required] public Guid ContentId { get; set; }
        [Required] public string Type { get; set; }
        [Required] public int Position { get; set; }
        [NotMapped] public abstract string TypeTitle { get; set; }

        public abstract object GetData();
        public abstract void SetData(object data);

        [ForeignKey(nameof(ContentId))] public Post Post { get; set; }
        [ForeignKey(nameof(ContentId))] public Page Page { get; set; }

        [ForeignKey(nameof(ContentId))] public Section Section { get; set; }

        public override string ToString()
        {
            return GetType().Name;
        }
    }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
    public abstract class ContentBlock<T> : ContentBlock, ITypedEntity<T> where T : ContentBlockData, new()

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
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
    public abstract class ContentBlockData : ITypedData
    {
    }
}
