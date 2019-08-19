using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Entities;
using BioEngine.Core.Entities.Blocks;
using BioEngine.Core.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace BioEngine.Core.Tests
{
    public class BlocksTests : CoreTest
    {
        public BlocksTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public async Task AddBlocksAsync()
        {
            var scope = GetScope();
            var dbContext = scope.GetDbContext();
            var contentRepository = scope.Get<TestContentRepository>();
            var section = await dbContext.Sections.FirstOrDefaultAsync();

            var content = new TestContent
            {
                Id = Guid.NewGuid(),
                Title = "Test blocks",
                Url = "testblocks",
                DatePublished = DateTimeOffset.Now,
                IsPublished = true,
                SiteIds = section.SiteIds,
                SectionIds = new[] {section.Id},
                AuthorId = "0",
            };

            var blocks = new List<ContentBlock>();
            var textBlock = new TextBlock
            {
                Id = Guid.NewGuid(),
                Position = 0,
                Data = new TextBlockData {Text = "Blaa"}
            };
            blocks.Add(textBlock);

            content.Blocks = blocks;

            var result = await contentRepository.AddAsync(content);
            Assert.True(result.IsSuccess);

            var count = await dbContext.Blocks
                .Where(b => b.ContentId == content.Id).CountAsync();
            Assert.Equal(0, count);

            await contentRepository.DeleteAsync(content);

            var addWithBlocksResult = await contentRepository.AddWithBlocksAsync(content);
            Assert.True(addWithBlocksResult.IsSuccess);

            count = await dbContext.Blocks
                .Where(b => b.ContentId == content.Id).CountAsync();
            Assert.Equal(1, count);
        }
    }
}
