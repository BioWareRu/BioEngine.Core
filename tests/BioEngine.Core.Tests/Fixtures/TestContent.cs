using BioEngine.Core.Abstractions;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;

namespace BioEngine.Core.Tests.Fixtures
{
    [Entity("testcontentitem")]
    public class TestContent : ContentItem<TestContentData>
    {
        public override string PublicRouteName { get; set; }
        public override string TypeTitle { get; } = "TestContent";
    }

    public class TestContentData : ITypedData
    {
    }
}
