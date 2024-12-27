using System.Collections.Generic;
using UnityEngine;

public class IterativeRandomizedDFS
{
    private readonly Vector2Int initial;
    private readonly int width;
    private readonly int height;
    private readonly Dictionary<Vector2Int, bool> visited = new Dictionary<Vector2Int, bool>();
    private readonly Stack<Vector2Int> toSearch = new Stack<Vector2Int>();
    private readonly List<(Vector2Int, Vector2Int)> maze = new List<(Vector2Int, Vector2Int)>();

    public IterativeRandomizedDFS(Vector2Int initial, int width, int height)
    {
        this.initial = initial;
        this.width = width;
        this.height = height;
    }

    public void Generate()
    {
        // Choose the initial cell, mark it as visited and push it to the stack
        visited.Add(initial, true);
        toSearch.Push(initial);

        // While the stack is not empty
        while (toSearch.Count > 0)
        {
            // Pop a cell from the stack and make it a current cell
            Vector2Int current = toSearch.Pop();

            // If the current cell has any neighbours which have not been visited
            List<Vector2Int> unvisitedNeighbours = GetUnvisitedNeighbours(current);
            if (unvisitedNeighbours.Count > 0)
            {
                // Push the current cell to the stack
                toSearch.Push(current);

                // Choose one of the unvisited neighbours
                int random = RNG.Range(0, unvisitedNeighbours.Count);
                Vector2Int chosen = unvisitedNeighbours[random];

                // Remove the wall between the current cell and the chosen cell
                maze.Add((current, chosen));

                // Mark the chosen cell as visited and push it to the stack
                visited.Add(chosen, true);
                toSearch.Push(chosen);
            }
        }
    }

    private List<Vector2Int> GetUnvisitedNeighbours(Vector2Int current)
    {
        List<Vector2Int> unvisitedNeighbours = new List<Vector2Int>(4);
        if (current.y + 1 < height && !visited.ContainsKey(current + Vector2Int.up))
        {
            unvisitedNeighbours.Add(current + Vector2Int.up);
        }

        if (current.x + 1 < width && !visited.ContainsKey(current + Vector2Int.right))
        {
            unvisitedNeighbours.Add(current + Vector2Int.right);
        }

        if (current.y - 1 >= 0 && !visited.ContainsKey(current + Vector2Int.down))
        {
            unvisitedNeighbours.Add(current + Vector2Int.down);
        }

        if (current.x - 1 >= 0 && !visited.ContainsKey(current + Vector2Int.left))
        {
            unvisitedNeighbours.Add(current + Vector2Int.left);
        }

        return unvisitedNeighbours;
    }

    public List<(Vector2Int, Vector2Int)> GetMaze()
    {
        return maze;
    }
}
