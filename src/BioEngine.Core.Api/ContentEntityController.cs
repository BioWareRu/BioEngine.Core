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
        protected BioEntitiesManager EntitiesManager { get; }

        protected ContentEntityController(
            BaseControllerContext<TEntity, TRepository> context,
            BioEntitiesManager entitiesManager, ContentBlocksRepository blocksRepository) : base(context)
        {
            _blocksRepository = blocksRepository;
            EntitiesManager = entitiesManager;
        }

        private ContentBlock CreateBlock(string type)
        {
            var blockType = EntitiesManager.GetBlocksMetadata().Where(entityMetadata =>
                    entityMetadata.Key == type &&
                    typeof(ContentBlock).IsAssignableFrom(entityMetadata.ObjectType))
                .Select(e => e.ObjectType).FirstOrDefault();
            if (blockType != null)
            {
                return Activator.CreateInstance(blockType) as ContentBlock;
            }

            return null;
        }

        protected override async Task<TEntity> MapDomainModelAsync(TRequest restModel,
            TEntity domainModel = null)
        {
            domainModel = await base.MapDomainModelAsync(restModel, domainModel);

            if (domainModel is ContentItem contentItemModel && string.IsNullOrEmpty(contentItemModel.AuthorId))
            {
                contentItemModel.AuthorId = CurrentUser.Id;
            }
            
            domainModel.Blocks = new List<ContentBlock>();
            var dbBlocks = await _blocksRepository.GetByIdsAsync(restModel.Blocks.Select(b => b.Id).ToArray());
            _blocksRepository.BeginBatch();
            foreach (var contentBlock in restModel.Blocks)
            {
                var block = dbBlocks.FirstOrDefault(b => b.Id == contentBlock.Id && b.ContentId == domainModel.Id);
                if (block == null)
                {
                    block = CreateBlock(contentBlock.Type);
                    await _blocksRepository.AddAsync(block);
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
            var entity = await Repository.GetByIdAsync(id);
            if (entity != null)
            {
                if (entity.IsPublished)
                {
                    return BadRequest();
                }

                await Repository.PublishAsync(entity, new BioRepositoryOperationContext {User = CurrentUser});
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

                await Repository.UnPublishAsync(entity, new BioRepositoryOperationContext {User = CurrentUser});
                await AfterSaveAsync(entity);
                return Model(await MapRestModelAsync(entity));
            }

            return NotFound();
        }
    }
}
