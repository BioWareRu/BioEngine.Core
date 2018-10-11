using System;
using System.Collections.Generic;
using BioEngine.Core.Providers;

namespace BioEngine.Core.Interfaces
{
    public interface IEntity
    {
        object GetId();
    }

    public interface IEntity<TId> : IEntity
    {
        TId Id { get; set; }
        DateTimeOffset DateAdded { get; set; }
        DateTimeOffset DateUpdated { get; set; }
        bool IsPublished { get; set; }
        DateTimeOffset? DatePublished { get; set; }
        List<SettingsEntry> Settings { get; set; }
    }

    public interface ISiteEntity<TId>
    {
        TId Id { get; set; }
        int[] SiteIds { get; set; }
    }

    public interface ISectionEntity<TId>
    {
        TId Id { get; set; }
        int[] SectionIds { get; set; }
        int[] TagIds { get; set; }
    }

    public interface IContentEntity<TId>
    {
        TId Id { get; set; }
        int Type { get; set; }
        int AuthorId { get; set; }
        string Title { get; set; }
        string Url { get; set; }
        string Description { get; set; }
        bool IsPinned { get; set; }
    }

    public interface ITypedEntity
    {
        int Type { get; set; }
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