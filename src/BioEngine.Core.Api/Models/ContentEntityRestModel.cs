using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Abstractions;
using BioEngine.Core.Api.Entities;
using BioEngine.Core.Repository;
using BioEngine.Core.Routing;
using Microsoft.AspNetCore.Routing;

namespace BioEngine.Core.Api.Models
{
    public class ContentEntityRestModel<TEntity> : SiteEntityRestModel<TEntity> where TEntity : class, IContentEntity
    {
        private readonly LinkGenerator _linkGenerator;
        private readonly SitesRepository _sitesRepository;
        public bool IsPublished { get; set; }
        public DateTimeOffset? DatePublished { get; set; }
        public List<PublicUrl> PublicUrls = new List<PublicUrl>();
        public List<ContentBlock> Blocks { get; set; }
        public string Url { get; set; }

        public ContentEntityRestModel(LinkGenerator linkGenerator, SitesRepository sitesRepository)
        {
            _linkGenerator = linkGenerator;
            _sitesRepository = sitesRepository;
        }

        protected override async Task ParseEntityAsync(TEntity entity)
        {
            await base.ParseEntityAsync(entity);
            IsPublished = entity.IsPublished;
            DatePublished = entity.DatePublished;
            var sites = await _sitesRepository.GetByIdsAsync(entity.SiteIds);
            Url = entity.Url;
            foreach (var site in sites)
            {
                PublicUrls.Add(new PublicUrl {Url = _linkGenerator.GeneratePublicUrl(entity, site), Site = site});
            }

            Blocks = entity.Blocks != null
                ? entity.Blocks.OrderBy(b => b.Position).Select(ContentBlock.Create).ToList()
                : new List<ContentBlock>();
        }

        protected override async Task<TEntity> FillEntityAsync(TEntity entity)
        {
            entity = await base.FillEntityAsync(entity);
            entity.Url = Url;
            return entity;
        }
    }

    public class PublicUrl
    {
        public Uri Url { get; set; }
        public Core.Entities.Site Site { get; set; }
    }
}
