using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OMap
{
    public interface IObjectMapper
    {
        TTarget Map<TTarget, TTargetBase>(object source) where TTarget : TTargetBase;
        void Map(object source, object target);
    }

    public class ObjectObjectMapper : IObjectMapper
    {
        private readonly IDependencyResolver _resolver;
        private readonly MappingConfigurationEntry[] _entries;

        public ObjectObjectMapper(IMappingConfigurationProvider mappingConfigurationProvider, IDependencyResolver resolver)
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
                try
                {
                    if (MapProperty(source, target, context, entry)) continue;
                    if (MapObject(source, target, context, entry)) continue;
                    if (MapCollection(source, target, context, entry)) continue;
                    if (MapFunction(source, target, context, entry)) continue;
                    throw new InvalidOperationException(string.Format("Unknown mapping entry {0}", entry.GetType()));
                }
                catch (Exception ex)
                {
                    throw new MappingException(string.Format("Error while mapping {0}. See InnerException for details", entry.EntryDescription), ex);
                }
            }
        }

        private bool MapProperty(object source, object target, MappingContext context, MappingConfigurationEntry baseEntry)
        {
            var entry = baseEntry as MappingConfigurationPropertyEntry;
            if (entry == null) return false;

            var dependencyTuple = GetDependencyTuple(context, entry.DependencyTupleType, entry.NamedResolutions);

            if (dependencyTuple != null)
            {
                entry.MappingAction.DynamicInvoke(source, target, dependencyTuple);
            }
            else
            {
                entry.MappingAction.DynamicInvoke(source, target);
            }

            return true;
        }

        private bool MapObject(object source, object target, MappingContext context, MappingConfigurationEntry baseEntry)
        {
            var entry = baseEntry as MappingConfigurationObjectEntry;
            if (entry == null) return false;

            var sourceObject = entry.GetSourceProperty.DynamicInvoke(source);
            var targetObject = entry.GetTargetProperty.DynamicInvoke(target);
            if (targetObject == null)
            {
                var newObject = Map(sourceObject, entry.TargetPropertyType, context);
                entry.SetTargetProperty.DynamicInvoke(target, newObject);
            }
            else
            {
                Map(sourceObject, targetObject, context);
            }

            return true;
        }

        private bool MapCollection(object source, object target, MappingContext context, MappingConfigurationEntry baseEntry)
        {
            var entry = baseEntry as MappingConfigurationCollectionEntry;
            if (entry == null) return false;

            var sourceEnumerable = (IEnumerable)entry.GetSourceProperty.DynamicInvoke(source);
            var targetCollection = entry.GetTargetProperty.DynamicInvoke(target);
            var sourceObjects = sourceEnumerable == null ? new object[0] : sourceEnumerable.Cast<object>().ToArray();
            var collectionItemType = MappingHelper.GetCollectionItemType(entry.TargetPropertyType);
            var mappedObjects = sourceObjects.Select(x => Map(x, collectionItemType, context)).ToArray();

            if (targetCollection == null)
            {
                if (entry.TargetPropertyType.IsArray)
                {
                    var array = Array.CreateInstance(collectionItemType, mappedObjects.Length);
                    Array.Copy(mappedObjects, array, mappedObjects.Length);
                    entry.SetTargetProperty.DynamicInvoke(target, array);
                }
                else
                {
                    var list = Activator.CreateInstance(entry.TargetPropertyType) as IList;
                    if (list == null) throw new MappingException(string.Format("Type {0} does not implement IList", entry.TargetPropertyType));
                    foreach (var obj in mappedObjects)
                    {
                        list.Add(obj);
                    }
                    entry.SetTargetProperty.DynamicInvoke(target, list);
                }
            }
            else
            {
                if (entry.TargetPropertyType.IsArray) throw new MappingException("Cannot map collection to initialized array.");
                var list = targetCollection as IList;
                if (list == null) throw new MappingException("Cannot map collection. Target does not implement IList");
                if (list.IsReadOnly) throw new MappingException("Cannot map collection to readonly IList");
                list.Clear();
                foreach (var obj in mappedObjects)
                {
                    list.Add(obj);
                }
            }

            return true;
        }

        private bool MapFunction(object source, object target, MappingContext context, MappingConfigurationEntry baseEntry)
        {
            var entry = baseEntry as MappingConfigurationFunctionEntry;
            if (entry == null) return false;

            var dependencyTuple = GetDependencyTuple(context, entry.DependencyTupleType, entry.NamedResolutions);

            if (dependencyTuple != null)
            {
                entry.Action.DynamicInvoke(source, target, dependencyTuple);
            }
            else
            {
                entry.Action.DynamicInvoke(source, target);
            }

            return true;
        }

        public TTarget Map<TTarget, TTargetBase>(object source) where TTarget : TTargetBase
        {
            var obj = Map(source, typeof(TTargetBase), new MappingContext(_resolver));
            return obj == null ? default(TTarget) : (TTarget)obj;
        }

        private object Map(object source, Type requestedTargetType, MappingContext context)
        {
            if (source == null) return null;

            var sourceType = source.GetType();
            var applicableEntries = GetApplicableEntries(sourceType, requestedTargetType, false);

            var targetTypes = applicableEntries.Where(x => x.Source == sourceType).Select(x => x.Target).Distinct().ToArray();
            if (targetTypes.Length > 1) throw new MappingException(string.Format("Cannot map from {0} to {1}. Cannot Decide what the target type should be. Options are: {2}", sourceType, requestedTargetType, string.Join(", ", targetTypes.Cast<object>().ToArray())));
            if (targetTypes.Length == 1)
            {
                var result = Activator.CreateInstance(targetTypes[0]);
                MapInternal(source, result, applicableEntries, context);
                return result;
            }

            targetTypes = applicableEntries.Select(x => x.Target).Distinct().ToArray();
            if (targetTypes.Length == 0) throw new MappingException(string.Format("Cannot map from {0} to {1}. No target types found", sourceType, requestedTargetType));
            if (targetTypes.Length != 1) throw new MappingException(string.Format("Cannot map from {0} to {1}. Cannot Decide what the target type should be. Options are: {2}", sourceType, requestedTargetType, string.Join(", ", targetTypes.Cast<object>().ToArray())));
            var result1 = Activator.CreateInstance(targetTypes[0]);
            MapInternal(source, result1, applicableEntries, context);
            return result1;
        }

        private static object GetDependencyTuple(MappingContext context, Type dependencyTupleType, IDictionary<Type, string> namedResolutions)
        {
            if (dependencyTupleType == null) return null;

            var dependencies = new List<object>();
            //dependencies are passed as a Tuple into the mapping function... find all generic arguments to determine dependencies.
            foreach (var resolveByType in dependencyTupleType.GenericTypeArguments)
            {
                //Check if the type is resolved by name... if so, use the name for resolution. Otherwise, just use type to resolve from IOC
                string namedResolution;
                namedResolutions.TryGetValue(resolveByType, out namedResolution);
                dependencies.Add(context.ResolveDependency(resolveByType, namedResolution));
            }

            return Activator.CreateInstance(dependencyTupleType, dependencies.ToArray());
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