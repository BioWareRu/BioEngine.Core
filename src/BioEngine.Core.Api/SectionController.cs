using System;
using System.IO;
using System.Threading.Tasks;
using BioEngine.Core.Abstractions;
using BioEngine.Core.Api.Models;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Repository;
using BioEngine.Core.Users;
using BioEngine.Core.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BioEngine.Core.Api
{
    [Authorize(Policy = BioPolicies.Sections)]
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

        [Authorize(Policy = BioPolicies.SectionsAdd)]
        public override Task<ActionResult<TResponse>> NewAsync()
        {
            return base.NewAsync();
        }

        [Authorize(Policy = BioPolicies.SectionsAdd)]
        public override Task<ActionResult<TResponse>> AddAsync(TRequest item)
        {
            return base.AddAsync(item);
        }

        [Authorize(Policy = BioPolicies.SectionsEdit)]
        public override Task<ActionResult<TResponse>> UpdateAsync(Guid id, TRequest item)
        {
            return base.UpdateAsync(id, item);
        }

        [Authorize(Policy = BioPolicies.SectionsDelete)]
        public override Task<ActionResult<TResponse>> DeleteAsync(Guid id)
        {
            return base.DeleteAsync(id);
        }

        [Authorize(Policy = BioPolicies.SectionsPublish)]
        public override Task<ActionResult<TResponse>> PublishAsync(Guid id)
        {
            return base.PublishAsync(id);
        }

        [Authorize(Policy = BioPolicies.SectionsPublish)]
        public override Task<ActionResult<TResponse>> HideAsync(Guid id)
        {
            return base.HideAsync(id);
        }
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
