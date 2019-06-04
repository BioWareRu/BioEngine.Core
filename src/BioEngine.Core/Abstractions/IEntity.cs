using System;
using System.Collections.Generic;
using BioEngine.Core.Properties;

namespace BioEngine.Core.Abstractions
{
    public interface IEntity
    {
        List<PropertiesEntry> Properties { get; set; }

        Guid Id { get; set; }
        string Title { get; set; }
        string Url { get; set; }
        DateTimeOffset DateAdded { get; set; }
        DateTimeOffset DateUpdated { get; set; }
    }
}
