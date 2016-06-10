using System;

namespace OMap
{
    public interface IDependencyResolver
    {
        object Resolve(Type type, string name = null);
    }
}