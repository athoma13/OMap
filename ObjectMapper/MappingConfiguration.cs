using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ObjectMapper
{
    public enum MapType
    {
        MapProperty,
        MapObject,
        MapCollection
    }


    public class MappingConfigurationEntry
    {
        public MapType MapType { get; private set; }
        public Type Source { get; private set; }
        public Type Target { get; private set; }
        public Delegate MappingAction { get; private set; }
        public Type DependencyTupleType { get; private set; }
        public IDictionary<Type, string> NamedResolutions { get; private set; }

        public MappingConfigurationEntry(Type source, Type target, Delegate mappingAction, Type dependencyTupleType, IDictionary<Type, string> namedResolutions, MapType mapType)
        {
            MapType = mapType;
            Source = source;
            Target = target;
            MappingAction = mappingAction;
            DependencyTupleType = dependencyTupleType;
            NamedResolutions = namedResolutions;
        }
    }

    public class MappingConfiguration
    {
        private readonly List<MappingConfigurationEntry> _entries;
        public ReadOnlyDictionary<Type, string> ResolutionNameDictionary { get; private set; }

        public MappingConfiguration(List<MappingConfigurationEntry> entries, IDictionary<Type, string> resolutionNames)
        {
            _entries = entries;
            ResolutionNameDictionary = new ReadOnlyDictionary<Type, string>(resolutionNames);
        }

        public MappingConfigurationEntry[] GetEntries()
        {
            return _entries.ToArray();
        }

        public MappingConfigurationEntry[] GetEntries(Type source, Type target)
        {
            var result = _entries.Where(x => source.IsAssignableFrom(x.Source) && target.IsAssignableFrom(x.Target)).ToArray();
            return result;
        }
    }
}