using System;
using System.Collections.Generic;
using System.Linq;

namespace ObjectMapper
{
    public interface IMapper
    {
        TTarget Map<TTarget>(object source);
        void Map(object source, object target);
    }

    public class ObjectMapper : IMapper
    {
        private readonly IDependencyResolver _resolver;
        private readonly MappingConfigurationEntry[] _entries;

        public ObjectMapper(IMappingConfigurationProvider mappingConfigurationProvider, IDependencyResolver resolver)
        {
            _resolver = resolver;
            _entries = mappingConfigurationProvider.GetConfiguration().GetEntries().ToArray();
        }

        public void Map(object source, object target)
        {
            Map(source, target, new MappingContext(_resolver));
        }

        private void Map(object source, object target, MappingContext context)
        {
            MapInternal(source, target, GetApplicableEntries(source.GetType(), target.GetType(), true), context);
        }

        private void MapInternal(object source, object target, MappingConfigurationEntry[] entries, MappingContext context)
        {
            foreach (var entry in entries)
            {
                var propertyEntry = entry as MappingConfigurationPropertyEntry;
                if (propertyEntry != null)
                {
                    object dependencyTuple = null;
                    if (propertyEntry.DependencyTupleType != null) dependencyTuple = GetDependencyTuple(context, propertyEntry);

                    if (dependencyTuple != null)
                    {
                        propertyEntry.MappingAction.DynamicInvoke(source, target, dependencyTuple);
                    }
                    else
                    {
                        propertyEntry.MappingAction.DynamicInvoke(source, target);
                    }
                    continue;
                }

                var objectEntry = entry as MappingConfigurationObjectEntry;
                if (objectEntry != null)
                {
                    var sourceObject = objectEntry.GetSourceProperty.DynamicInvoke(source);
                    var targetObject = objectEntry.GetTargetProperty.DynamicInvoke(target);
                    if (targetObject == null)
                    {
                        var newObject = Map(sourceObject, objectEntry.TargetPropertyType, context);
                        objectEntry.SetTargetProperty.DynamicInvoke(target, newObject);
                    }
                    else
                    {
                        Map(sourceObject, targetObject, context);
                    }
                }
            }
        }

        public TTarget Map<TTarget>(object source)
        {
            var obj = Map(source, typeof(TTarget), new MappingContext(_resolver));
            return obj == null ? default(TTarget) : (TTarget)obj;
        }


        private object Map(object source, Type requestedTargetType, MappingContext context)
        {
            if (source == null) return null;

            var sourceType = source.GetType();
            var applicableEntries = GetApplicableEntries(sourceType, requestedTargetType, false);

            var targetTypes = applicableEntries.Where(x => x.Source == sourceType).Select(x => x.Target).Distinct().ToArray();
            if (targetTypes.Length > 1) throw new InvalidOperationException(string.Format("Cannot map from {0} to {1}. Cannot Decide what the target type should be. Options are: {2}", sourceType, requestedTargetType, string.Join(", ", targetTypes.Cast<object>().ToArray())));
            if (targetTypes.Length == 1)
            {
                var result = Activator.CreateInstance(targetTypes[0]);
                MapInternal(source, result, applicableEntries, context);
                return result;
            }

            targetTypes = applicableEntries.Select(x => x.Target).Distinct().ToArray();
            if (targetTypes.Length == 0) throw new InvalidOperationException(string.Format("Cannot map from {0} to {1}. No target types found", sourceType, requestedTargetType));
            if (targetTypes.Length != 1) throw new InvalidOperationException(string.Format("Cannot map from {0} to {1}. Cannot Decide what the target type should be. Options are: {2}", sourceType, requestedTargetType, string.Join(", ", targetTypes.Cast<object>().ToArray())));
            var result1 = Activator.CreateInstance(targetTypes[0]);
            MapInternal(source, result1, applicableEntries, context);
            return result1;
        }

        private static object GetDependencyTuple(MappingContext context, MappingConfigurationPropertyEntry entry)
        {
            if (entry.DependencyTupleType == null) return null;

            var dependencies = new List<object>();
            //dependencies are passed as a Tuple into the mapping function... find all generic arguments to determine dependencies.
            foreach (var resolveByType in entry.DependencyTupleType.GenericTypeArguments)
            {
                //Check if the type is resolved by name... if so, use the name for resolution. Otherwise, just use type to resolve from IOC
                string namedResolution;
                entry.NamedResolutions.TryGetValue(resolveByType, out namedResolution);
                dependencies.Add(context.ResolveDependency(resolveByType, namedResolution));
            }

            return Activator.CreateInstance(entry.DependencyTupleType, dependencies.ToArray());
        }


        private MappingConfigurationEntry[] GetApplicableEntries(Type source, Type target, bool explicitTarget)
        {
            if (explicitTarget)
            {
                return _entries.Where(x => x.Source.IsAssignableFrom(source) && x.Target.IsAssignableFrom(target)).ToArray();
            }
            else
            {
                return _entries.Where(x => x.Source.IsAssignableFrom(source) && target.IsAssignableFrom(x.Target)).ToArray();
            }
        }


    }

}