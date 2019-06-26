using System;
using BioEngine.Core.Abstractions;

namespace BioEngine.Core.Api.Entities
{
    public class ContentItemVersionInfo
    {
        public ContentItemVersionInfo(Guid id, DateTimeOffset date, IUser user)
        {
            Id = id;
            Date = date;
            User = user;
        }

        public Guid Id { get; }
        public DateTimeOffset Date { get; }

        public IUser User { get; }
    }
}
