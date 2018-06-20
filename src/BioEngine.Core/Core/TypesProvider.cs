using System;
using System.Collections.Generic;
using System.Reflection;

namespace BioEngine.Core.Core
{
    public class TypesProvider
    {
        private readonly List<(Type type, int discriminator, Type dataType)> _sectionTypes =
            new List<(Type type, int discriminator, Type dataType)>();

        private readonly List<(Type type, int discriminator, Type dataType)> _contentTypes =
            new List<(Type type, int discriminator, Type dataType)>();

        public void AddSectionType(Type sectionType)
        {
            var attr = sectionType.GetCustomAttribute<TypedEntityAttribute>();
            if (attr == null)
            {
                throw new ArgumentException($"Section type without type attribute: {sectionType}");
            }

            var dataType = sectionType.BaseType?.GenericTypeArguments[0];

            if (dataType == null)
            {
                throw new ArgumentException($"Section type without data type: {sectionType}");
            }

            _sectionTypes.Add((sectionType, attr.Type, dataType));
        }

        public void AddContentType(Type contentType)
        {
            var attr = contentType.GetCustomAttribute<TypedEntityAttribute>();
            if (attr == null)
            {
                throw new ArgumentException($"Content type without type attribute: {contentType}");
            }

            var dataType = contentType.BaseType?.GenericTypeArguments[0];

            if (dataType == null)
            {
                throw new ArgumentException($"Content type without data type: {contentType}");
            }

            _contentTypes.Add((contentType, attr.Type, dataType));
        }

        public IEnumerable<(Type type, int discriminator, Type dataType)> GetSectionTypes()
        {
            return _sectionTypes;
        }

        public IEnumerable<(Type type, int discriminator, Type dataType)> GetContentTypes()
        {
            return _contentTypes;
        }
    }
}