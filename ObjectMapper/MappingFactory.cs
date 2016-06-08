using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ObjectMapper
{
    public interface IMappingFactory
    {
        IObjectMapper<TSource, TTarget> CreateMapper<TSource, TTarget>();
    }

    public class MappingFactory : IMappingFactory
    {
        private readonly IDependencyResolver _resolver;
        private readonly MappingConfiguration _configuration;

        public MappingFactory(IDependencyResolver resolver, MappingConfiguration configuration)
        {
            _resolver = resolver;
            _configuration = configuration;
        }

        public IObjectMapper<TSource, TTarget> CreateMapper<TSource, TTarget>()
        {
            var entries = _configuration.GetEntries(typeof(TSource), typeof(TTarget));
            if (!entries.Any()) throw new InvalidOperationException(string.Format("No Mapper found for {0} and {1}", typeof(TSource), typeof(TTarget)));

            //Resolve all dependencies here...
            //NOTE: If multiple actions require the same dependency, it is reused!

            var mappingEntries = new List<MappingActionEntry>();
            var resolvedDependencies = new Dictionary<Type, object>();
            foreach (var entry in entries)
            {
                var dependencyTuple = GetDependencies(resolvedDependencies, entry);
                mappingEntries.Add(new MappingActionEntry(entry.Source, entry.Target, entry.MappingAction, dependencyTuple));
            }

            return new ObjectMapper<TSource, TTarget>(mappingEntries);
        }

        private object GetDependencies(IDictionary<Type, object> resolvedDependencies, MappingConfigurationEntry entry)
        {
            if (entry.DependencyTupleType == null) return null;

            var dependencies = new List<object>();
            //dependencies are passed as a Tuple into the mapping function... find all generic arguments to determine dependencies.
            foreach (var resolveByType in entry.DependencyTupleType.GenericTypeArguments)
            {
                object dependency;
                if (!resolvedDependencies.TryGetValue(resolveByType, out dependency))
                {
                    //Check if the type is resolved by name... if so, use the name for resolution. Otherwise, just use type to resolve from IOC
                    string namedResolution;
                    entry.NamedResolutions.TryGetValue(resolveByType, out namedResolution);

                    dependency = _resolver.Resolve(resolveByType, namedResolution);
                    resolvedDependencies.Add(resolveByType, dependency);
                }
                dependencies.Add(dependency);
            }
            return Activator.CreateInstance(entry.DependencyTupleType, dependencies.ToArray());
        }
    }
}
