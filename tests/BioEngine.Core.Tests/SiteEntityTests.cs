using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using BioEngine.Core.Tests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace BioEngine.Core.Tests
{
    [SuppressMessage("ReSharper", "VSTHRD200")]
    public class SiteEntityTests : CoreTest
    {
        public SiteEntityTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public async Task SiteIdsAutoFill()
        {
            var scope = GetScope();
            var repository = scope.Get<TestSectionRepository>();

            var section = new TestSection
            {
                Title = "Test Section 2",
                Url = "test2"
            };

            var result = await repository.AddAsync(section);
            Assert.True(result.IsSuccess);
            Assert.Single(section.SiteIds);
        }
    }
}
