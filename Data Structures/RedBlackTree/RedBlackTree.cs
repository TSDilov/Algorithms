using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RedBlackTree
{
    /// A red black tree implementation.
    public class RedBlackTree<T> : IEnumerable<T> where T : IComparable
    {
        //if enabled, lookup will fasten deletion/insertion/exists operations.
        internal readonly Dictionary<T, BSTNodeBase<T>> nodeLookUp;

        public RedBlackTree(Boolean enableNodeLookUp = false, IEqualityComparer<T> equalityComparer = null)
        {
            if (enableNodeLookUp)
            {
                if (!typeof(T).GetTypeInfo().IsValueType && equalityComparer == null)
                {
                    throw new ArgumentException("equalityComparer parameter is required when node lookup us enabled and T is not a value type.");
                }

                this.nodeLookUp = new Dictionary<T, BSTNodeBase<T>>(equalityComparer ?? EqualityComparer<T>.Default);
            }
        }

        // Initialize the BST with given sorted keys optionally.
        // Time complexity: O(n). <summary>

        public RedBlackTree(IEnumerable<T> sortedCollection, bool enableNodeLookUp = false,
            IEqualityComparer<T> equalityComparer = null)
        {
            BSTHelpers.ValidateSortedCollection(sortedCollection);
            var nodes = sortedCollection.Select(x => new RedBlackTreeNode<T>(null, x)).ToArray();
            this.Root = (RedBlackTreeNode<T>)BSTHelpers.ToBST(nodes);
            assignColors(this.Root);
            BSTHelpers.AssignCount(this.Root);
            if (enableNodeLookUp)
            {
                if (!typeof(T).GetTypeInfo().IsValueType && equalityComparer == null)
                {
                    throw new ArgumentException("equalityComparer parameter is required when node lookup us enabled and T is not a value type.");
                }

                this.nodeLookUp = nodes.ToDictionary(x => x.Value, x => x as BSTNodeBase<T>, 
                                                    equalityComparer ?? EqualityComparer<T>.Default);
            }
            
        }

        internal RedBlackTreeNode<T> Root { get; set; }

        public int Count => this.Root == null ? 0 : this.Root.Count;


        /// <summary>
        ///  Time complexity: O(log(n))
        /// </summary>
        public bool HasItem(T value)
        {
            if (Root == null)
            {
                return false;
            }

            if (this.nodeLookUp != null)
            {
                return this.nodeLookUp.ContainsKey(value);
            }

            return Find(value).Item1 != null;
        }

        internal void Clear()
        {
            this.Root = null;
        }

        /// <summary>
        ///  Time complexity: O(log(n))
        /// </summary>
        public T Max()
        {
            var max = this.Root.FindMax();
            return max == null ? default(T) : max.Value;
        }

        private RedBlackTreeNode<T> FindMaxPrivate(RedBlackTreeNode<T> node)
        {
            return node.FindMax() as RedBlackTreeNode<T>;
        }

        /// <summary>
        ///  Time complexity: O(log(n))
        /// </summary>
        public T Min()
        {
            var min = this.Root.FindMin();
            return min == null ? default(T) : min.Value;
        }

        /// <summary>
        ///  Time complexity: O(log(n))
        /// </summary>
        public int IndexOf(T item)
        {
            return this.Root.Position(item);
        }

        /// <summary>
        ///  Time complexity: O(log(n))
        /// </summary>
        public T ElementAt(int index)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentNullException("index");
            }

            return this.Root.KthSmallest(index).Value;
        }

        internal RedBlackTreeNode<T> FindNode(T value)
        {
            return Root == null ? null : Find(value).Item1;
        }

        internal bool Exists(T value)
        {
            return FindNode(value) != null;
        }

        //find the node with the given identifier among descendants of parent and parent
        //uses pre-order traversal

        internal (RedBlackTreeNode<T>, int) Find(T value)
        {
            if (this.nodeLookUp != null)
            {
                if (this.nodeLookUp.ContainsKey(value))
                {
                    var node = (this.nodeLookUp[value] as RedBlackTreeNode<T>);
                    return (node, Root.Position(value));
                }

                return (null, -1);
            }

            var result = Root.Find(value);
            return (result.Item1 as RedBlackTreeNode<T>, result.Item2);
        }

        /// <summary>
        ///  Time complexity: O(log(n)).
        ///  Returns the position (index) of the value in sorted order of this BST.
        /// </summary>
        public int Insert(T value)
        {
            var node = InsertAndReturnNode(value);
            return node.Item2;
        }

        internal (RedBlackTreeNode<T>, int) InsertAndReturnNode(T value)
        {
            //empty tree
            if (Root == null)
            {
                Root = new RedBlackTreeNode<T>(null, value) { NodeColor = RedBlackTreeNodeColor.Black };
                if (this.nodeLookUp != null)
                {
                    this.nodeLookUp[value] = Root;
                }

                return (Root, 0);
            }

            var newNode = PrivateInsert(Root, value);

            if (this.nodeLookUp != null)
            {
                this.nodeLookUp[value] = newNode.Item1;
            }

            return newNode;
        }

        //O(log(n)) always
        private (RedBlackTreeNode<T>, int) PrivateInsert(RedBlackTreeNode<T> currentNode, T newNodeValue)
        {
            var insertionPosition = 0;

            while (true)
            {
                var compareResult = currentNode.Value.CompareTo(newNodeValue);

                //current node is less than new item
                if (compareResult < 0)
                {
                    insertionPosition += (currentNode.Left != null ? currentNode.Left.Count : 0) + 1;

                    //no right child
                    if (currentNode.Right == null)
                    {
                        //insert
                        var node = currentNode.Right = new RedBlackTreeNode<T>(currentNode, newNodeValue);
                        BalanceInsertion(currentNode.Right);
                        return (node, insertionPosition);
                    }

                    currentNode = currentNode.Right;
                }
                //current node is greater than new node
                else if (compareResult > 0)
                {
                    if (currentNode.Left == null)
                    {
                        //insert
                        var node = currentNode.Left = new RedBlackTreeNode<T>(currentNode, newNodeValue);
                        BalanceInsertion(currentNode.Left);
                        return (node, insertionPosition);
                    }

                    currentNode = currentNode.Left;
                }
                else
                {
                    //duplicate
                    throw new Exception("Item with same key exists");
                }
            }
        }

        private void BalanceInsertion(RedBlackTreeNode<T> nodeToBalance)
        {

            while (true)
            {
                if (nodeToBalance == Root)
                {
                    nodeToBalance.NodeColor = RedBlackTreeNodeColor.Black;
                    break;
                }

                //if node to balance is red
                if (nodeToBalance.NodeColor == RedBlackTreeNodeColor.Red)
                {
                    //red-red relation; fix it!
                    if (nodeToBalance.Parent.NodeColor == RedBlackTreeNodeColor.Red)
                    {
                        //red sibling
                        if (nodeToBalance.Parent.Sibling != null && nodeToBalance.Parent.Sibling.NodeColor == RedBlackTreeNodeColor.Red)
                        {
                            //mark both children of parent as black and move up balancing 
                            nodeToBalance.Parent.Sibling.NodeColor = RedBlackTreeNodeColor.Black;
                            nodeToBalance.Parent.NodeColor = RedBlackTreeNodeColor.Black;

                            //root is always black
                            if (nodeToBalance.Parent.Parent != Root)
                            {
                                nodeToBalance.Parent.Parent.NodeColor = RedBlackTreeNodeColor.Red;
                            }

                            nodeToBalance.UpdateCounts();
                            nodeToBalance.Parent.UpdateCounts();
                            nodeToBalance = nodeToBalance.Parent.Parent;
                        }
                        //absent sibling or black sibling
                        else if (nodeToBalance.Parent.Sibling == null || nodeToBalance.Parent.Sibling.NodeColor == RedBlackTreeNodeColor.Black)
                        {
                            if (nodeToBalance.IsLeftChild && nodeToBalance.Parent.IsLeftChild)
                            {
                                var newRoot = nodeToBalance.Parent;
                                SwapColors(nodeToBalance.Parent, nodeToBalance.Parent.Parent);
                                RightRotate(nodeToBalance.Parent.Parent);

                                if (newRoot == Root)
                                {
                                    Root.NodeColor = RedBlackTreeNodeColor.Black;
                                }

                                nodeToBalance.UpdateCounts();
                                nodeToBalance = newRoot;
                            }
                            else if (nodeToBalance.IsLeftChild && nodeToBalance.Parent.IsRightChild)
                            {
                                RightRotate(nodeToBalance.Parent);

                                var newRoot = nodeToBalance;

                                SwapColors(nodeToBalance.Parent, nodeToBalance);
                                LeftRotate(nodeToBalance.Parent);

                                if (newRoot == Root)
                                {
                                    Root.NodeColor = RedBlackTreeNodeColor.Black;
                                }

                                nodeToBalance.UpdateCounts();
                                nodeToBalance = newRoot;
                            }
                            else if (nodeToBalance.IsRightChild && nodeToBalance.Parent.IsRightChild)
                            {
                                var newRoot = nodeToBalance.Parent;
                                SwapColors(nodeToBalance.Parent, nodeToBalance.Parent.Parent);
                                LeftRotate(nodeToBalance.Parent.Parent);

                                if (newRoot == Root)
                                {
                                    Root.NodeColor = RedBlackTreeNodeColor.Black;
                                }

                                nodeToBalance.UpdateCounts();
                                nodeToBalance = newRoot;
                            }
                            else if (nodeToBalance.IsRightChild && nodeToBalance.Parent.IsLeftChild)
                            {
                                LeftRotate(nodeToBalance.Parent);

                                var newRoot = nodeToBalance;

                                SwapColors(nodeToBalance.Parent, nodeToBalance);
                                RightRotate(nodeToBalance.Parent);

                                if (newRoot == Root)
                                {
                                    Root.NodeColor = RedBlackTreeNodeColor.Black;
                                }

                                nodeToBalance.UpdateCounts();
                                nodeToBalance = newRoot;
                            }
                        }
                    }
                }

                if (nodeToBalance.Parent != null)
                {
                    nodeToBalance.UpdateCounts();
                    nodeToBalance = nodeToBalance.Parent;
                    continue;
                }

                break;
            }

            nodeToBalance.UpdateCounts(true);

        }

        private void SwapColors(RedBlackTreeNode<T> node1, RedBlackTreeNode<T> node2)
        {
            var tmpColor = node2.NodeColor;
            node2.NodeColor = node1.NodeColor;
            node1.NodeColor = tmpColor;
        }

        private void RightRotate(RedBlackTreeNode<T> node)
        {
            var prevRoot = node;
            var leftRightChild = prevRoot.Left.Right;

            var newRoot = node.Left;

            //make left child as root
            prevRoot.Left.Parent = prevRoot.Parent;

            if (prevRoot.Parent != null)
            {
                if (prevRoot.Parent.Left == prevRoot)
                {
                    prevRoot.Parent.Left = prevRoot.Left;
                }
                else
                {
                    prevRoot.Parent.Right = prevRoot.Left;
                }
            }

            //move prev root as right child of current root
            newRoot.Right = prevRoot;
            prevRoot.Parent = newRoot;

            //move right child of left child of prev root to left child of right child of new root
            newRoot.Right.Left = leftRightChild;
            if (newRoot.Right.Left != null)
            {
                newRoot.Right.Left.Parent = newRoot.Right;
            }

            if (prevRoot == Root)
            {
                Root = newRoot;
            }

            newRoot.Left.UpdateCounts();
            newRoot.Right.UpdateCounts();
            newRoot.UpdateCounts();
        }

        private void LeftRotate(RedBlackTreeNode<T> node)
        {
            var prevRoot = node;
            var rightLeftChild = prevRoot.Right.Left;

            var newRoot = node.Right;

            //make right child as root
            prevRoot.Right.Parent = prevRoot.Parent;

            if (prevRoot.Parent != null)
            {
                if (prevRoot.Parent.Left == prevRoot)
                {
                    prevRoot.Parent.Left = prevRoot.Right;
                }
                else
                {
                    prevRoot.Parent.Right = prevRoot.Right;
                }
            }

            //move prev root as left child of current root
            newRoot.Left = prevRoot;
            prevRoot.Parent = newRoot;

            //move left child of right child of prev root to right child of left child of new root
            newRoot.Left.Right = rightLeftChild;
            if (newRoot.Left.Right != null)
            {
                newRoot.Left.Right.Parent = newRoot.Left;
            }

            if (prevRoot == Root)
            {
                Root = newRoot;
            }

            newRoot.Left.UpdateCounts();
            newRoot.Right.UpdateCounts();
            newRoot.UpdateCounts();
        }

        public int Delete(T value)
        {
            if (this.Root == null)
            {
                return -1;
            }

            var node = Find(value);

            if (node.Item1 == null)
            {
                return -1;
            }

            var position = node.Item2;
            PrivateDelete(node.Item1);

            if (this.nodeLookUp != null)
            {
                this.nodeLookUp.Remove(value);
            }

            return position;
        }

        private void PrivateDelete(RedBlackTreeNode<T> node)
        {
            if (node.IsLeaf)
            {
                if (node.NodeColor == RedBlackTreeNodeColor.Red)
                {
                    DeleteLeaf(node);
                    node.Parent?.UpdateCounts(true);
                    return;
                }

                DeleteLeaf(node);
                BalanceNode(node.Parent);
            }
            else
            {
                //case one - right tree is null (move sub tree up)
                if (node.Left != null && node.Right == null)
                {
                    DeleteLeftNode(node);
                    BalanceNode(node.Left);
                }
                //case two - left tree is null  (move sub tree up)
                else if (node.Right != null && node.Left == null)
                {
                    DeleteRightNode(node);
                    BalanceNode(node.Right);
                }
                //case three - two child trees 
                //replace the node value with maximum element of left subtree (left max node)
                //and then delete the left max node
                else
                {
                    var maxLeftNode = FindMaxPrivate(node.Left);

                    if (this.nodeLookUp != null)
                    {
                        this.nodeLookUp[node.Value] = maxLeftNode;
                        this.nodeLookUp[maxLeftNode.Value] = node;
                    }

                    node.Value = maxLeftNode.Value;

                    //delete left max node
                    PrivateDelete(maxLeftNode);
                    return;
                }
            }
        }

        private void BalanceNode(RedBlackTreeNode<T> nodeToBalance)
        {
            //handle six cases
            while (nodeToBalance != null)
            {
                nodeToBalance.UpdateCounts();
                nodeToBalance = HandleDoubleBlack(nodeToBalance);
            }
        }

        private void DeleteLeaf(RedBlackTreeNode<T> node)
        {
            //if node is root
            if (node.Parent == null)
            {
                Root = null;
            }
            //assign nodes parent.left/right to null
            else if (node.IsLeftChild)
            {
                node.Parent.Left = null;
            }
            else
            {
                node.Parent.Right = null;
            }
        }

        private void DeleteRightNode(RedBlackTreeNode<T> node)
        {
            //root
            if (node.Parent == null)
            {
                Root.Right.Parent = null;
                Root = Root.Right;
                Root.NodeColor = RedBlackTreeNodeColor.Black;
                return;
            }

            //node is left child of parent
            if (node.IsLeftChild)
            {
                node.Parent.Left = node.Right;
            }
            //node is right child of parent
            else
            {
                node.Parent.Right = node.Right;
            }

            node.Right.Parent = node.Parent;

            if (node.Right.NodeColor != RedBlackTreeNodeColor.Red)
            {
                return;
            }

            //black deletion! But we can take its red child and recolor it to black
            //and we are done!
            node.Right.NodeColor = RedBlackTreeNodeColor.Black;

        }

        private void DeleteLeftNode(RedBlackTreeNode<T> node)
        {
            //root
            if (node.Parent == null)
            {
                Root.Left.Parent = null;
                Root = Root.Left;
                Root.NodeColor = RedBlackTreeNodeColor.Black;
                return;
            }

            //node is left child of parent
            if (node.IsLeftChild)
            {
                node.Parent.Left = node.Left;
            }
            //node is right child of parent
            else
            {
                node.Parent.Right = node.Left;
            }

            node.Left.Parent = node.Parent;

            if (node.Left.NodeColor != RedBlackTreeNodeColor.Red)
            {
                return;
            }

            //black deletion! But we can take its red child and recolor it to black
            //and we are done!
            node.Left.NodeColor = RedBlackTreeNodeColor.Black;
        }

        /// <summary>
        ///  Time complexity: O(log(n))
        /// </summary>
        public T RemoveAt(int index)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentException("index");
            }

            var node = Root.KthSmallest(index) as RedBlackTreeNode<T>;

            var deletedValue = node.Value;

            PrivateDelete(node);

            if (this.nodeLookUp != null)
            {
                this.nodeLookUp.Remove(deletedValue);
            }

            return node.Value;
        }

        private void rightRotate(RedBlackTreeNode<T> node)
        {
            var prevRoot = node;
            var leftRightChild = prevRoot.Left.Right;

            var newRoot = node.Left;

            //make left child as root
            prevRoot.Left.Parent = prevRoot.Parent;

            if (prevRoot.Parent != null)
            {
                if (prevRoot.Parent.Left == prevRoot)
                {
                    prevRoot.Parent.Left = prevRoot.Left;
                }
                else
                {
                    prevRoot.Parent.Right = prevRoot.Left;
                }
            }

            //move prev root as right child of current root
            newRoot.Right = prevRoot;
            prevRoot.Parent = newRoot;

            //move right child of left child of prev root to left child of right child of new root
            newRoot.Right.Left = leftRightChild;
            if (newRoot.Right.Left != null)
            {
                newRoot.Right.Left.Parent = newRoot.Right;
            }

            if (prevRoot == Root)
            {
                Root = newRoot;
            }

            newRoot.Left.UpdateCounts();
            newRoot.Right.UpdateCounts();
            newRoot.UpdateCounts();
        }

        private void leftRotate(RedBlackTreeNode<T> node)
        {
            var prevRoot = node;
            var rightLeftChild = prevRoot.Right.Left;

            var newRoot = node.Right;

            //make right child as root
            prevRoot.Right.Parent = prevRoot.Parent;

            if (prevRoot.Parent != null)
            {
                if (prevRoot.Parent.Left == prevRoot)
                {
                    prevRoot.Parent.Left = prevRoot.Right;
                }
                else
                {
                    prevRoot.Parent.Right = prevRoot.Right;
                }
            }

            //move prev root as left child of current root
            newRoot.Left = prevRoot;
            prevRoot.Parent = newRoot;

            //move left child of right child of prev root to right child of left child of new root
            newRoot.Left.Right = rightLeftChild;
            if (newRoot.Left.Right != null)
            {
                newRoot.Left.Right.Parent = newRoot.Left;
            }

            if (prevRoot == Root)
            {
                Root = newRoot;
            }

            newRoot.Left.UpdateCounts();
            newRoot.Right.UpdateCounts();
            newRoot.UpdateCounts();
        }

        private RedBlackTreeNode<T> HandleDoubleBlack(RedBlackTreeNode<T> node)
        {
            //case 1
            if (node == Root)
            {
                node.NodeColor = RedBlackTreeNodeColor.Black;
                return null;
            }

            //case 2
            if (node.Parent != null
                 && node.Parent.NodeColor == RedBlackTreeNodeColor.Black
                 && node.Sibling != null
                 && node.Sibling.NodeColor == RedBlackTreeNodeColor.Red
                 && ((node.Sibling.Left == null && node.Sibling.Right == null)
                 || (node.Sibling.Left != null && node.Sibling.Right != null
                   && node.Sibling.Left.NodeColor == RedBlackTreeNodeColor.Black
                   && node.Sibling.Right.NodeColor == RedBlackTreeNodeColor.Black)))
            {
                node.Parent.NodeColor = RedBlackTreeNodeColor.Red;
                node.Sibling.NodeColor = RedBlackTreeNodeColor.Black;

                if (node.Sibling.IsRightChild)
                {
                    leftRotate(node.Parent);
                }
                else
                {
                    rightRotate(node.Parent);
                }

                return node;
            }
            //case 3
            if (node.Parent != null
             && node.Parent.NodeColor == RedBlackTreeNodeColor.Black
             && node.Sibling != null
             && node.Sibling.NodeColor == RedBlackTreeNodeColor.Black
             && (node.Sibling.Left == null && node.Sibling.Right == null
             || node.Sibling.Left != null && node.Sibling.Right != null
                                          && node.Sibling.Left.NodeColor == RedBlackTreeNodeColor.Black
                                          && node.Sibling.Right.NodeColor == RedBlackTreeNodeColor.Black))
            {
                //pushed up the double black problem up to parent
                //so now it needs to be fixed
                node.Sibling.NodeColor = RedBlackTreeNodeColor.Red;

                return node.Parent;
            }


            //case 4
            if (node.Parent != null
                 && node.Parent.NodeColor == RedBlackTreeNodeColor.Red
                 && node.Sibling != null
                 && node.Sibling.NodeColor == RedBlackTreeNodeColor.Black
                 && (node.Sibling.Left == null && node.Sibling.Right == null
                 || node.Sibling.Left != null && node.Sibling.Right != null
                                              && node.Sibling.Left.NodeColor == RedBlackTreeNodeColor.Black
                                              && node.Sibling.Right.NodeColor == RedBlackTreeNodeColor.Black))
            {
                //just swap the color of parent and sibling
                //which will compensate the loss of black count 
                node.Parent.NodeColor = RedBlackTreeNodeColor.Black;
                node.Sibling.NodeColor = RedBlackTreeNodeColor.Red;
                node.UpdateCounts(true);
                return null;
            }


            //case 5
            if (node.Parent != null
                && node.Parent.NodeColor == RedBlackTreeNodeColor.Black
                && node.Sibling != null
                && node.Sibling.IsRightChild
                && node.Sibling.NodeColor == RedBlackTreeNodeColor.Black
                && node.Sibling.Left != null
                && node.Sibling.Left.NodeColor == RedBlackTreeNodeColor.Red
                && node.Sibling.Right != null
                && node.Sibling.Right.NodeColor == RedBlackTreeNodeColor.Black)
            {
                node.Sibling.NodeColor = RedBlackTreeNodeColor.Red;
                node.Sibling.Left.NodeColor = RedBlackTreeNodeColor.Black;
                rightRotate(node.Sibling);

                return node;
            }

            //case 5 mirror
            if (node.Parent != null
               && node.Parent.NodeColor == RedBlackTreeNodeColor.Black
               && node.Sibling != null
               && node.Sibling.IsLeftChild
               && node.Sibling.NodeColor == RedBlackTreeNodeColor.Black
               && node.Sibling.Left != null
               && node.Sibling.Left.NodeColor == RedBlackTreeNodeColor.Black
               && node.Sibling.Right != null
               && node.Sibling.Right.NodeColor == RedBlackTreeNodeColor.Red)
            {
                node.Sibling.NodeColor = RedBlackTreeNodeColor.Red;
                node.Sibling.Right.NodeColor = RedBlackTreeNodeColor.Black;
                leftRotate(node.Sibling);

                return node;
            }

            //case 6
            if (node.Parent != null
                && node.Parent.NodeColor == RedBlackTreeNodeColor.Black
                && node.Sibling != null
                && node.Sibling.IsRightChild
                && node.Sibling.NodeColor == RedBlackTreeNodeColor.Black
                && node.Sibling.Right != null
                && node.Sibling.Right.NodeColor == RedBlackTreeNodeColor.Red)
            {
                //left rotate to increase the black count on left side by one
                //and mark the red right child of sibling to black 
                //to compensate the loss of Black on right side of parent
                node.Sibling.Right.NodeColor = RedBlackTreeNodeColor.Black;
                leftRotate(node.Parent);
                node.UpdateCounts(true);
                return null;
            }

            //case 6 mirror
            if (node.Parent != null
              && node.Parent.NodeColor == RedBlackTreeNodeColor.Black
              && node.Sibling != null
              && node.Sibling.IsLeftChild
              && node.Sibling.NodeColor == RedBlackTreeNodeColor.Black
              && node.Sibling.Left != null
              && node.Sibling.Left.NodeColor == RedBlackTreeNodeColor.Red)
            {
                //right rotate to increase the black count on right side by one
                //and mark the red left child of sibling to black
                //to compensate the loss of Black on right side of parent
                node.Sibling.Left.NodeColor = RedBlackTreeNodeColor.Black;
                rightRotate(node.Parent);
                node.UpdateCounts(true);
                return null;
            }

            node.UpdateCounts(true);
            return null;
        }

        private void assignColors(RedBlackTreeNode<T> current)
        {
            if (current == null)
            {
                return;
            }

            assignColors(current.Left);
            assignColors(current.Right);

            if (current.IsLeaf)
            {
                current.NodeColor = RedBlackTreeNodeColor.Red;
            }
            else 
            {
                current.NodeColor = RedBlackTreeNodeColor.Black;
            }
        }

        /// <summary>
        ///     Get the next lower value to given value in this BST.
        /// </summary>
        public T NextLower(T value)
        {
            var node = FindNode(value);
            if (node == null)
            {
                return default(T);
            }

            var next = (node as BSTNodeBase<T>).NextLower();
            return next != null ? next.Value : default(T);
        }

        /// <summary>
        ///     Get the next higher to given value in this BST.
        /// </summary>
        public T NextHigher(T value)
        {
            var node = FindNode(value);
            if (node == null)
            {
                return default(T);
            }

            var next = (node as BSTNodeBase<T>).NextHigher();
            return next != null ? next.Value : default(T);
        }


        /// <summary>
        /// Descending enumerable.
        /// </summary>
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new BSTEnumerator<T>(Root);
        }

        public IEnumerator<T> GetEnumeratorDesc()
        {
            return new BSTEnumerator<T>(Root, false);
        }
    }
}
