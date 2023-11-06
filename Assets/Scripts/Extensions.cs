using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class IEnumerableExtensions
{
    public static T RandomElement<T>(this IEnumerable<T> enumerable)
    {
        int index = Random.Range(0, enumerable.Count());
        return enumerable.ElementAt(index);
    }
}
