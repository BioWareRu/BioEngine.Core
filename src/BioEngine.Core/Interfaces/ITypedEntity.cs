using System;

namespace BioEngine.Core.Interfaces
{
    public interface IEntity<TId>
    {
        TId Id { get; set; }
        DateTimeOffset DateAdded { get; set; }
        DateTimeOffset DateUpdated { get; set; }
        bool IsPublished { get; set; }
        DateTimeOffset? DatePublished { get; set; }
    }

    public interface ISiteEntity
    {
        int[] SiteIds { get; set; }
    }

    public interface ISectionEntity
    {
        int[] SectionIds { get; set; }
    }

    public interface ITypedEntity
    {
        
    }

    public interface ITypedEntity<T> : ITypedEntity where T : TypedData, new()
    {
        T Data { get; set; }
    }

    public abstract class TypedData
    {
    }
}