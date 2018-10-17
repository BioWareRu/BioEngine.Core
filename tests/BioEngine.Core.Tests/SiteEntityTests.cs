using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Tests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace BioEngine.Core.Tests
{
    public class SiteEntityTests : CoreTest
    {
        public SiteEntityTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public async Task SaveWithoutSiteIdsFails()
        {
            var context = CreateDbContext();
            var repository = GetSectionsRepository(context);

            var section = new TestSection
            {
                Title = "Test Section 2",
                Url = "test2"
            };

            var result = await repository.Add(section);
            Assert.False(result.IsSuccess);
            var error = result.Errors.FirstOrDefault(e => e.PropertyName == nameof(section.SiteIds));
            Assert.NotNull(error);
        }
    }
}