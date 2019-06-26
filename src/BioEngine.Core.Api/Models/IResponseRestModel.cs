using System.Collections.Generic;
using System.Threading.Tasks;
using BioEngine.Core.Abstractions;

namespace BioEngine.Core.Api.Models
{
    public interface IResponseRestModel<in TEntity>
        where TEntity : class, IEntity
    {
        Task SetEntityAsync(TEntity entity);
    }

    public interface IContentResponseRestModel<TEntity> : IResponseRestModel<TEntity>
        where TEntity : class, IEntity, IContentEntity
    {
        List<Entities.ContentBlock> Blocks { get; set; }
    }
}
