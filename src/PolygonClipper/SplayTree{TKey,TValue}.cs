// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

#nullable enable

using System.Collections.Generic;

namespace PolygonClipper;

/// <summary>
/// A Splay Tree implementation that provides efficient access and updates while maintaining a balanced structure.
/// </summary>
/// <typeparam name="TKey">The type of keys in the tree.</typeparam>
/// <typeparam name="TValue">The type of values associated with the keys.</typeparam>
public class SplayTree<TKey, TValue>
{
    private readonly IComparer<TKey> comparer;
    private Node? root;
    private readonly bool noDuplicates;

    /// <summary>
    /// Initializes a new instance of the <see cref="SplayTree{TKey, TValue}"/> class.
    /// </summary>
    /// <param name="comparer">The comparer to use for key comparison.</param>
    /// <param name="noDuplicates">Indicates whether duplicate keys are allowed.</param>
    public SplayTree(IComparer<TKey>? comparer = null, bool noDuplicates = false)
    {
        this.comparer = comparer ?? Comparer<TKey>.Default;
        this.noDuplicates = noDuplicates;
    }

    /// <summary>
    /// Gets the number of elements in the tree.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// Inserts a key-value pair into the tree, maintaining the splay tree structure.
    /// </summary>
    /// <param name="key">The key to insert.</param>
    /// <param name="value">The value to associate with the key.</param>
    /// <returns>The inserted node, or null if the insertion was skipped due to a duplicate key.</returns>
    public Node Insert(TKey key, TValue? value = default)
    {
        Node? z = this.root;
        Node? p = null;
        IComparer<TKey> comp = this.comparer;
        int cmp;

        // If duplicates are not allowed
        if (this.noDuplicates)
        {
            while (z != null)
            {
                p = z;
                cmp = comp.Compare(z.Key, key);

                if (cmp == 0)
                {
                    // If the key already exists, return without inserting
                    return z;
                }
                else if (cmp < 0)
                {
                    z = z.Right;
                }
                else
                {
                    z = z.Left;
                }
            }
        }
        else
        {
            while (z != null)
            {
                p = z;
                if (comp.Compare(z.Key, key) < 0)
                {
                    z = z.Right;
                }
                else
                {
                    z = z.Left;
                }
            }
        }

        // Create the new node and attach it to the parent
        z = new Node(key, value, p);

        if (p == null)
        {
            this.root = z; // The tree was empty, new node becomes root
        }
        else if (comp.Compare(p.Key, z.Key) < 0)
        {
            p.Right = z;
        }
        else
        {
            p.Left = z;
        }

        // Splay the new node to bring it to the root
        this.Splay(z);

        this.Count++;
        return z;
    }

    /// <summary>
    /// Removes a node with the specified key from the tree.
    /// </summary>
    /// <param name="key">The key to remove.</param>
    /// <returns><c>true</c> if the node was removed; otherwise, <c>false</c>.</returns>
    public bool Remove(TKey key)
    {
        Node? z = this.Find(key);
        if (z == null)
        {
            return false;
        }

        this.Splay(z);

        if (z.Left == null)
        {
            this.Replace(z, z.Right);
        }
        else if (z.Right == null)
        {
            this.Replace(z, z.Left);
        }
        else
        {
            Node? y = this.MinNode(z.Right);
            if (y != null)
            {
                if (y.Parent != z)
                {
                    this.Replace(y, y.Right);
                    y.Right = z.Right;
                    y.Right.Parent = y;
                }

                this.Replace(z, y);
                y.Left = z.Left;
                y.Left.Parent = y;
            }
        }

        this.Count--;
        return true;
    }

