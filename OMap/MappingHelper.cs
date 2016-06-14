using System;
using System.Collections.Generic;
using System.Linq;

namespace OMap
{
    internal static class MappingHelper
    {
        public static Type AsCollectionTypeItem(Type type)
        {
            Type itemType;
            if (TryGetCollectionType(type, out itemType)) return itemType;
            return type;
        }

        public static bool TryGetCollectionType(Type type, out Type itemType)
        {
            var itemTypes = GetItemCollectionTypes(type);
            if (itemTypes.Length == 1)
            {
                itemType = itemTypes[0];
                return true;
            }
            else
            {
                itemType = null;
                return false;
            }
        }

        public static Type GetCollectionItemType(Type collectionType)
        {
            var itemTypes = GetItemCollectionTypes(collectionType);
            if (itemTypes.Length == 0) throw new MappingException(string.Format("Cannot determine item type for collectionType {0}", collectionType));
            if (itemTypes.Length > 1) throw new MappingException(string.Format("Cannot determine item type for collectionType {0} - Too many implementations of IEnumerable<>", collectionType));
            return itemTypes[0];
        }


        private static Type[] GetItemCollectionTypes(Type collectionType)
        {
            var interfaces = collectionType.GetInterfaces().ToList();
            if (collectionType.IsInterface) interfaces.Add(collectionType);

            var enumerables = interfaces.Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>)).ToArray();
            return enumerables.Select(x => x.GetGenericArguments()[0]).ToArray();
        }

    }
}
