using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Interfaces;
using BioEngine.Core.Tests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace BioEngine.Core.Tests
{
    public class SiteEntityTests : CoreTest
    {
        public SiteEntityTests(CoreTestFixture testFixture, ITestOutputHelper testOutputHelper) : base(testFixture,
            testOutputHelper)
        {
        }

        [Fact]
        public async Task SaveWithoutSiteIdsFails()
        {
            using (var context = CreateDbContext())
            {
                var repository = GetSectionsRepository(context);

                var section = new TestSection
                {
                    Title = "Test Section 2",
                    Url = "test2"
                };

                var result = await repository.Add(section);
                Assert.False(result.IsSuccess);
                Assert.True(result.Errors.Any(e => e.PropertyName == nameof(section.SiteIds)));
            }
        }
    }
}