using System;
using System.Collections.Generic;

namespace ObjectMapper
{
    public class MappingContext
    {
        private readonly IDependencyResolver _resolver;
        private readonly Dictionary<Type, Dictionary<string, object>> _dependencyCache = new Dictionary<Type, Dictionary<string, object>>();

        public MappingContext(IDependencyResolver resolver)
        {
            _resolver = resolver;
        }

        public object ResolveDependency(Type type, string name)
        {
            Dictionary<string, object> tmp;

            if (!_dependencyCache.TryGetValue(type, out tmp))
            {
                tmp = new Dictionary<string, object>();
                _dependencyCache.Add(type, tmp);
            }

            object dependency;
            var key = name ?? string.Empty;
            if (!tmp.TryGetValue(key, out dependency))
            {
                //NOTE: Yes, pass a null name to the resolver here!
                dependency = _resolver.Resolve(type, name);
                tmp.Add(key, dependency);
            }

            return dependency;
        }
    }
}