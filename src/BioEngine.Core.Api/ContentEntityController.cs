using System;
using System.Collections.Generic;
using System.Linq;
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
        ContentEntityController<TEntity, TRepository, TResponse, TRequest> : RequestRestController<
            TEntity, TRepository, TResponse, TRequest>
        where TEntity : class, IContentEntity, IBioEntity
        where TResponse : class, IContentResponseRestModel<TEntity>
        where TRequest : class, IContentRequestRestModel<TEntity>
        where TRepository : IContentEntityRepository<TEntity>
    {
        private readonly ContentBlocksRepository _blocksRepository;

        protected ContentEntityController(
            BaseControllerContext<TEntity, TRepository> context,
            ContentBlocksRepository blocksRepository) : base(context)
        {
            _blocksRepository = blocksRepository;
        }

        protected override Task<AddOrUpdateOperationResult<TEntity>> DoAddAsync(TEntity entity,
            IBioRepositoryOperationContext operationContext)
        {
            return Repository.AddWithBlocksAsync(entity, operationContext);
        }

        protected override Task<AddOrUpdateOperationResult<TEntity>> DoUpdateAsync(TEntity entity,
            IBioRepositoryOperationContext operationContext)
        {
            return Repository.UpdateWithBlocksAsync(entity, operationContext);
        }

        protected override Task<TEntity> DoGetByIdAsync(Guid id)
        {
            return Repository.GetByIdWithBlocksAsync(id);
        }

        private ContentBlock CreateBlock(string type)
        {
            return ModelBuilderExtensions.CreateBlock(type);
        }

        protected override async Task<TEntity> MapDomainModelAsync(TRequest restModel,
            TEntity domainModel = null)
        {
            domainModel = await base.MapDomainModelAsync(restModel, domainModel);

            domainModel.Blocks = new List<ContentBlock>();
            var dbBlocks = await _blocksRepository.GetByIdsAsync(restModel.Blocks.Select(b => b.Id).ToArray());
            foreach (var contentBlock in restModel.Blocks)
            {
                var block = dbBlocks.FirstOrDefault(b => b.Id == contentBlock.Id && b.ContentId == domainModel.Id);
                if (block == null)
                {
                    block = CreateBlock(contentBlock.Type);
                }

                if (block != null)
                {
                    block.Id = contentBlock.Id;
                    block.ContentId = domainModel.Id;
                    block.Position = contentBlock.Position;
                    block.SetData(contentBlock.Data);
                    block.DateUpdated = DateTimeOffset.UtcNow;
                    domainModel.Blocks.Add(block);
                }
            }

            return domainModel;
        }

        [HttpPost("publish/{id}")]
        public virtual async Task<ActionResult<TResponse>> PublishAsync(Guid id)
        {
            var entity = await Repository.GetByIdWithBlocksAsync(id);
            if (entity != null)
            {
                if (entity.IsPublished)
                {
                    return BadRequest();
                }

                await Repository.PublishAsync(entity, GetBioRepositoryOperationContext());
                await AfterSaveAsync(entity);
                return Model(await MapRestModelAsync(entity));
            }

            return NotFound();
        }

        [HttpPost("hide/{id}")]
        public virtual async Task<ActionResult<TResponse>> HideAsync(Guid id)
        {
            var entity = await Repository.GetByIdAsync(id);
            if (entity != null)
            {
                if (!entity.IsPublished)
                {
                    return BadRequest();
                }

                await Repository.UnPublishAsync(entity, GetBioRepositoryOperationContext());
                await AfterSaveAsync(entity);
                return Model(await MapRestModelAsync(entity));
            }

            return NotFound();
        }
    }
}
