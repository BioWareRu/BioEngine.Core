using Microsoft.Extensions.Caching.Memory;
using Xunit;
using Xunit.Abstractions;

namespace BioEngine.Core.Tests
{
    public class CacheTests:CoreTest
    {
        public CacheTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public void TestCache()
        {
            var scope = GetScope();
            var cache = scope.Get<IMemoryCache>();
            var key = "foo";
            var value = "bar";
            cache.Set(key, value);

            var fromCache = cache.Get<string>(key);
            
            Assert.Equal(value, fromCache);
        }
    }
}
