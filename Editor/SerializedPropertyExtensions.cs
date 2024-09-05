using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;

namespace TarasK8.UI.Editor.Animations
{
    public static class SerializedPropertyExtensions
    {
        public static IEnumerable<SerializedProperty> GetChildProperties(this SerializedProperty parent, int depth = 1)
        {
            parent = parent.Copy();

            int depthOfParent = parent.depth;
            var enumerator = parent.GetEnumerator();

            while (enumerator.MoveNext())
            {
                if (enumerator.Current is not SerializedProperty childProperty)
                {
                    continue;
                }
                if (childProperty.depth > (depthOfParent + depth))
                {
                    continue;
                }
                yield return childProperty.Copy();
            }
        }

        public static object SetManagedReference(this SerializedProperty property, Type type)
        {
            object result = null;

#if UNITY_2021_3_OR_NEWER
            // NOTE: managedReferenceValue getter is available only in Unity 2021.3 or later.
            if ((type != null) && (property.managedReferenceValue != null))
            {
                // Restore an previous values from json.
                string json = JsonUtility.ToJson(property.managedReferenceValue);
                result = JsonUtility.FromJson(json, type);
            }
#endif

            if (result == null)
            {
                result = (type != null) ? Activator.CreateInstance(type) : null;
            }

            property.managedReferenceValue = result;
            return result;

        }
    }
}