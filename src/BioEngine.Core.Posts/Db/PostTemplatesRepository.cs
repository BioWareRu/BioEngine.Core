using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BioEngine.Core.Entities;
using BioEngine.Core.Posts.Entities;
using BioEngine.Core.Repository;

namespace BioEngine.Core.Posts.Db
{
    public class PostTemplatesRepository<TUserPk> : BioRepository<PostTemplate<TUserPk>>
    {
        public PostTemplatesRepository(BioRepositoryContext<PostTemplate<TUserPk>> repositoryContext) : base(
            repositoryContext)
        {
        }

        public Task<(PostTemplate<TUserPk>[] items, int itemsCount)> GetTemplatesAsync()
        {
            return GetAllAsync();
        }

        public async Task<PostTemplate<TUserPk>> CreateTemplateAsync(Post<TUserPk> content)
        {
            var template = new PostTemplate<TUserPk>
            {
                Title = content.Title,
                AuthorId = content.AuthorId,
                Data = new PostTemplateData {Blocks = content.Blocks, Title = content.Title, Url = content.Url,},
                TagIds = content.TagIds,
                SectionIds = content.SectionIds,
            };

            var result = await AddAsync(template);
            if (!result.IsSuccess)
            {
                throw new Exception(result.ErrorsString);
            }

            return template;
        }

        public async Task<Post<TUserPk>> CreateFromTemplateAsync(Guid templateId)
        {
            var template = await GetByIdAsync(templateId);
            if (template == null)
            {
                throw new ArgumentException(nameof(template));
            }


            var content = Activator.CreateInstance<Post<TUserPk>>();
            content.Blocks = new List<ContentBlock>();
            foreach (var contentBlock in template.Data.Blocks)
            {
                contentBlock.Id = Guid.NewGuid();
                contentBlock.ContentId = Guid.Empty;
                content.Blocks.Add(contentBlock);
            }

            content.Url = template.Data.Url;
            content.Title = template.Data.Title;
            content.TagIds = template.TagIds;
            content.SectionIds = template.SectionIds;
            content.DateAdded = DateTimeOffset.UtcNow;
            content.DateUpdated = DateTimeOffset.UtcNow;

            return content;
        }
    }
}
