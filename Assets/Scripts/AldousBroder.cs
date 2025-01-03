using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class AldousBroder : MazeGenerator
{
    private readonly Dictionary<Vector2Int, bool> unvisited = new Dictionary<Vector2Int, bool>();

    public AldousBroder(Vector2Int initial, int width, int height, float stepDuration, CancellationToken token)
        : base(initial, width, height, stepDuration, token) { }

    public override async Awaitable Generate()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                unvisited.Add(new Vector2Int(x, y), true);
            }
        }

        // Pick a random cell as the current cell and mark it as visited
        Vector2Int current = Initial;
        unvisited.Remove(current);

        GenerationStepEventArgs e = new GenerationStepEventArgs
        {
            Changes = new List<(Vector2Int position, Maze.MazeTile tile)>(1) { (Initial, Maze.Tile(current)) }
        };
        OnGenerationStep(e);
        await Awaitable.WaitForSecondsAsync(StepDuration, CancellationToken);

        // While there are unvisited cells
        while (unvisited.Count > 0)
        {
            // Pick a random neighbour
            List<Vector2Int> neighbours = GetNeighbours(current);
            Vector2Int neighbour = neighbours[RNG.Range(0, neighbours.Count)];

            // If the chosen neighbour has not been visited:
            if (unvisited.ContainsKey(neighbour))
            {
                // Remove the wall between the current cell and the chosen neighbour.
                Maze.RemoveWall(current, neighbour);
                // Mark the chosen neighbour as visited.
                unvisited.Remove(neighbour);

                e.Changes = new List<(Vector2Int position, Maze.MazeTile tile)>(2)
                {
                    (current, Maze.Tile(current)),
                    (neighbour, Maze.Tile(neighbour))
                };
                OnGenerationStep(e);
                await Awaitable.WaitForSecondsAsync(StepDuration, CancellationToken);
            }
            // Make the chosen neighbour the current cell.
            current = neighbour;
        }

    }

    private List<Vector2Int> GetNeighbours(Vector2Int cell)
    {
        List<Vector2Int> neighbours = new List<Vector2Int>(4);

        AddToNeighbours(cell + Vector2Int.up);
        AddToNeighbours(cell + Vector2Int.right);
        AddToNeighbours(cell + Vector2Int.down);
        AddToNeighbours(cell + Vector2Int.left);

        return neighbours;

        void AddToNeighbours(Vector2Int neighbourCell)
        {
            if (neighbourCell is { x: >= 0, y: >= 0 } && neighbourCell.x < Width && neighbourCell.y < Height)
            {
                neighbours.Add(neighbourCell);
            }
        }
    }
}
