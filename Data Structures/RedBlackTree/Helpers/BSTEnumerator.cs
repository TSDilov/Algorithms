using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedBlackTree
{
    internal class BSTEnumerator<T> : IEnumerator<T> where T : IComparable
    {
        private readonly bool asc;
        private readonly BSTNodeBase<T> root;
        private BSTNodeBase<T> current;

        internal BSTEnumerator(BSTNodeBase<T> root, bool asc = true)
        {
            this.root = root;
            this.asc = asc;
        }
        public T Current => this.current.Value;

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            this.current = null;
        }

        public bool MoveNext()
        {
            if (this.root == null)
            {
                return false;
            }

            if (this.current == null)
            {
                this.current = asc ? root.FindMin() : root.FindMax();
                return true;
            }

            var next = asc ? current.NextHigher() : current.NextLower();

            if (next != null)
            {
                this.current = next;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            this.current = this.root;
        }
    }
}
