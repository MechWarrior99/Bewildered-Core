using System;
using System.Collections.Generic;

namespace Bewildered.Editor
{
    [Serializable]
    public class DropdownItem
    {
        private List<DropdownItem> _children = new List<DropdownItem>();

        public List<DropdownItem> Children
        {
            get { return _children; }
            set { _children = value; }
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

        public DropdownItem(string name)
        {
            Name = name;
        }

        public void AddChild(DropdownItem child)
        {
            child.Parent = this;
            _children.Add(child);
        }

        /// <summary>
        /// Adds a set of children without setting their parent.
        /// </summary>
        /// <param name="items"></param>
        internal void DirectAddChildRange(List<DropdownItem> items)
        {
            _children.AddRange(items);
        }

        public List<DropdownItem> GetAllChildren()
        {
            List<DropdownItem> children = new List<DropdownItem>(_children);

            foreach (var child in _children)
            {
                children.AddRange(child.GetAllChildren());
            }

            return children;
        }
    } 
}