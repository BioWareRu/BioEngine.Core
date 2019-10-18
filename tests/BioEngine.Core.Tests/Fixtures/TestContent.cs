using System;
using System.Collections.Generic;
using BioEngine.Core.Abstractions;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;

namespace BioEngine.Core.Tests.Fixtures
{
    [Entity("testcontentitem")]
    public class TestContent : BaseEntity, IContentItem
    {
        public string PublicRouteName { get; set; }
        public string Url { get; set; }
        public Guid[] SiteIds { get; set; } = new Guid[0];
        public string Title { get; set; }
        public List<ContentBlock> Blocks { get; set; } = new List<ContentBlock>();
        public bool IsPublished { get; set; }
        public DateTimeOffset? DatePublished { get; set; }
        public Guid[] SectionIds { get; set; } = new Guid[0];
        public Guid[] TagIds { get; set; } = new Guid[0];
        public List<Section> Sections { get; set; }
        public List<Tag> Tags { get; set; }
    }
}
