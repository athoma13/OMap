using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMap.Tests
{
    public static class TestHelper
    {
        public static IObjectMapper CreateMapper(Action<ConfigurationBuilder> builder)
        {
            return CreateMapper(new ResolverMock(), builder);
        }

        public static IObjectMapper CreateMapper(IDependencyResolver resolver, Action<ConfigurationBuilder> builder)
        {
            var mappingProvider = new MappingConfigurationProvider(builder);
            var mapper = new ObjectMapper(mappingProvider, resolver);
            return mapper;
        }
    }

    public class ResolverMock : IDependencyResolver
    {
        private readonly Dictionary<Tuple<Type, string>, Delegate> _dependencies = new Dictionary<Tuple<Type, string>, Delegate>();

        public void Add<T>(Func<T> dependency, string name = null)
        {
            _dependencies[Tuple.Create(typeof(T), name)] = dependency;
        }

        public object Resolve(Type type, string name = null)
        {
            Delegate tmp;
            if (!_dependencies.TryGetValue(Tuple.Create(type, name), out tmp)) throw new InvalidOperationException("Dependency not registered");
            return tmp.DynamicInvoke();
        }
    }

}
