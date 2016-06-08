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
            MapInternal(source, target, GetApplicableEntries(source.GetType(), target.GetType(), true), new MappingContext(_resolver));
        }

        private static void MapInternal(object source, object target, MappingConfigurationEntry[] entries, MappingContext context)
        {
            foreach (var entry in entries)
            {
                object dependencyTuple = null;
                if (entry.DependencyTupleType != null) dependencyTuple = GetDependencyTuple(context, entry);

                if (dependencyTuple != null)
                {
                    entry.MappingAction.DynamicInvoke(source, target, dependencyTuple);
                }
                else
                {
                    entry.MappingAction.DynamicInvoke(source, target);
                }
            }
        }

        private static object GetDependencyTuple(MappingContext context, MappingConfigurationEntry entry)
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

    }

}