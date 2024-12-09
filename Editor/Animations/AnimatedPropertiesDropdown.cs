using System;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using System.Collections.Generic;

namespace TarasK8.UI.Editor.Animations
{
    public class AnimatedPropertiesDropdown : AdvancedDropdown
    {
        private readonly string[] _properties;
        private readonly Dictionary<AdvancedDropdownItem, string> _itemToPathMap;
        private readonly Dictionary<string, int> _propertyIndexMap;

        public Action<int> OnItemSelected { get; set; }

        public AnimatedPropertiesDropdown(AdvancedDropdownState state, string[] properties) : base(state)
        {
            _properties = properties;
            minimumSize = new Vector2(200f, 300f);

            // Initialize lookup dictionaries
            _itemToPathMap = new Dictionary<AdvancedDropdownItem, string>();
            _propertyIndexMap = BuildPropertyIndexMap(properties);
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("Properties");

            // Build the tree from the string array
            var categoryTree = BuildCategoryTree(_properties);

            // Convert the tree into AdvancedDropdownItems
            foreach (var category in categoryTree)
            {
                AddCategoryToDropdown(root, category.Key, category.Value, category.Key);
            }

            return root;
        }

        private Dictionary<string, object> BuildCategoryTree(string[] properties)
        {
            var root = new Dictionary<string, object>();

            foreach (var property in properties)
            {
                var parts = property.Split('/');
                var currentNode = root;

                for (int i = 0; i < parts.Length; i++)
                {
                    if (!currentNode.ContainsKey(parts[i]))
                    {
                        currentNode[parts[i]] = new Dictionary<string, object>();
                    }

                    // Navigate to the next level
                    currentNode = (Dictionary<string, object>)currentNode[parts[i]];
                }
            }

            return root;
        }

        private void AddCategoryToDropdown(AdvancedDropdownItem parent, string categoryName, object subcategories, string fullPath)
        {
            var categoryItem = new AdvancedDropdownItem(categoryName);

            // Store the full path for this item
            _itemToPathMap[categoryItem] = fullPath;

            parent.AddChild(categoryItem);

            if (subcategories is Dictionary<string, object> subcategoryDict)
            {
                foreach (var subcategory in subcategoryDict)
                {
                    var childFullPath = $"{fullPath}/{subcategory.Key}";
                    AddCategoryToDropdown(categoryItem, subcategory.Key, subcategory.Value, childFullPath);
                }
            }
        }

        private Dictionary<string, int> BuildPropertyIndexMap(string[] properties)
        {
            var map = new Dictionary<string, int>();
            for (int i = 0; i < properties.Length; i++)
            {
                map[properties[i]] = i;
            }
            return map;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            base.ItemSelected(item);

            // Lookup the full path of the selected item
            if (_itemToPathMap.TryGetValue(item, out string selectedProperty))
            {
                if (_propertyIndexMap.TryGetValue(selectedProperty, out int index))
                {
                    OnItemSelected?.Invoke(index); // Raise the event with the index
                }
                else
                {
                    Debug.LogWarning($"Selected item '{selectedProperty}' not found in the properties array.");
                }
            }
            else
            {
                Debug.LogWarning("Selected item not mapped to a property.");
            }
        }
    }
}
