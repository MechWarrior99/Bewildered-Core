using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace Bewildered.Editor
{
    public enum TypeDisplayGroup { ByNamespace, ByFullNamespace, None }

    /// <summary>
    /// A dropdown for selecting a <see cref="Type"/> from a list.
    /// </summary>
    public class TypeDropdown : AdvancedDropdownView
    {
        private const string NoneItemName = "(None)";

        public class TypeDropdownItem : DropdownItem
        {
            public Type Type { get; }

            public TypeDropdownItem(Type type, bool fullName) : base(fullName ? type.FullName : type.GetFriendlyName())
            {
                Type = type;
            }
        }

        private Assembly[] _avalibleAssemblies;
        private Type _parentType;
        private TypeDisplayGroup _grouping;
        private Texture _csIcon;

        /// <summary>
        /// Callback for when a <see cref="Type"/> has been selected from the <see cref="TypeDropdown"/>.
        /// </summary>
        public event Action<Type> OnTypeSelected;

        public TypeDropdown() : base() { }

        public TypeDropdown(Type parentType, Action<Type> onTypeSelected) : this(parentType, null, TypeDisplayGroup.ByNamespace, onTypeSelected) { }

        public TypeDropdown(Assembly[] assemblies, Action<Type> onTypeSelected) : this(null, assemblies, TypeDisplayGroup.ByNamespace, onTypeSelected) { }

        public TypeDropdown(Type parentType, TypeDisplayGroup grouping, Action<Type> onTypeSelected) : this(parentType, null, grouping, onTypeSelected) { }

        public TypeDropdown(Assembly[] assemblies, TypeDisplayGroup grouping, Action<Type> onTypeSelected) : this(null, assemblies, grouping, onTypeSelected) { }

        public TypeDropdown(Type parentType, Assembly[] assemblies, TypeDisplayGroup grouping, Action<Type> onTypeSelected) : base() 
        {
            _parentType = parentType;
            _avalibleAssemblies = assemblies;
            _grouping = grouping;
            OnTypeSelected = onTypeSelected;
            _csIcon = EditorGUIUtility.IconContent("cs Script Icon").image;

            Reload();
        }

        protected override DropdownItem BuildRoot()
        {
            DropdownItem root = new DropdownItem("Type");

            List<Type> types = new List<Type>();

            if (_parentType == null)
            {
                if (_avalibleAssemblies == null)
                    _avalibleAssemblies = AppDomain.CurrentDomain.GetAssemblies();

                foreach (var assembly in _avalibleAssemblies)
                    types.AddRange(TypeUtility.GetVisibleTypesFromAssembly(assembly));
            }
            else
            {
                types = new List<Type>(TypeCache.GetTypesDerivedFrom(_parentType));
            }

            switch (_grouping)
            {
                case TypeDisplayGroup.ByNamespace:
                    GroupByNamespace(root, types);
                    break;
                case TypeDisplayGroup.ByFullNamespace:
                    GroupByFullNamespace(root, types);
                    break;
                case TypeDisplayGroup.None:
                    foreach (var type in types)
                        root.AddChild(new TypeDropdownItem(type, true));
                    break;
            }

            root.SortTree();
            root.InserChild(0, new DropdownItem(NoneItemName));

            return root;
        }

        private void GroupByNamespace(DropdownItem root, IEnumerable<Type> types)
        {
            Dictionary<string, DropdownItem> itemsByNamespace = new Dictionary<string, DropdownItem>();

            foreach (Type type in types)
            {
                TypeDropdownItem typeItem = new TypeDropdownItem(type, false);

                if (string.IsNullOrEmpty(type.Namespace))
                {
                    root.AddChild(typeItem);
                }
                else
                {
                    if (!itemsByNamespace.TryGetValue(type.Namespace, out DropdownItem namespaceItem))
                    {
                        namespaceItem = new DropdownItem(type.Namespace);
                        root.AddChild(namespaceItem);
                        itemsByNamespace.Add(type.Namespace, namespaceItem);
                    }

                    namespaceItem.AddChild(typeItem);
                }
            }
        }

        private void GroupByFullNamespace(DropdownItem root, IEnumerable<Type> types)
        {
            Dictionary<string, DropdownItem> itemsByNamespace = new Dictionary<string, DropdownItem>();

            foreach (Type type in types)
            {
                TypeDropdownItem typeItem = new TypeDropdownItem(type, false);

                if (string.IsNullOrEmpty(type.Namespace))
                {
                    root.AddChild(typeItem);
                }
                else
                {
                    // Make sure all the items along a path exist.
                    string[] typeNamespaces = type.Namespace.Split('.');
                    string currentNamespace = "";
                    for (int i = 0; i < typeNamespaces.Length; i++)
                    {
                        string previusNamespace = currentNamespace;
                        currentNamespace += i > 0 ? "." + typeNamespaces[i] : typeNamespaces[i];

                        if (!itemsByNamespace.TryGetValue(currentNamespace, out DropdownItem namespaceItem))
                        {
                            namespaceItem = new DropdownItem(typeNamespaces[i]);
                            if (i > 0)
                                itemsByNamespace[previusNamespace].AddChild(namespaceItem);
                            else
                                root.AddChild(namespaceItem);

                            itemsByNamespace.Add(currentNamespace, namespaceItem);
                        }
                    }

                    //Add the item as a child.
                    itemsByNamespace[type.Namespace].AddChild(typeItem);
                }
            }
        }

        protected override void BindItem(VisualElement element, DropdownItem item, int index)
        {
            base.BindItem(element, item, index);

            Image iconImage = element.Q<Image>(ItemIconUssClassName);

            if (item is TypeDropdownItem typeItem)
            {
                iconImage.image = AssetPreview.GetMiniTypeThumbnail(typeItem.Type);
                if (iconImage.image == null)
                    iconImage.image = _csIcon;
            }
            else
            {
                iconImage.image = null;
            }

        }

        protected override void ItemSelected(DropdownItem item)
        {
            if (item is TypeDropdownItem typeItem)
                OnTypeSelected?.Invoke(typeItem.Type);
            else if (item.Name == NoneItemName)
                OnTypeSelected?.Invoke(null);
        }
    } 
}
