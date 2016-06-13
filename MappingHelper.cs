using System;

internal static class MappingHelper
{
    public static bool TryGetCollectionType(Type type, out Type itemType)
    {
        var itemTypes = GetItemCollectionTypes(type);
        if (itemTypes.Length == 0)
        {
            itemType = itemTypes[0];
            return true;
        }
        else
        {
            itemType = null;
            return false;
        }

        var enumerables = collectionType.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>)).ToArray();
        if (enumerables.Length == 0) throw new MappingException(string.Format("Cannot determine item type for collectionType {0}", collectionType));
        if (enumerables.Length > 1) throw new MappingException(string.Format("Cannot determine item type for collectionType {0} - Too many implementations of IEnumerable<>", collectionType));
        return enumerables[0].GetGenericArguments()[0];
    }

    public static Type GetCollectionItemType(Type collectionType)
    {
        var itemTypes = GetItemCollectionTypes(type);
        if (itemTypes.Length == 0) throw new MappingException(string.Format("Cannot determine item type for collectionType {0}", collectionType));
        if (itemTypes.Length > 1) throw new MappingException(string.Format("Cannot determine item type for collectionType {0} - Too many implementations of IEnumerable<>", collectionType));
        return itemTypes[0];
    }


    private static Type[] GetItemCollectionTypes(Type collectionType)
    {
        var enumerables = collectionType.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>)).ToArray();
        return enumerables.Select(x => x.GetGenericArguments()[0]).ToArray();
    }

}
