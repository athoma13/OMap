﻿using System;
using System.Collections.Generic;

namespace OMap
{
    public enum MapType
    {
        MapProperty,
        MapObject,
        MapCollection,
        MapFunction,
        MapAll,
        MapIgnore
    }

    public class MappingConfigurationEntry
    {
        public Type Source { get; private set; }
        public Type Target { get; private set; }
        public string EntryDescription { get; private set; }

        public MappingConfigurationEntry(Type source, Type target, string entryDescription)
        {
            Source = source;
            Target = target;
            EntryDescription = entryDescription;
        }
    }

    public class MappingConfigurationPropertyEntry : MappingConfigurationEntry
    {
        public Delegate MappingAction { get; private set; }
        public Type DependencyTupleType { get; private set; }
        public IDictionary<Type, string> NamedResolutions { get; private set; }

        public MappingConfigurationPropertyEntry(Type source, Type target, string entryDescription, Delegate mappingAction, Type dependencyTupleType, IDictionary<Type, string> namedResolutions)
            :base(source, target, entryDescription)
        {
            MappingAction = mappingAction;
            DependencyTupleType = dependencyTupleType;
            NamedResolutions = namedResolutions;
        }
    }

    public class MappingConfigurationObjectEntry : MappingConfigurationEntry
    {
        public Delegate GetSourceProperty { get; set; }
        public Delegate GetTargetProperty { get; set; }
        public Delegate SetTargetProperty { get; set; }
        public Type TargetPropertyType { get; set; }

        public MappingConfigurationObjectEntry(Type source, Type target, string entryDescription, Delegate getSourceProperty, Delegate getTargetProperty, Delegate setTargetProperty, Type targetPropertyType)
            : base(source, target, entryDescription)
        {
            GetSourceProperty = getSourceProperty;
            GetTargetProperty = getTargetProperty;
            SetTargetProperty = setTargetProperty;
            TargetPropertyType = targetPropertyType;
        }
    }

    public class MappingConfigurationCollectionEntry : MappingConfigurationEntry
    {
        public Delegate GetSourceProperty { get; set; }
        public Delegate GetTargetProperty { get; set; }
        public Delegate SetTargetProperty { get; set; }
        public Type TargetPropertyType { get; set; }

        public MappingConfigurationCollectionEntry(Type source, Type target, string entryDescription, Delegate getSourceProperty, Delegate getTargetProperty, Delegate setTargetProperty, Type targetPropertyType)
            : base(source, target, entryDescription)
        {
            GetSourceProperty = getSourceProperty;
            GetTargetProperty = getTargetProperty;
            SetTargetProperty = setTargetProperty;
            TargetPropertyType = targetPropertyType;
        }
    }

    public class MappingConfigurationFunctionEntry : MappingConfigurationEntry
    {
        public Delegate Action { get; private set; }
        public Type DependencyTupleType { get; private set; }
        public IDictionary<Type, string> NamedResolutions { get; private set; }

        public MappingConfigurationFunctionEntry(Type source, Type target, string entryDescription, Delegate action, Type dependencyTupleType, IDictionary<Type, string> namedResolutions)
            : base(source, target, entryDescription)
        {
            Action = action;
            DependencyTupleType = dependencyTupleType;
            NamedResolutions = namedResolutions;
        }
    }


    public class MappingConfiguration
    {
        private readonly List<MappingConfigurationEntry> _entries;

        public MappingConfiguration(List<MappingConfigurationEntry> entries)
        {
            _entries = entries;
        }

        public MappingConfigurationEntry[] GetEntries()
        {
            return _entries.ToArray();
        }
    }
}