using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Android.Types;
using UnityEngine;

namespace UnityFracture
{
    // https://code.google.com/archive/p/yet-another-tree-structure/wikis/CSharpTree.wiki
    public class TreeNode : IEnumerable<TreeNode>
    {
        public ICollection<TreeNode> Children { get; set; }
        public ICollection<TreeNode> Parents { get; set; }
        public Connections connections;

        public TreeNode(Connections obj)
        {
            connections = obj;
        }

        public bool IsRoot
        { 
            get 
            { 
                foreach(var parent in Parents)
                {
                    if (parent != null)
                        return false;
                }
                return true;
            } 
        }
        public bool IsLeaf
        {
            get
            {
                return Children.Count == 0;
            }
        }

        public void AddChild(TreeNode child)
        {
            Children.Add(child);
            RegisterChildForSearch(child);
        }
        public void AddParent(TreeNode parent)
        {
            Parents.Add(parent);
        }

        private ICollection<TreeNode> ElementsIndex { get; set; }

        private void RegisterChildForSearch(TreeNode node)
        {
            ElementsIndex.Add(node);
            foreach (var parent in Parents)
            {
                if (parent != null)
                    parent.RegisterChildForSearch(node);
            }
        }

        public TreeNode FindTreeNode(Func<TreeNode, bool> predicate)
        {
            return this.ElementsIndex.FirstOrDefault(predicate);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<TreeNode> GetEnumerator()
        {
            yield return this;
            foreach (var directChild in this.Children)
            {
                foreach (var anyChild in directChild)
                    yield return anyChild;
            }
        }
    }
}