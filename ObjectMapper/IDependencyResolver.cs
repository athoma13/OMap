using System;

namespace ObjectMapper
{
    public interface IDependencyResolver
    {
        object Resolve(Type type, string name = null);
    }
}