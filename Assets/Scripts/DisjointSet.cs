using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Disjoint Sets using union by rank and path compression.
/// </summary>
/// <typeparam name="T">The type of the data contained in the disjoint set.</typeparam>
public class DisjointSet<T> : IEnumerable<T> where T : IEquatable<T>
{
    /// <summary>
    /// A node of the internal tree structure, representing a set in the disjoint set.
    /// It holds some data, a rank, and a reference to its parent.
    /// If the parent is itself, it's a root node, i.e. the set representative.
    /// </summary>
    private class Node
    {
        /// <summary>
        /// The data that the node holds.
        /// </summary>
        public T Data { get; }

        /// <summary>
        /// The parent node of this node. If the parent is itself, it's a root node, i.e. the set representative.
        /// </summary>
        public Node Parent { get; set; }

        /// <summary>
        /// The rank of node.
        /// The rank starts at 0. If two nodes of the same rank are merged in a union,
        /// one is chosen arbitrarily and becomes the parent node,
        /// and the rank of the parent node increases with one.
        /// If two nodes of different rank are merged in a union,
        /// the node with the highest rank becomes the parent node.
        /// </summary>
        /// <seealso cref="DisjointSet{T}.Union"/>
        public int Rank { get; set; }

        /// <summary>
        /// Constructs a new node. It becomes a new set with itself as only member and set representative.
        /// </summary>
        /// <param name="data">The data that the node should contain.</param>
        public Node(T data)
        {
            Data = data;
            Parent = this;
            Rank = 0;
        }
    }

    private readonly Dictionary<T, Node> nodes;

    /// <summary>
    /// Constructs a new empty <see cref="DisjointSet{T}"/>.
    /// </summary>
    public DisjointSet()
    {
       nodes = new Dictionary<T, Node>();
    }

    /// <summary>
    /// Constructs a new empty <see cref="DisjointSet{T}"/>, with an initial capacity.
    /// </summary>
    /// <param name="capacity">The initial number of elements that the <see cref="DisjointSet{T}"/> can contain.</param>
    public DisjointSet(int capacity)
    {
        nodes = new Dictionary<T, Node>(capacity);
    }

    /// <summary>
    /// Ensures that the <see cref="DisjointSet{T}"/> can hold up to a specified number of elements
    /// without any further expansion of its backing storage.
    /// </summary>
    /// <param name="capacity">The number of elements.</param>
    public void EnsureCapacity(int capacity)
    {
        nodes.EnsureCapacity(capacity);
    }

    /// <summary>
    /// Gets the number of elements in the disjoint set.
    /// </summary>
    public int Count => nodes.Count;

    /// <summary>
    /// Returns an enumerator that iterates through a <see cref="DisjointSet{T}"/>.
    /// </summary>
    /// <returns>An IEnumerator object that can be used to iterate through the <see cref="DisjointSet{T}"/>.</returns>
    public IEnumerator<T> GetEnumerator()
    {
        return nodes.Keys.GetEnumerator();
    }

    /// <summary>
    /// Returns an enumerator that iterates through a <see cref="DisjointSet{T}"/>.
    /// </summary>
    /// <returns>An IEnumerator object that can be used to iterate through the <see cref="DisjointSet{T}"/>.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return nodes.Keys.GetEnumerator();
    }

    /// <summary>
    /// Determines whether the <see cref="DisjointSet{T}"/> contains the specified data.
    /// </summary>
    /// <param name="data">The data to check for.</param>
    /// <returns>
    /// <see langword="true"/> if the <see cref="DisjointSet{T}"/> contains the specified <paramref name="data"/>,
    /// otherwise <see langword="false"/>.
    /// </returns>
    public bool ContainsData(T data)
    {
        return nodes.ContainsKey(data);
    }

    /// <summary>
    /// Determines whether the <see cref="DisjointSet{T}"/> contains any elements.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the <see cref="DisjointSet{T}"/> contains no elements,
    /// otherwise <see langword="false"/>.
    /// </returns>
    public bool IsEmpty()
    {
        return Count == 0;
    }

    /// <summary>
    /// Removes all data from the <see cref="DisjointSet{T}"/>.
    /// </summary>
    public void Clear()
    {
        nodes.Clear();
    }

    /// <summary>
    /// Makes a new set containing <paramref name="data"/>, and adds it to the <see cref="DisjointSet{T}"/>.
    /// </summary>
    /// <param name="data">The data the set should contain.</param>
    /// <returns>
    /// <see langword="false"/> if the <see cref="DisjointSet{T}"/> already contains
    /// a set with the specified <paramref name="data"/>,
    /// otherwise <see langword="true"/>.
    /// </returns>
    public bool MakeSet(T data)
    {
        if (ContainsData(data)) return false;

        Node node = new Node(data);
        nodes.Add(data, node);

        return true;
    }

    /// <summary>
    /// Makes a union of two sets containing <paramref name="firstData"/> and <paramref name="secondData"/>,
    /// respectively.
    /// </summary>
    /// <param name="firstData">The data of the first set.</param>
    /// <param name="secondData">The data of the second set.</param>
    /// <returns>
    /// <see langword="true"/> if the two sets are joined in a union,
    /// <see langword="false"/> if the two sets already belong to the same set.
    /// </returns>
    public bool Union(T firstData, T secondData)
    {
        // Find the set representatives for firstData and secondData
        Node firstRoot = FindSet(nodes[firstData]);
        Node secondRoot = FindSet(nodes[secondData]);

        // Part of the same set so nothing needs to be done
        if(firstRoot == secondRoot) return false;

        // The set with the highest rank becomes the parent of the other set
        if (firstRoot.Rank >= secondRoot.Rank)
        {
            // The union of two sets with the same rank has a rank one higher
            if (firstRoot.Rank == secondRoot.Rank) firstRoot.Rank++;
            secondRoot.Parent = firstRoot;
        }
        else
        {
            firstRoot.Parent = secondRoot;
        }

        return true;
    }

    /// <summary>
    /// Finds the data belonging to the set representative of the set that the specified
    /// <paramref name="data"/> belongs to.
    /// </summary>
    /// <param name="data">The data belonging to the set to find the set representative for.</param>
    /// <returns>
    /// The data belonging to the set representative of the set that contains <paramref name="data"/>.
    /// </returns>
    public T FindSet(T data) => FindSet(nodes[data]).Data;

    /// <summary>
    /// Finds the set representative of the supplied set.
    /// </summary>
    /// <param name="node">The set to find the set representative of.</param>
    /// <returns>The set representative of the supplied set.</returns>
    private Node FindSet(Node node)
    {
        Node parent = node.Parent;

        // If the parent is itself, this node is the root node
        if (parent == node) return parent;

        // Update parent node recursively for path compression
        node.Parent = FindSet(node.Parent);

        return node.Parent;
    }
}
