using System.Collections;

namespace RedBlack;

public class RedBlackTree<T> : IEnumerable<T>
    where T : IComparable<T>
{
    private enum Color
    {
        Red,
        Black
    }

    private class Node
    {
        public T Key;
        public Color Color;
        public Node Parent;
        public Node Left;
        public Node Right;
            
        // -----------------------------
        // Extended Fields for Order Stats
        // -----------------------------
            
        /// <summary>
        /// Number of duplicates of this key. By default = 1 for a new node.
        /// If we insert the same key multiple times, we increment this count.
        /// </summary>
        public int DuplicateCount;

        /// <summary>
        /// The size of the subtree = Left.SubtreeSize + Right.SubtreeSize + DuplicateCount
        /// </summary>
        public int SubtreeSize;

        // Some helpers
        public bool IsRed => Color == Color.Red;
        public bool IsBlack => Color == Color.Black;

        // Sentinel constructor
        public Node()
        {
            Key = default!;
            Color = Color.Black;
            Parent = this;
            Left = this;
            Right = this;
            DuplicateCount = 0;
            SubtreeSize = 0;
        }

        // Normal node constructor
        public Node(T key, Node nil)
        {
            Key = key;
            Color = Color.Red; // new nodes start as Red
            Parent = nil;
            Left = nil;
            Right = nil;
            DuplicateCount = 1;       // 1 occurrence by default
            SubtreeSize = 1;         // subtree has 1 element by default
        }
    }

    private readonly Node Nil;  // Sentinel node
    private Node Root;          // If empty, Root = Nil

    public int Count => Root.SubtreeSize; // Or track separately, but here Root.SubtreeSize is total elements

    public RedBlackTree()
    {
        Nil = new Node();  // sentinel
        Root = Nil;
    }

    // ------------------------------------------------------------------
    //  INSERT
    //  - If key found, increment DuplicateCount, update subtree sizes up
    //  - If new node, do normal RB insertion + fix-up
    // ------------------------------------------------------------------

    public void Insert(T key)
    {
        Node current = Root;
        Node parent = Nil;

        // 1) Standard BST find
        while (current != Nil)
        {
            parent = current;
            int cmp = key.CompareTo(current.Key);

            // We also update subtreeSize on the path for any new insertion
            // Because eventually we might create a new node down here
            // or increment an existing node's DuplicateCount.
            current.SubtreeSize++;

            if (cmp == 0)
            {
                // The key already exists => increment duplicate count
                current.DuplicateCount++;
                // no rotations/fix-up needed since the tree shape hasn't changed
                return;
            }

            current = cmp < 0 ? current.Left : current.Right;
        }

        // 2) If we exit the loop, 'current' = Nil => we have a new node
        Node newNode = new Node(key, Nil)
        {
            Parent = parent
        };

        if (parent == Nil)
        {
            // The tree was empty
            Root = newNode;
        }
        else if (key.CompareTo(parent.Key) < 0)
        {
            parent.Left = newNode;
        }
        else
        {
            parent.Right = newNode;
        }

        // Fix-up for color conflicts
        InsertFixUp(newNode);

        // After fix-up, we might need to ensure SubtreeSize is correct up the chain
        // But we've already incremented for the path to newNode.
        // The rotations themselves will fix SubtreeSize where needed, 
        // or we can do a local fix after each rotation. 
    }

    private void InsertFixUp(Node z)
    {
        while (z.Parent.IsRed)
        {
            if (z.Parent == z.Parent.Parent.Left)
            {
                Node y = z.Parent.Parent.Right;
                if (y.IsRed)
                {
                    // Case 1
                    z.Parent.Color = Color.Black;
                    y.Color = Color.Black;
                    z.Parent.Parent.Color = Color.Red;
                    z = z.Parent.Parent;
                }
                else
                {
                    // Case 2 or 3
                    if (z == z.Parent.Right)
                    {
                        // Case 2
                        z = z.Parent;
                        RotateLeft(z);
                    }
                    // Case 3
                    z.Parent.Color = Color.Black;
                    z.Parent.Parent.Color = Color.Red;
                    RotateRight(z.Parent.Parent);
                }
            }
            else
            {
                // Mirror
                Node y = z.Parent.Parent.Left;
                if (y.IsRed)
                {
                    z.Parent.Color = Color.Black;
                    y.Color = Color.Black;
                    z.Parent.Parent.Color = Color.Red;
                    z = z.Parent.Parent;
                }
                else
                {
                    if (z == z.Parent.Left)
                    {
                        z = z.Parent;
                        RotateRight(z);
                    }
                    z.Parent.Color = Color.Black;
                    z.Parent.Parent.Color = Color.Red;
                    RotateLeft(z.Parent.Parent);
                }
            }
        }
        Root.Color = Color.Black;
    }

    // ------------------------------------------------------------------
    //  DELETE
    // ------------------------------------------------------------------

    public bool Delete(T key)
    {
        Node z = FindNode(key);
        if (z == Nil) return false;

        // If node has DuplicateCount > 1, just decrement
        if (z.DuplicateCount > 1)
        {
            // We only reduce the subtree size by 1 going up
            z.DuplicateCount--;
            // Move up the chain to fix SubtreeSize
            var cur = z;
            while (cur != Nil)
            {
                cur.SubtreeSize--;
                cur = cur.Parent;
            }
            return true;
        }

        DeleteNode(z);
        return true;
    }

    private void DeleteNode(Node z)
    {
        Node y = z;
        Node x;
        var yOriginalColor = y.Color;

        if (z.Left == Nil)
        {
            x = z.Right;
            Transplant(z, z.Right);
        }
        else if (z.Right == Nil)
        {
            x = z.Left;
            Transplant(z, z.Left);
        }
        else
        {
            y = Minimum(z.Right);
            yOriginalColor = y.Color;
            x = y.Right;
            if (y.Parent == z)
            {
                x.Parent = y;
            }
            else
            {
                Transplant(y, y.Right);
                y.Right = z.Right;
                y.Right.Parent = y;
                RecalcSize(y);
            }
            Transplant(z, y);
            y.Left = z.Left;
            y.Left.Parent = y;
            y.Color = z.Color;

            // We took over z's duplicateCount
            y.DuplicateCount = z.DuplicateCount;

            // We must recalc size for y (since it now has new children)
            RecalcSize(y);
        }

        // Decrement SubtreeSize up the chain from z.Parent
        {
            var cur = z.Parent;
            while (cur != Nil)
            {
                cur.SubtreeSize--;
                cur = cur.Parent;
            }
        }

        if (yOriginalColor == Color.Black)
        {
            DeleteFixUp(x);
        }
    }

    private void DeleteFixUp(Node x)
    {
        while (x != Root && x.IsBlack)
        {
            if (x == x.Parent.Left)
            {
                Node w = x.Parent.Right;
                if (w.IsRed)
                {
                    w.Color = Color.Black;
                    x.Parent.Color = Color.Red;
                    RotateLeft(x.Parent);
                    w = x.Parent.Right;
                }
                if (w.Left.IsBlack && w.Right.IsBlack)
                {
                    w.Color = Color.Red;
                    x = x.Parent;
                }
                else
                {
                    if (w.Right.IsBlack)
                    {
                        w.Left.Color = Color.Black;
                        w.Color = Color.Red;
                        RotateRight(w);
                        w = x.Parent.Right;
                    }
                    w.Color = x.Parent.Color;
                    x.Parent.Color = Color.Black;
                    w.Right.Color = Color.Black;
                    RotateLeft(x.Parent);
                    x = Root;
                }
            }
            else
            {
                Node w = x.Parent.Left;
                if (w.IsRed)
                {
                    w.Color = Color.Black;
                    x.Parent.Color = Color.Red;
                    RotateRight(x.Parent);
                    w = x.Parent.Left;
                }
                if (w.Right.IsBlack && w.Left.IsBlack)
                {
                    w.Color = Color.Red;
                    x = x.Parent;
                }
                else
                {
                    if (w.Left.IsBlack)
                    {
                        w.Right.Color = Color.Black;
                        w.Color = Color.Red;
                        RotateLeft(w);
                        w = x.Parent.Left;
                    }
                    w.Color = x.Parent.Color;
                    x.Parent.Color = Color.Black;
                    w.Left.Color = Color.Black;
                    RotateRight(x.Parent);
                    x = Root;
                }
            }
        }
        x.Color = Color.Black;
    }

    // ------------------------------------------------------------------
    //  ROTATIONS (Left / Right)
    //  We must re-calc subtree sizes after each rotation
    // ------------------------------------------------------------------

    private void RotateLeft(Node x)
    {
        Node y = x.Right;
        x.Right = y.Left;
        if (y.Left != Nil)
        {
            y.Left.Parent = x;
        }
        y.Parent = x.Parent;
        if (x.Parent == Nil)
        {
            Root = y;
        }
        else if (x == x.Parent.Left)
        {
            x.Parent.Left = y;
        }
        else
        {
            x.Parent.Right = y;
        }
        y.Left = x;
        x.Parent = y;

        // Recalc sizes
        RecalcSize(x);
        RecalcSize(y);
    }

    private void RotateRight(Node x)
    {
        Node y = x.Left;
        x.Left = y.Right;
        if (y.Right != Nil)
        {
            y.Right.Parent = x;
        }
        y.Parent = x.Parent;
        if (x.Parent == Nil)
        {
            Root = y;
        }
        else if (x == x.Parent.Right)
        {
            x.Parent.Right = y;
        }
        else
        {
            x.Parent.Left = y;
        }
        y.Right = x;
        x.Parent = y;

        // Recalc sizes
        RecalcSize(x);
        RecalcSize(y);
    }

    /// <summary>
    /// Recalculates the SubtreeSize of a node from its children + own DuplicateCount.
    /// </summary>
    private void RecalcSize(Node x)
    {
        if (x == Nil) return;
        x.SubtreeSize = x.Left.SubtreeSize + x.Right.SubtreeSize + x.DuplicateCount;
    }

    private void Transplant(Node u, Node v)
    {
        if (u.Parent == Nil)
        {
            Root = v;
        }
        else if (u == u.Parent.Left)
        {
            u.Parent.Left = v;
        }
        else
        {
            u.Parent.Right = v;
        }
        v.Parent = u.Parent;
    }

    private Node FindNode(T key)
    {
        Node current = Root;
        while (current != Nil)
        {
            int cmp = key.CompareTo(current.Key);
            if (cmp == 0) return current;
            current = (cmp < 0) ? current.Left : current.Right;
        }
        return Nil;
    }

    private Node Minimum(Node x)
    {
        while (x.Left != Nil)
        {
            x = x.Left;
        }
        return x;
    }

    // ------------------------------------------------------------------
    //  RANK & SELECT (Order Statistics)
    // ------------------------------------------------------------------

    /// <summary>
    /// Returns the k-th smallest element (1-based).
    /// Throws if k is out of range.
    /// </summary>
    public T Select(int k)
    {
        if (k < 1 || k > Count)
            throw new ArgumentOutOfRangeException(nameof(k), "k is out of range");

        return Select(Root, k);
    }

    private T Select(Node x, int k)
    {
        // Number of elements in the left subtree
        int leftSize = x.Left.SubtreeSize;
            
        // If k is in the left subtree
        if (k <= leftSize)
        {
            return Select(x.Left, k);
        }

        // If k is in the node's duplicates
        // leftSize < k <= leftSize + x.DuplicateCount
        if (k <= leftSize + x.DuplicateCount)
        {
            return x.Key;
        }

        // Else it's in the right subtree
        return Select(x.Right, k - leftSize - x.DuplicateCount);
    }

    /// <summary>
    /// Returns how many elements are strictly less than 'key'.
    /// If you want "less or equal," adjust the logic accordingly.
    /// </summary>
    public int Rank(T key)
    {
        return Rank(Root, key);
    }

    private int Rank(Node x, T key)
    {
        // If we reach Nil, rank is 0
        if (x == Nil) return 0;

        int cmp = key.CompareTo(x.Key);
        if (cmp < 0)
        {
            // All elements in right subtree are >= x.Key,
            // so rank is entirely in left subtree
            return Rank(x.Left, key);
        }

        if (cmp > 0)
        {
            // If key > x.Key, then rank is:
            // left subtree size + duplicates of x + rank in right subtree
            int leftSize = x.Left.SubtreeSize;
            return leftSize + x.DuplicateCount + Rank(x.Right, key);
        }

        // key == x.Key => the rank is everything in the left subtree
        return x.Left.SubtreeSize;
    }

    // ------------------------------------------------------------------
    //  API: Contains, Iterate, Print, etc.
    // ------------------------------------------------------------------

    /// <summary>
    /// Whether the tree contains 'key' (ignores duplicates, i.e., returns true if any node has that key).
    /// </summary>
    public bool Contains(T key) => FindNode(key) != Nil;

    /// <summary>
    /// In-order traversal enumerator (ascending order).
    /// Each key is repeated DuplicateCount times.
    /// </summary>
    public IEnumerator<T> GetEnumerator()
    {
        var stack = new Stack<Node>();
        Node current = Root;

        while (stack.Count > 0 || current != Nil)
        {
            if (current != Nil)
            {
                stack.Push(current);
                current = current.Left;
            }
            else
            {
                current = stack.Pop();
                // yield the key DuplicateCount times
                for (int i = 0; i < current.DuplicateCount; i++)
                {
                    yield return current.Key;
                }
                current = current.Right;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Debug print in in-order.
    /// Shows (key[color]:count, subtreeSize).
    /// </summary>
    public void PrintInOrder()
    {
        PrintInOrder(Root);
        Console.WriteLine();
    }

    private void PrintInOrder(Node x)
    {
        if (x == Nil) return;
        PrintInOrder(x.Left);
        Console.Write($"{x.Key}({x.Color}):{x.DuplicateCount}[{x.SubtreeSize}] ");
        PrintInOrder(x.Right);
    }
}
