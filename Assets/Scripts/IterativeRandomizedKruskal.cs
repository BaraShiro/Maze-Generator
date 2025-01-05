using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class IterativeRandomizedKruskal: MazeGenerator
{
    private readonly List<(Vector2Int, Vector2Int)> walls = new List<(Vector2Int, Vector2Int)>();
    private readonly DisjointSet<Vector2Int> disjointSet = new DisjointSet<Vector2Int>();

    public IterativeRandomizedKruskal(Vector2Int initial, int width, int height, float stepDuration, CancellationToken token)
        : base(initial, width, height, stepDuration, token) { }

    public override async Awaitable<Maze> Generate()
    {
        walls.Capacity = ((Width * Height) * 2) - (Width + Height);
        disjointSet.EnsureCapacity(Width * Height);

        // Create a list of all walls, and create a set for each cell, each containing just one cell
        ForAllPositions((Vector2Int position) =>
        {
            Vector2Int up = position + Vector2Int.up;
            if (up.y < Height) walls.Add((position, up));

            Vector2Int right = position + Vector2Int.right;
            if (right.x < Width) walls.Add((position, right));

            disjointSet.MakeSet(position);
        });

        // For each wall, in some random order
        walls.Shuffle();
        foreach ((Vector2Int first, Vector2Int second) in walls)
        {
            // If the cells divided by this wall belong to distinct sets
            if (disjointSet.FindSet(first) != disjointSet.FindSet(second))
            {
                // Remove the current wall
                Maze.RemoveWall(first, second);
                // Join the sets of the formerly divided cells
                disjointSet.Union(first, second);

                await GenerationStep(first, second);
            }
        }

        return Maze;
    }
}
