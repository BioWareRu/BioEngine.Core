using BioEngine.Core.Entities;
using BioEngine.Core.Interfaces;

namespace BioEngine.Core.Tests.Fixtures
{
    public class TestContent : ContentItem<TestContentData>
    {
        public override string TypeTitle { get; set; } = "Контент";
    }

    public class TestContentData : TypedData
    {
        public string Text { get; set; }
    }
}