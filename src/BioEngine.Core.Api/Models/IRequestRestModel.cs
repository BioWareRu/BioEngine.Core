using System.Collections.Generic;
using System.Threading.Tasks;
using BioEngine.Core.Abstractions;

namespace BioEngine.Core.Api.Models
{
    public interface IRequestRestModel<TEntity>
        where TEntity : class, IEntity
    {
        Task<TEntity> GetEntityAsync(TEntity entity);
    }

    public interface IContentRequestRestModel<TEntity> : IRequestRestModel<TEntity>
        where TEntity : class, IEntity, IContentEntity
    {
        List<Entities.ContentBlock> Blocks { get; set; }
    }
}
