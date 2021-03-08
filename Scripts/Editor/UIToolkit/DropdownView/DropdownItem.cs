using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Bewildered.Editor
{
    [Serializable]
    public class DropdownItem : IComparable<DropdownItem>
    {
        private List<DropdownItem> _children = new List<DropdownItem>();

        public ReadOnlyCollection<DropdownItem> Children
        {
            get { return _children.AsReadOnly(); }
        }

        public bool HasChildren
        {
            get { return _children.Count > 0; }
        }

        public bool HasParent
        {
            get { return Parent != null; }
        }

        public string Name 
        {
            get;
        }

        public DropdownItem Parent 
        {
            get;
            private set; 
        }

        public DropdownItem this[int index]
        {
            get { return _children[index]; }
            set { _children[index] = value; }
        }

        public DropdownItem(string name)
        {
            Name = name;
        }

        public void InserChild(int index, DropdownItem child)
        {
            child.Parent = this;
            _children.Insert(index, child);
        }

        public void AddChild(DropdownItem child)
        {
            child.Parent = this;
            _children.Add(child);
        }

        public void SortTree()
        {
            _children.Sort();
            foreach (var child in _children)
            {
                child.SortTree();
            }
        }

        public void SortTree(Comparison<DropdownItem> comparison)
        {
            _children.Sort(comparison);
            foreach (var child in _children)
            {
                child.SortTree(comparison);
            }
        }

        /// <summary>
        /// Adds a set of children without setting their parent.
        /// </summary>
        /// <param name="items"></param>
        internal void DirectAddChildRange(List<DropdownItem> items)
        {
            _children.AddRange(items);
        }

        public IEnumerable<DropdownItem> GetAllChildren()
        {
            List<DropdownItem> children = new List<DropdownItem>(_children);

            foreach (var child in _children)
            {
                children.AddRange(child.GetAllChildren());
            }

            return children;
        }

        public int CompareTo(DropdownItem other)
        {
            int childrenDiff = other.HasChildren.CompareTo(HasChildren);
            return childrenDiff != 0 ? childrenDiff : string.CompareOrdinal(Name, other.Name);
        }
    } 
}