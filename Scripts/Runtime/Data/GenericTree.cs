using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bewildered
{
    [Serializable]
    public class GenericTree<T> : GenericTreeItem<T>, ISerializationCallbackReceiver
    {
        [SerializeField] private int _nextId = -1;
        [SerializeField] private List<int> _allChildIds = new List<int>();
        [SerializeField] private List<GenericTreeItem<T>> _allChildren = new List<GenericTreeItem<T>>();

        internal List<int> AllChildIds
        {
            get { return _allChildIds; }
        }

        internal List<GenericTreeItem<T>> AllChildren
        {
            get { return _allChildren; }
        }

        internal Dictionary<int, GenericTreeItem<T>> ItemsById { get; set; } = new Dictionary<int, GenericTreeItem<T>>();

        internal int GetNextId()
        {
            _nextId++;
            return _nextId;
        }

        internal void AddItemToTree(GenericTreeItem<T> item)
        {
            _allChildren.Add(item);
            _allChildIds.Add(item.Id);
            item.Root = this;
            item.ForEach(AddItemToTree);
        }

        internal void RemoveItemFromTree(GenericTreeItem<T> item)
        {
            _allChildren.Remove(item);
            _allChildIds.Remove(item.Id);
            item.ForEach(RemoveItemFromTree);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            SetupHierarchyFromIds();
        }

        internal void SetupHierarchyFromIds()
        {
            if (ItemsById == null)
                ItemsById = new Dictionary<int, GenericTreeItem<T>>();

            // Populate the dictionary so that the nodes can be gotten easily by their id.
            ItemsById.Clear();
            for (int i = 0; i < _allChildIds.Count; i++)
            {
                ItemsById[_allChildIds[i]] = _allChildren[i];
            }

            Children.Clear();
            Root = this;

            // The root children need to be handled on their own because it's Children property will not be populated because the root is not in the _allItems list.
            // It cannot be added to the _allItems list because it would be a seperate reference so adding Children to it would not add them to the root.
            // This may not be true.

            foreach (int childId in ChildIds)
            {
                SetupItem(GetItemFromId(childId));
            }

            foreach (var item in _allChildren)
            {
                SetupItem(item);
            }
        }

        private void SetupItem(GenericTreeItem<T> item)
        {
            item.Root = this;
            item.Parent = GetItemFromId(item.ParentId);
            item.Parent.Children.Add(item);
        }

        private GenericTreeItem<T> GetItemFromId(int id)
        {
            ItemsById.TryGetValue(id, out GenericTreeItem<T> item);

            return item;
        }
    } 
}
