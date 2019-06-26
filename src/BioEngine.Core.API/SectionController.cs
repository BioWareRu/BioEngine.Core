using System.IO;
using System.Threading.Tasks;
using BioEngine.Core.Abstractions;
using BioEngine.Core.Api.Models;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Repository;
using BioEngine.Core.Web;
using Microsoft.AspNetCore.Mvc;

namespace BioEngine.Core.Api
{
    public abstract class
        SectionController<TEntity, TData, TRepository, TResponse, TRequest> : ContentEntityController<
            TEntity, TRepository,
            TResponse
            , TRequest>
        where TEntity : Section<TData>, IEntity
        where TData : ITypedData, new()
        where TResponse : class, IContentResponseRestModel<TEntity>
        where TRequest : SectionRestModel<TEntity>, IContentRequestRestModel<TEntity>
        where TRepository : IContentEntityRepository<TEntity>
    {
        protected SectionController(BaseControllerContext<TEntity, TRepository> context,
            BioEntitiesManager entitiesManager,
            ContentBlocksRepository blocksRepository) : base(context, entitiesManager, blocksRepository)
        {
        }


        public override async Task<ActionResult<StorageItem>> UploadAsync([FromQuery] string name)
        {
            var file = await GetBodyAsFileAsync();
            return await Storage.SaveFileAsync(file, name, Path.Combine("sections", GetUploadPath()));
        }

        protected abstract string GetUploadPath();
    }

    public abstract class
        SectionController<TEntity, TRepository, TResponse> : ResponseRestController<TEntity,
            TRepository, TResponse>
        where TEntity : Section, IEntity
        where TResponse : IResponseRestModel<TEntity>
        where TRepository : IBioRepository<TEntity>
    {
        protected SectionController(BaseControllerContext<TEntity, TRepository> context) : base(context)
        {
        }
    }
}
