using System;
using System.Collections.Generic;
using BioEngine.Core.Properties;

namespace BioEngine.Core.Interfaces
{
    public interface IEntity
    {
        List<PropertiesEntry> Properties { get; set; }

        Guid Id { get; set; }
        DateTimeOffset DateAdded { get; set; }
        DateTimeOffset DateUpdated { get; set; }
        bool IsPublished { get; set; }
        DateTimeOffset? DatePublished { get; set; }
    }

    public interface ISiteEntity
    {
        Guid Id { get; set; }
        Guid[] SiteIds { get; set; }
    }

    public interface ISingleSiteEntity
    {
        Guid Id { get; set; }
        Guid SiteId { get; set; }
    }

    public interface ISectionEntity
    {
        Guid Id { get; set; }
        Guid[] SectionIds { get; set; }
        Guid[] TagIds { get; set; }
    }

    public interface IContentEntity
    {
        Guid Id { get; set; }
        int AuthorId { get; set; }
        string Title { get; set; }
        string Url { get; set; }
        bool IsPinned { get; set; }
    }

    public interface ITypedEntity
    {
        string Type { get; set; }
        string TypeTitle { get; set; }
    }

    public interface ITypedEntity<T> : ITypedEntity where T : TypedData, new()
    {
        T Data { get; set; }
    }

    public abstract class TypedData
    {
    }
}
