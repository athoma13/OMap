using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OMap
{
    internal static class MapAllHelper
    {
        public static void MapAll(IInternalBuilder builder, Type source, Type target, IEnumerable<LambdaExpression> exceptions)
        {
            var targetMembers = GetMemberNames(target);
            var exclusions = GetMemberNames(exceptions);
            var actualTargetMembers = targetMembers.Except(exclusions).ToArray();
            var equalityComparer = StringComparer.OrdinalIgnoreCase;
            var sourceMembers = new HashSet<string>(GetMemberNames(source), equalityComparer);



            var pairs = actualTargetMembers.Select(x => Tuple.Create(x, sourceMembers.FirstOrDefault(s => equalityComparer.Equals(s, x)))).ToArray();
            var notMatched = pairs.Where(x => x.Item2 == null).Select(x => x.Item1).ToArray();
            if (notMatched.Any()) throw new MappingException(string.Format("Error Creating map for {0}->{1}: Could not find mapping equivalent for {2} on destination type {1}", source.Name, target.Name, string.Join(", ", notMatched)));

            foreach (var pair in pairs)
            {
                var targetMemberName = pair.Item1;
                var sourceMemberName = pair.Item2;
                var targetExpression = CreateLambdaExpression(target, targetMemberName);
                var sourceExpression = CreateLambdaExpression(source, sourceMemberName);

                var directMap = targetExpression.ReturnType == sourceExpression.ReturnType;
                IConverter converter = null;
                if (!directMap)
                {
                    //Check if there is a converter for that map...
                    //Use last here so conversions are kind of overridable (i.e. Last applicable conversion wins).
                    var conversion = builder.Conversions.LastOrDefault(x => x.CanConvert(sourceExpression.ReturnType, targetExpression.ReturnType));
                    if (conversion != null) converter = conversion.Create(sourceExpression.ReturnType, targetExpression.ReturnType);
                    //Set direct map to true if a converter is found (but will have to use a converter)
                    directMap = converter != null;
                }

                if (!directMap)
                {
                    var entries = builder.GetEntries();
                    Type sourceItemType;
                    Type targetItemType;
                    var isSourceEnumerable = MappingHelper.TryGetCollectionType(sourceExpression.ReturnType, out sourceItemType);
                    var isTargetEnumerable = MappingHelper.TryGetCollectionType(targetExpression.ReturnType, out targetItemType);
                    var sourceMappingType = isSourceEnumerable ? sourceItemType : sourceExpression.ReturnType;
                    var targetMappingType = isTargetEnumerable ? targetItemType : targetExpression.ReturnType;

                    var hasMapping = entries.Any(x => x.SourceType == sourceMappingType && x.TargetType == targetMappingType);
                    if (hasMapping)
                    {
                        var isCollection = isSourceEnumerable || isTargetEnumerable;
                        builder.AddEntry(new BuilderConfigurationSourceTargetExpressionEntry(sourceExpression, targetExpression, null, isCollection ? MapType.MapCollection : MapType.MapObject));
                    }
                    else
                    {
                        throw new MappingException(string.Format("Could not create map for {0}.{1}->{2}.{3}. No Mapping found for {4}->{5}.", source.Name, sourceMemberName, target.Name, targetMemberName, sourceExpression.ReturnType.Name, targetExpression.ReturnType.Name));
                    }
                }
                else
                {
                    builder.AddEntry(new BuilderConfigurationSourceTargetExpressionEntry(sourceExpression, targetExpression, converter, MapType.MapProperty));
                }
            }
        }

        private static LambdaExpression CreateLambdaExpression(Type type, string memberName)
        {
            var p = Expression.Parameter(type, "x");
            var memberAccess = Expression.PropertyOrField(p, memberName);
            var lambda = Expression.Lambda(memberAccess, p);
            return lambda;
        }

        private static string[] GetMemberNames(IEnumerable<LambdaExpression> expressions)
        {
            var results = expressions.Select(x => SingleMemberExpessionVisitor.GetMemberSingle(x).Member.Name).ToArray();
            return results;
        }

        private static string[] GetMemberNames(Type type)
        {
            var result = new List<string>();
            result.AddRange(type.GetProperties().Select(x => x.Name));
            result.AddRange(type.GetFields().Select(x => x.Name));
            return result.ToArray();
        }

        private class SingleMemberExpessionVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression _parameter;
            private List<MemberExpression> Results { get; set; }

            private SingleMemberExpessionVisitor(ParameterExpression parameter)
            {
                _parameter = parameter;
                Results = new List<MemberExpression>();
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                if (_parameter == node.Expression)
                {
                    Results.Add(node);
                }
                return base.VisitMember(node);
            }

            public static MemberExpression GetMemberSingle(LambdaExpression expression)
            {
                var visitor = new SingleMemberExpessionVisitor(expression.Parameters[0]);
                visitor.Visit(expression);
                if (visitor.Results.Count != 1) throw new InvalidOperationException(string.Format("Expression is not in a valid format, expected something like x => x.Property, but received '{0}'", expression));
                return visitor.Results[0];
            }
        }
    }
}
