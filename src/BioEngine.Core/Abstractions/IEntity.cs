using System;

namespace BioEngine.Core.Abstractions
{
    public interface IEntity
    {
        string GetId();
    }

    public interface IEntity<TKey> : IEntity
    {
        TKey Id { get; set; }
    }

    public interface IBioEntity : IEntity<Guid>
    {
        string Title { get; set; }
        DateTimeOffset DateAdded { get; set; }
        DateTimeOffset DateUpdated { get; set; }
    }
}
