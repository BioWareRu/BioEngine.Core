using BioEngine.Core.Entities;
using BioEngine.Core.Interfaces;

namespace BioEngine.Core.Tests.Fixtures
{
    [TypedEntity(1)]
    public class TestSection : Section<TestSectionData>
    {
        public override string TypeTitle { get; set; } = "Раздел";
    }

    public class TestSectionData : TypedData
    {
    }
}