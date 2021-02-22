using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bewildered
{
    [Serializable]
    public class GenericTreeItem<T> : IEnumerable<GenericTreeItem<T>>
    {
        [SerializeField] private int _id;
        [SerializeField] private List<int> _childIds = new List<int>();
        [SerializeField] private int _parentId;
        [SerializeField] private T _value;

        [NonSerialized] private List<GenericTreeItem<T>> _children = new List<GenericTreeItem<T>>();
        [NonSerialized] protected GenericTreeItem<T> _parent;
        [NonSerialized] protected GenericTree<T> _root;

        public int Id
        {
            get { return _id; }
        }

        public GenericTreeItem<T> Parent
        {
            get { return _parent; }
            internal set { _parent = value; }
        }

        public GenericTree<T> Root
        {
            get { return _root; }
            internal set { _root = value; }
        }

        public T Value
        {
            get { return _value; }
            set { _value = value; }
        }

        internal int ParentId
        {
            get { return _parentId; }
            set { _parentId = value; }
        }

        internal List<GenericTreeItem<T>> Children
        {
            get { return _children; }
        }

        internal List<int> ChildIds
        {
            get { return _childIds; }
        }

        public virtual void AddChild(GenericTreeItem<T> child)
        {
            if (child == null)
                return;

            // Remove the child from it's current root if it has one and is different from this root.
            if (child.Root != _root && child.Root != null)
            {
                child.Root.RemoveItemFromTree(child);
                child._id = _root.GetNextId();
            }

            // Remove the child from it's current parent if it has one.
            if (child.Parent != null)
            {
                child.Parent.RemoveChildReference(child);
            }

            if (_root != null)
                _root.AddItemToTree(child);

            AddChildReference(child);
        }

        public virtual bool RemoveChild(GenericTreeItem<T> child)
        {
            if (!_children.Contains(child) || child == null)
                return false;

            child.Root.RemoveItemFromTree(child);

            RemoveChildReference(child);

            return true;
        }

        protected void AddChildReference(GenericTreeItem<T> child)
        {
            _children.Add(child);
            _childIds.Add(child.Id);
            child._parentId = _id;
            child._parent = this;
        }

        protected void RemoveChildReference(GenericTreeItem<T> child)
        {
            _children.Remove(child);
            _childIds.Remove(child.Id);
            child._parentId = -1;
            child._parent = null;
        }

        public virtual bool Contains(GenericTreeItem<T> item)
        {
            return _children.Contains(item);
        }

        public virtual bool ContainsRecursive(GenericTreeItem<T> item)
        {
            foreach (var child in _children)
            {
                if (child == item)
                    return true;

                bool contains = child.ContainsRecursive(item);
                if (contains)
                    return contains;
            }

            return false;
        }

        public GenericTreeItem<T> FindChild(Predicate<GenericTreeItem<T>> match)
        {
            return _children.Find(match);
        }

        public virtual void ReorderChild(GenericTreeItem<T> child, int toIndex)
        {

        }

        /// <summary>
        /// Recursivly gets all of the children of the <see cref="GenericTreeItem{T}"/>.
        /// </summary>
        /// <returns>All the children of the <see cref="GenericTreeItem{T}"/> at any depth. Ordered depth first.</returns>
        public IEnumerable<GenericTreeItem<T>> GetAllChildren()
        {
            List<GenericTreeItem<T>> allChildren = new List<GenericTreeItem<T>>();
            foreach (var child in _children)
            {
                allChildren.Add(child);
                allChildren.AddRange(child.GetAllChildren());
            }

            return allChildren;
        }

        /// <summary>
        /// Performs the specified action on each direct child of the <see cref="GenericTreeItem{T}"/>.
        /// </summary>
        /// <param name="action">The <see cref="Action{T}"/> delegate to perform on each direct child of the <see cref="GenericTreeItem{T}"/>.</param>
        public void ForEach(Action<GenericTreeItem<T>> action)
        {
            _children.ForEach(action);
        }

        /// <summary>
        /// Performs the specified action on each child in the whole tree of the <see cref="GenericTreeItem{T}"/>.
        /// </summary>
        /// <param name="action">The <see cref="Action{T}"/> delegate to perform on each child of the <see cref="GenericTreeItem{T}"/>.</param>
        public void ForEachRecursive(Action<GenericTreeItem<T>> action)
        {
            foreach (GenericTreeItem<T> child in _children)
            {
                action?.Invoke(child);
                child.ForEachRecursive(action);
            }
        }

        public IEnumerator<GenericTreeItem<T>> GetEnumerator()
        {
            return _children.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _children.GetEnumerator();
        }
    } 
}
