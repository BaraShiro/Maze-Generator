using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Disjoint Sets using union by rank and path compression
/// </summary>
/// <typeparam name="T">The type of the data contained in the set</typeparam>
public class DisjointSet<T> : IEnumerable<T> where T : IEquatable<T>
{
    private class Node
    {
        public T Data { get; }
        public Node Parent { get; set; }
        public int Rank { get; set; }

        public Node(T data)
        {
            Data = data;
            Parent = this;
            Rank = 0;
        }
    }

    private readonly Dictionary<T, Node> nodes = new Dictionary<T, Node>();

    public int Count => nodes.Count;

    public IEnumerator<T> GetEnumerator()
    {
        return nodes.Keys.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return nodes.Keys.GetEnumerator();
    }

    public bool ContainsData(T data)
    {
        return nodes.ContainsKey(data);
    }

    public bool IsEmpty()
    {
        return Count == 0;
    }

    public void Clear()
    {
        nodes.Clear();
    }

    public bool MakeSet(T data)
    {
        if (ContainsData(data)) return false;

        Node node = new Node(data);
        nodes.Add(data, node);

        return true;
    }

    public bool Union(T firstData, T secondData)
    {
        // Find the set representatives for data1 and data2
        Node firstRoot = FindSet(nodes[firstData]);
        Node secondRoot = FindSet(nodes[secondData]);

        // Part of the same set so nothing needs to be done
        // if(firstRoot.Data.Equals(secondRoot.Data)) return;
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

    public T FindSet(T data) => FindSet(nodes[data]).Data;

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
