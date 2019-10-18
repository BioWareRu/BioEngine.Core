using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Posts.Db;
using BioEngine.Core.Posts.Entities;
using BioEngine.Core.Repository;
using BioEngine.Core.Search;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace BioEngine.Core.Posts.Search
{
    [UsedImplicitly]
    public class PostsSearchProvider<TUserPk> : BaseSearchProvider<Post<TUserPk>>
    {
        private readonly TagsRepository _tagsRepository;
        private readonly PostsRepository<TUserPk> _postsRepository;

        public PostsSearchProvider(ILogger<PostsSearchProvider<TUserPk>> logger,
            TagsRepository tagsRepository, PostsRepository<TUserPk> postsRepository,
            ISearcher searcher = null) : base(logger, searcher)
        {
            _tagsRepository = tagsRepository;
            _postsRepository = postsRepository;
        }

        protected override async Task<SearchModel[]> GetSearchModelsAsync(Post<TUserPk>[] entities)
        {
            var tagIds = entities.SelectMany(e => e.TagIds).Distinct().ToArray();
            var tags = await _tagsRepository.GetByIdsAsync(tagIds);
            return entities.Select(post =>
            {
                var model =
                    new SearchModel(post.Id, post.Title, post.Url,
                        string.Join(" ", post.Blocks.Select(b => b.ToString()).Where(s => !string.IsNullOrEmpty(s))),
                        post.DateAdded)
                    {
                        SectionIds = post.SectionIds,
                        AuthorId = post.AuthorId.ToString(),
                        SiteIds = post.SiteIds,
                        Tags = tags.Where(t => post.TagIds.Contains(t.Id)).Select(t => t.Title).ToArray()
                    };

                return model;
            }).ToArray();
        }

        protected override Task<Post<TUserPk>[]> GetEntitiesAsync(SearchModel[] searchModels)
        {
            var ids = searchModels.Select(s => s.Id).Distinct().ToArray();
            return _postsRepository.GetByIdsAsync(ids);
        }
    }
}
