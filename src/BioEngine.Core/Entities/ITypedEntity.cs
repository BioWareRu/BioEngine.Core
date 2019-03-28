using System;
using System.Collections.Generic;
using BioEngine.Core.Properties;

namespace BioEngine.Core.Entities
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

    public interface ISiteEntity : IEntity
    {
        Guid[] SiteIds { get; set; }
    }

    public interface ISingleSiteEntity : IEntity
    {
        Guid SiteId { get; set; }
    }

    public interface ISectionEntity : IEntity
    {
        Guid[] SectionIds { get; set; }
        Guid[] TagIds { get; set; }
    }

    public interface IContentEntity : IEntity
    {
    }

    public interface ITypedEntity
    {
        string TypeTitle { get; }
    }

    public interface ITypedEntity<T> : ITypedEntity where T : TypedData, new()
    {
        T Data { get; set; }
    }

    public abstract class TypedData
    {
    }
}
