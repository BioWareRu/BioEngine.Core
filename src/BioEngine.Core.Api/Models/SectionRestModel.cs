using System.Threading.Tasks;
using BioEngine.Core.Abstractions;
using BioEngine.Core.Entities;
using BioEngine.Core.Properties;
using BioEngine.Core.Repository;
using Microsoft.AspNetCore.Routing;

namespace BioEngine.Core.Api.Models
{
    public abstract class SectionRestModel<TEntity> : ContentEntityRestModel<TEntity>,
        IContentRequestRestModel<TEntity>
        where TEntity : Section, ISiteEntity, IEntity
    {
        public async Task<TEntity> GetEntityAsync(TEntity entity)
        {
            await FillEntityAsync(entity);
            entity.Title = Title;
            return entity;
        }

        protected SectionRestModel(LinkGenerator linkGenerator, SitesRepository sitesRepository,
            PropertiesProvider propertiesProvider) : base(linkGenerator,
            sitesRepository, propertiesProvider)
        {
        }
    }

    public abstract class SectionRestModel<TEntity, TData> : SectionRestModel<TEntity>
        where TEntity : Section, ITypedEntity<TData>, ISiteEntity, IEntity
        where TData : ITypedData, new()
    {
        public TData Data { get; set; }

        protected override async Task<TEntity> FillEntityAsync(TEntity entity)
        {
            entity = await base.FillEntityAsync(entity);
            entity.Data = Data;
            return entity;
        }

        protected SectionRestModel(LinkGenerator linkGenerator, SitesRepository sitesRepository,
            PropertiesProvider propertiesProvider) : base(linkGenerator,
            sitesRepository, propertiesProvider)
        {
        }
    }

    public abstract class ResponseSectionRestModel<TEntity> : SectionRestModel<TEntity>,
        IResponseRestModel<TEntity>
        where TEntity : Section, ISiteEntity, IEntity
    {
        public string Type { get; set; }
        public string TypeTitle { get; set; }

        public async Task SetEntityAsync(TEntity entity)
        {
            await ParseEntityAsync(entity);
        }

        protected override async Task ParseEntityAsync(TEntity entity)
        {
            await base.ParseEntityAsync(entity);
            Type = entity.Type;
            Title = entity.Title;
            if (entity is ITypedEntity typedEntity)
            {
                TypeTitle = typedEntity.TypeTitle;
            }
        }

        protected ResponseSectionRestModel(LinkGenerator linkGenerator, SitesRepository sitesRepository,
            PropertiesProvider propertiesProvider) : base(
            linkGenerator, sitesRepository, propertiesProvider)
        {
        }
    }

    public abstract class ResponseSectionRestModel<TEntity, TData> : SectionRestModel<TEntity>,
        IContentResponseRestModel<TEntity>
        where TEntity : Section, ISiteEntity, IEntity, ITypedEntity<TData>
        where TData : ITypedData, new()
    {
        public string Type { get; set; }
        public string TypeTitle { get; set; }
        public TData Data { get; set; }

        public async Task SetEntityAsync(TEntity entity)
        {
            await ParseEntityAsync(entity);
        }

        protected override async Task ParseEntityAsync(TEntity entity)
        {
            await base.ParseEntityAsync(entity);
            Title = entity.Title;
            Type = entity.Type;
            TypeTitle = entity.TypeTitle;

            Data = entity.Data;
        }

        protected ResponseSectionRestModel(LinkGenerator linkGenerator, SitesRepository sitesRepository,
            PropertiesProvider propertiesProvider) : base(
            linkGenerator, sitesRepository, propertiesProvider)
        {
        }
    }
}
