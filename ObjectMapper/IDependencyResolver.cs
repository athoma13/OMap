using System;

namespace ObjectMapper
{
    public interface IDependencyResolver
    {
        object Resolve(Type type, string name = null);
    }

    public class ResolverMock : IDependencyResolver
    {
        public object Resolve(Type type, string name = null)
        {
            return null;
        }
    }
}