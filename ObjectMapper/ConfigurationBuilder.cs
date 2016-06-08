using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ObjectMapper
{
    public class BuilderNode<TSource, TTarget>
    {
        private readonly IAddConfigurationEntry _addConfigurationEntry;

        public BuilderNode(IAddConfigurationEntry addConfigurationEntry)
        {
            _addConfigurationEntry = addConfigurationEntry;
        }

        public BuilderNode<TSource, TTarget> MapProperty<TProperty>(Expression<Func<TSource, TProperty>> from, Expression<Func<TTarget, TProperty>> to)
        {
            _addConfigurationEntry.AddEntry(new BuilderConfigurationEntry(from, to, BuildeConfigurationEntryType.SourceTargetNoDependency));
            return this;
        }
    }

    public class BuilderNode<TSource, TTarget, TDependencies>
    {
        private readonly IAddConfigurationEntry _addConfigurationEntry;

        public BuilderNode(IAddConfigurationEntry addConfigurationEntry)
        {
            _addConfigurationEntry = addConfigurationEntry;
        }

        public BuilderNode<TSource, TTarget, TDependencies> MapProperty<TProperty>(Expression<Func<TSource, TProperty>> from, Expression<Func<TTarget, TProperty>> to)
        {
            _addConfigurationEntry.AddEntry(new BuilderConfigurationEntry(from, to, BuildeConfigurationEntryType.SourceTargetNoDependency));
            return this;
        }

        public BuilderNode<TSource, TTarget, TDependencies> MapProperty<TProperty>(Expression<Func<TSource, TDependencies, TProperty>> from, Expression<Func<TTarget, TProperty>> to)
        {
            _addConfigurationEntry.AddEntry(new BuilderConfigurationEntry(from, to, BuildeConfigurationEntryType.SourceTargetWithDependency));
            return this;
        }
    }

    public class BuilderNodeWithDependencies<TSource, TTarget>
    {
        private readonly IAddConfigurationEntry _addConfigurationEntry;

        public BuilderNodeWithDependencies(IAddConfigurationEntry addConfigurationEntry)
        {
            _addConfigurationEntry = addConfigurationEntry;
        }

        public BuilderNode<TSource, TTarget> MapProperty<TProperty>(Expression<Func<TSource, TProperty>> from, Expression<Func<TTarget, TProperty>> to)
        {
            _addConfigurationEntry.AddEntry(new BuilderConfigurationEntry(from, to, BuildeConfigurationEntryType.SourceTargetNoDependency));
            return new BuilderNode<TSource, TTarget>(_addConfigurationEntry);
        }

        public BuilderNode<TSource, TTarget, Tuple<T>> WithDependencies<T>(string name = null)
        {
            _addConfigurationEntry.SetNamedResolutions(new Dictionary<Type, string>() { { typeof(T), name } });
            return new BuilderNode<TSource, TTarget, Tuple<T>>(_addConfigurationEntry);
        }
        public BuilderNode<TSource, TTarget, Tuple<T1, T2>> WithDependencies<T1, T2>(string name1 = null, string name2 = null)
        {
            _addConfigurationEntry.SetNamedResolutions(new Dictionary<Type, string>() { { typeof(T1), name1 }, { typeof(T2), name2 } });
            return new BuilderNode<TSource, TTarget, Tuple<T1, T2>>(_addConfigurationEntry);
        }
        public BuilderNode<TSource, TTarget, Tuple<T1, T2, T3>> WithDependencies<T1, T2, T3>(string name1 = null, string name2 = null, string name3 = null)
        {
            _addConfigurationEntry.SetNamedResolutions(new Dictionary<Type, string>() { { typeof(T1), name1 }, { typeof(T2), name2 }, { typeof(T3), name3 } });
            return new BuilderNode<TSource, TTarget, Tuple<T1, T2, T3>>(_addConfigurationEntry);
        }
        public BuilderNode<TSource, TTarget, Tuple<T1, T2, T3, T4>> WithDependencies<T1, T2, T3, T4>(string name1 = null, string name2 = null, string name3 = null, string name4 = null)
        {
            _addConfigurationEntry.SetNamedResolutions(new Dictionary<Type, string>() { { typeof(T1), name1 }, { typeof(T2), name2 }, { typeof(T3), name3 }, { typeof(T4), name4 } });
            return new BuilderNode<TSource, TTarget, Tuple<T1, T2, T3, T4>>(_addConfigurationEntry);
        }
        public BuilderNode<TSource, TTarget, Tuple<T1, T2, T3, T4, T5>> WithDependencies<T1, T2, T3, T4, T5>(string name1 = null, string name2 = null, string name3 = null, string name4 = null, string name5 = null)
        {
            _addConfigurationEntry.SetNamedResolutions(new Dictionary<Type, string>() { { typeof(T1), name1 }, { typeof(T2), name2 }, { typeof(T3), name3 }, { typeof(T4), name4 }, { typeof(T5), name5 } });
            return new BuilderNode<TSource, TTarget, Tuple<T1, T2, T3, T4, T5>>(_addConfigurationEntry);
        }
        public BuilderNode<TSource, TTarget, Tuple<T1, T2, T3, T4, T5, T6>> WithDependencies<T1, T2, T3, T4, T5, T6>(string name1 = null, string name2 = null, string name3 = null, string name4 = null, string name5 = null, string name6 = null)
        {
            _addConfigurationEntry.SetNamedResolutions(new Dictionary<Type, string>() { { typeof(T1), name1 }, { typeof(T2), name2 }, { typeof(T3), name3 }, { typeof(T4), name4 }, { typeof(T5), name5 }, { typeof(T6), name6 } });
            return new BuilderNode<TSource, TTarget, Tuple<T1, T2, T3, T4, T5, T6>>(_addConfigurationEntry);
        }
        public BuilderNode<TSource, TTarget, Tuple<T1, T2, T3, T4, T5, T6, T7>> WithDependencies<T1, T2, T3, T4, T5, T6, T7>(string name1 = null, string name2 = null, string name3 = null, string name4 = null, string name5 = null, string name6 = null, string name7 = null)
        {
            _addConfigurationEntry.SetNamedResolutions(new Dictionary<Type, string>() { { typeof(T1), name1 }, { typeof(T2), name2 }, { typeof(T3), name3 }, { typeof(T4), name4 }, { typeof(T5), name5 }, { typeof(T6), name6 }, { typeof(T7), name7 } });
            return new BuilderNode<TSource, TTarget, Tuple<T1, T2, T3, T4, T5, T6, T7>>(_addConfigurationEntry);
        }
        public BuilderNode<TSource, TTarget, Tuple<T1, T2, T3, T4, T5, T6, T7, T8>> WithDependencies<T1, T2, T3, T4, T5, T6, T7, T8>(string name1 = null, string name2 = null, string name3 = null, string name4 = null, string name5 = null, string name6 = null, string name7 = null, string name8 = null)
        {
            _addConfigurationEntry.SetNamedResolutions(new Dictionary<Type, string>() { { typeof(T1), name1 }, { typeof(T2), name2 }, { typeof(T3), name3 }, { typeof(T4), name4 }, { typeof(T5), name5 }, { typeof(T6), name6 }, { typeof(T7), name7 }, { typeof(T8), name8 } });
            return new BuilderNode<TSource, TTarget, Tuple<T1, T2, T3, T4, T5, T6, T7, T8>>(_addConfigurationEntry);
        }
    }

    public interface IAddConfigurationEntry
    {
        void AddEntry(BuilderConfigurationEntry entry);
        void SetNamedResolutions(IDictionary<Type, string> namedResolutions);
    }

    public enum BuildeConfigurationEntryType
    {
        SourceTargetWithDependency,
        SourceTargetNoDependency,
    }

    public class BuilderConfigurationEntry
    {
        public LambdaExpression SourceExpression { get; private set; }
        public LambdaExpression TargetExpression { get; private set; }
        public BuildeConfigurationEntryType EntryType { get; private set; }

        public BuilderConfigurationEntry(LambdaExpression sourceExpression, LambdaExpression targetExpression, BuildeConfigurationEntryType entryType)
        {
            SourceExpression = sourceExpression;
            TargetExpression = targetExpression;
            EntryType = entryType;
        }
    }

    public class ConfigurationBuilder : IAddConfigurationEntry
    {
        private readonly List<BuilderConfigurationEntry> _builderEntries = new List<BuilderConfigurationEntry>();
        private IDictionary<Type, string> _namedResolutions = new Dictionary<Type, string>();

        public BuilderNodeWithDependencies<TSource, TTarget> CreateMap<TSource, TTarget>()
        {
            return new BuilderNodeWithDependencies<TSource, TTarget>(this);
        }
        void IAddConfigurationEntry.AddEntry(BuilderConfigurationEntry entry)
        {
            _builderEntries.Add(entry);
        }
        void IAddConfigurationEntry.SetNamedResolutions(IDictionary<Type, string> names)
        {
            _namedResolutions = names;
        }

        public MappingConfiguration Build()
        {
            var configEntries = new List<MappingConfigurationEntry>();
            foreach (var entry in _builderEntries)
            {
                var hasDependency = entry.EntryType == BuildeConfigurationEntryType.SourceTargetWithDependency;
                var action = CreateMappingAction(entry.SourceExpression, entry.TargetExpression, hasDependency);
                var configEntry = new MappingConfigurationEntry(entry.SourceExpression.Parameters[0].Type, entry.TargetExpression.Parameters[0].Type, action, hasDependency ? entry.SourceExpression.Parameters[1].Type : null, _namedResolutions);
                configEntries.Add(configEntry);
            }
            return new MappingConfiguration(configEntries, _namedResolutions);
        }

        private static Delegate CreateMappingAction(LambdaExpression source, LambdaExpression target, bool hasDependency)
        {
            var pSource = Expression.Parameter(source.Parameters[0].Type, "source");
            var pDependencies = hasDependency ? Expression.Parameter(source.Parameters[1].Type, "dependencies") : null;
            var body = source.Body;
            body = Replace(body, source.Parameters[0], pSource);
            if (hasDependency) body = Replace(body, source.Parameters[1], pDependencies);

            var pTarget = Expression.Parameter(target.Parameters[0].Type, "target");
            var targetProperty = target.Body;
            targetProperty = Replace(targetProperty, target.Parameters[0], pTarget);

            var setter = Expression.Assign(targetProperty, body);
            var lamda = hasDependency ? Expression.Lambda(setter, pSource, pTarget, pDependencies) : Expression.Lambda(setter, pSource, pTarget);
            return lamda.Compile();
        }


        private class ReplaceExpressionVisitor : ExpressionVisitor
        {
            private readonly Expression _node;
            private readonly Expression _withNode;

            public ReplaceExpressionVisitor(Expression node, Expression withNode)
            {
                _node = node;
                _withNode = withNode;
            }

            public override Expression Visit(Expression node)
            {
                return node == _node ? _withNode : base.Visit(node);
            }
        }

        private static Expression Replace(Expression expression, Expression node, Expression withNode)
        {
            return new ReplaceExpressionVisitor(node, withNode).Visit(expression);
        }
    }
}