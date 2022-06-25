using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedBlackTree
{
    internal class RedBlackTreeNode<T> : BSTNodeBase<T> where T : IComparable
    {
        internal new RedBlackTreeNode<T> Parent
        {
            get { return (RedBlackTreeNode<T>)base.Parent; }
            set { base.Parent = value; }
        }

        internal RedBlackTreeNode<T> Left
        {
            get { return (RedBlackTreeNode<T>)base.Left; }
            set { base.Left = value; }
        }

        internal RedBlackTreeNode<T> Right
        {
            get { return (RedBlackTreeNode<T>)base.Right; }
            set { base.Right = value; }
        }

        internal RedBlackTreeNodeColor NodeColor { get; set; }

        internal RedBlackTreeNode<T> Sibling => Parent.Left == this ?
                                                Parent.Right : Parent.Left;

        internal RedBlackTreeNode(RedBlackTreeNode<T> parent, T value)
        {
            this.Parent = parent;
            this.Value = value;
            this.NodeColor = RedBlackTreeNodeColor.Red;
        }   
        
    }
}
