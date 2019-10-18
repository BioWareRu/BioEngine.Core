using System;
using BioEngine.Core.Users;

namespace BioEngine.Core.Api.Entities
{
    public class ContentItemVersionInfo<TUserPk>
    {
        public ContentItemVersionInfo(Guid id, DateTimeOffset date, IUser<TUserPk> user)
        {
            Id = id;
            Date = date;
            User = user;
        }

        public Guid Id { get; }
        public DateTimeOffset Date { get; }

        public IUser<TUserPk> User { get; }
    }
}
