using System.Collections.Generic;
using UnityEngine;

namespace TarasK8.UI.Utilites
{
    public static class GetComponentExtension
    {
        public static void GetComponentsInNearestChildren<T>(this Transform parent, ICollection<T> results)
        {
            results.Clear();
            foreach (Transform item in parent)
            {
                if (item.TryGetComponent<T>(out var component))
                {
                    results.Add(component);
                }
            }
        }
    }
}
