using System;
using System.Collections.Generic;
using System.Linq;

namespace ObjectMapper
{
    public interface IObjectMapper<in TSource, TTarget>
    {
        void Map(TSource source, TTarget target);
        TTarget Map(TSource source);
    }

    public class ObjectMapper<TSource, TTarget> : IObjectMapper<TSource, TTarget>
    {
        private readonly MappingActionEntry[] _entries;

        public ObjectMapper(IEnumerable<MappingActionEntry> entries)
        {
            _entries = entries.ToArray();
        }

        public void Map(TSource source, TTarget target)
        {
            MapInternal(source, target);
        }

        public void MapInternal(object source, object target)
        {
            foreach (var entry in _entries)
            {
                if (entry.HasDependencies)
                {
                    entry.MappingAction.DynamicInvoke(source, target, entry.ResolvedDependencies);
                }
                else
                {
                    entry.MappingAction.DynamicInvoke(source, target);
                }
            }
        }


        public TTarget Map(TSource source)
        {
            if (source == null) return default(TTarget);

            var sourceType = source.GetType();
            var targetTypes = _entries.Where(x => x.Source == sourceType).Select(x => x.Target).Distinct().ToArray();
            if (targetTypes.Length > 1) throw new InvalidOperationException(string.Format("Cannot map from {0} to {1}. Cannot Decide what the target type should be. Options are: {2}", sourceType, typeof(TTarget), string.Join(", ", targetTypes.Cast<object>().ToArray())));
            if (targetTypes.Length == 1)
            {
                var result = Activator.CreateInstance(targetTypes[0]);
                MapInternal(source, result);
                return (TTarget)result;
            }

            targetTypes = _entries.Select(x => x.Target).Distinct().ToArray();
            if (targetTypes.Length == 0) throw new InvalidOperationException(string.Format("Cannot map from {0} to {1}. No target types found", sourceType, typeof(TTarget)));
            if (targetTypes.Length != 1) throw new InvalidOperationException(string.Format("Cannot map from {0} to {1}. Cannot Decide what the target type should be. Options are: {2}", sourceType, typeof(TTarget), string.Join(", ", targetTypes.Cast<object>().ToArray())));
            var result1 = Activator.CreateInstance(targetTypes[0]);
            MapInternal(source, result1);
            return (TTarget)result1;

        }
    }

    public class MappingActionEntry
    {
        public Type Source { get; set; }
        public Type Target { get; set; }
        public Delegate MappingAction { get; private set; }
        public object ResolvedDependencies { get; private set; }
        public bool HasDependencies { get { return ResolvedDependencies != null; } }

        public MappingActionEntry(Type source, Type target, Delegate mappingAction, object resolvedDependencies)
        {
            Source = source;
            Target = target;
            MappingAction = mappingAction;
            ResolvedDependencies = resolvedDependencies;
        }
    }

}