    /// <summary>
    /// Finds a node with the specified key.
    /// </summary>
    /// <param name="key">The key to find.</param>
    /// <returns>The node with the specified key, or null if not found.</returns>
    public Node? Find(TKey key)
    {
        Node? z = this.root;
        IComparer<TKey> comp = this.comparer;
        while (z != null)
        {
            int cmp = comp.Compare(key, z.Key);
            if (cmp < 0)
            {
                z = z.Right;
            }
            else if (cmp > 0)
            {
                z = z.Left;
            }
            else
            {
                return z;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the node with the smallest key starting from the specified node.
    /// </summary>
    /// <param name="startNode">The node to start the search from. If null, starts from the root.</param>
    /// <returns>The node with the smallest key, or null if the tree is empty.</returns>
    public Node? MinNode(Node? startNode = null)
    {
        Node? current = startNode ?? this.root;
        while (current?.Left != null)
        {
            current = current.Left;
        }

        return current;
    }

    /// <summary>
    /// Gets the node with the largest key starting from the specified node.
    /// </summary>
    /// <param name="startNode">The node to start the search from. If null, starts from the root.</param>
    /// <returns>The node with the largest key, or null if the tree is empty.</returns>
    public Node? MaxNode(Node? startNode = null)
    {
        Node? current = startNode ?? this.root;
        while (current?.Right != null)
        {
            current = current.Right;
        }

        return current;
    }

    /// <summary>
    /// Gets the successor node (the next node in sorted order) of the given node.
    /// </summary>
    /// <param name="node">The reference node.</param>
    /// <returns>The successor node, or <c>null</c> if no successor exists.</returns>
    public Node? Next(Node? node)
    {
        Node? successor = node;

        if (successor != null)
        {
            if (successor.Right != null)
            {
                // Move to the right child and then to the leftmost descendant
                successor = successor.Right;
                while (successor.Left != null)
                {
                    successor = successor.Left;
                }
            }
            else
            {
                // Move up the tree until we find a node that is not a right child
                Node? parent = successor.Parent;
                while (parent != null && parent.Right == successor)
                {
                    successor = parent;
                    parent = parent.Parent;
                }

                successor = parent;
            }
        }

        return successor;
    }

    /// <summary>
    /// Gets the predecessor node (the previous node in sorted order) of the given node.
    /// </summary>
    /// <param name="node">The reference node.</param>
    /// <returns>The predecessor node, or <c>null</c> if no predecessor exists.</returns>
    public Node? Previous(Node? node)
    {
        Node? predecessor = node;

        if (predecessor != null)
        {
            if (predecessor.Left != null)
            {
                // Move to the left child and then to the rightmost descendant
                predecessor = predecessor.Left;
                while (predecessor.Right != null)
                {
                    predecessor = predecessor.Right;
                }
            }
            else
            {
                // Move up the tree until we find a node that is not a left child
                Node? parent = predecessor.Parent;
                while (parent != null && parent.Left == predecessor)
                {
                    predecessor = parent;
                    parent = parent.Parent;
                }

                predecessor = parent;
            }
        }

        return predecessor;
    }

    private void Replace(Node? u, Node? v)
    {
        if (u?.Parent == null)
        {
            this.root = v;
        }
        else if (u == u.Parent.Left)
        {
            u.Parent.Left = v;
        }
        else
        {
            u.Parent.Right = v;
        }

        if (v != null)
        {
            v.Parent = u?.Parent;
        }
    }

    /// <summary>
    /// Splays the given node to the root of the tree.
    /// </summary>
    /// <param name="x">The node to splay.</param>
    private void Splay(Node x)
    {
        while (x.Parent != null)
        {
            Node p = x.Parent;
            Node? gp = p.Parent;

            if (gp?.Parent != null)
            {
                // Assign ggp as the grandparent's parent
                Node ggp = gp.Parent;
                if (ggp.Left == gp)
                {
                    ggp.Left = x;
                }
                else
                {
                    ggp.Right = x;
                }

                x.Parent = ggp;
            }
            else
            {
                // No grandparent, make x the root
                x.Parent = null;
                this.root = x;
            }

            Node? l = x.Left;
            Node? r = x.Right;

            if (x == p.Left)
            {
                // Left case
                if (gp != null)
                {
                    if (gp.Left == p)
                    {
                        /* Zig-zig */
                        if (p.Right != null)
                        {
                            gp.Left = p.Right;
                            gp.Left.Parent = gp;
                        }
                        else
                        {
                            gp.Left = null;
                        }

                        p.Right = gp;
                        gp.Parent = p;
                    }
                    else
                    {
                        /* Zig-zag */
                        if (l != null)
                        {
                            gp.Right = l;
                            l.Parent = gp;
                        }
                        else
                        {
                            gp.Right = null;
                        }

                        x.Left = gp;
                        gp.Parent = x;
                    }
                }

                if (r != null)
                {
                    p.Left = r;
                    r.Parent = p;
                }
                else
                {
                    p.Left = null;
                }

                x.Right = p;
                p.Parent = x;
            }
            else
            {
                // Right case
                if (gp != null)
                {
                    if (gp.Right == p)
                    {
                        /* Zig-zig */
                        if (p.Left != null)
                        {
                            gp.Right = p.Left;
                            gp.Right.Parent = gp;
                        }
                        else
                        {
                            gp.Right = null;
                        }

                        p.Left = gp;
                        gp.Parent = p;
                    }
                    else
                    {
                        /* Zig-zag */
                        if (r != null)
                        {
                            gp.Left = r;
                            r.Parent = gp;
                        }
                        else
                        {
                            gp.Left = null;
                        }

                        x.Right = gp;
                        gp.Parent = x;
                    }
                }

                if (l != null)
                {
                    p.Right = l;
                    l.Parent = p;
                }
                else
                {
                    p.Right = null;
                }

                x.Left = p;
                p.Parent = x;
            }
        }
    }

    /// <summary>
    /// Represents a node in the splay tree.
    /// </summary>
    public sealed class Node
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Node"/> class.
        /// </summary>
        /// <param name="key">The key associated with the node.</param>
        /// <param name="value">The value associated with the key. Can be <c>null</c>.</param>
        /// <param name="parent">The parent node of this node. Defaults to <c>null</c> for the root node.</param>
        public Node(TKey key, TValue? value, Node? parent = null)
        {
            this.Key = key;
            this.Value = value;
            this.Parent = parent;
        }

        /// <summary>
        /// Gets the key associated with this node.
        /// </summary>
        public TKey Key { get; }

        /// <summary>
        /// Gets the value associated with this node. Can be <c>null</c>.
        /// </summary>
        public TValue? Value { get; }

        /// <summary>
        /// Gets or sets the parent node of this node.
        /// </summary>
        public Node? Parent { get; set; }

        /// <summary>
        /// Gets or sets the left child node of this node.
        /// </summary>
        public Node? Left { get; set; }

        /// <summary>
        /// Gets or sets the right child node of this node.
        /// </summary>
        public Node? Right { get; set; }
    }
}
