using System.Collections.Generic;
using UnityEngine;

public class IterativeRandomizedKruskal: MazeGenerator
{
    private readonly List<(Vector2Int, Vector2Int)> walls = new List<(Vector2Int, Vector2Int)>();
    private readonly DisjointSet<Vector2Int> disjointSet = new DisjointSet<Vector2Int>();

    public IterativeRandomizedKruskal(Vector2Int initial, int width, int height) : base(initial, width, height) { }

    public override async Awaitable Generate()
    {
        // Create a list of all walls, and create a set for each cell, each containing just one cell
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (y < Height - 1)
                {
                    walls.Add((new Vector2Int(x, y), new Vector2Int(x, y + 1)));
                }

                if (x < Width - 1)
                {
                    walls.Add((new Vector2Int(x, y), new Vector2Int(x + 1, y)));
                }

                disjointSet.MakeSet(new Vector2Int(x, y));
            }
        }
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

                GenerationStepEventArgs e = new GenerationStepEventArgs
                {
                    Changes = new List<(Vector2Int position, Maze.MazeTile tile)>(2)
                    {
                        (first, Maze.Tile(first)),
                        (second, Maze.Tile(second))
                    }
                };
                OnGenerationStep(e);

                await Awaitable.WaitForSecondsAsync(0.2f, Application.exitCancellationToken);
            }
        }
    }
}
