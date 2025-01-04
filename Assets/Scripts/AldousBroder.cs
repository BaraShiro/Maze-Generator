using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class AldousBroder : MazeGenerator
{
    private readonly HashSet<Vector2Int> unvisited = new HashSet<Vector2Int>();

    public AldousBroder(Vector2Int initial, int width, int height, float stepDuration, CancellationToken token)
        : base(initial, width, height, stepDuration, token) { }

    public override async Awaitable Generate()
    {
        unvisited.EnsureCapacity(Width * Height);

        ForAllPositions((Vector2Int position) =>
        {
            unvisited.Add(position);
        });

        // Pick a random cell as the current cell and mark it as visited
        Vector2Int current = Initial;
        unvisited.Remove(current);

        await InitialGenerationStep();

        // While there are unvisited cells
        while (unvisited.Count > 0)
        {
            // Pick a random neighbour
            List<Vector2Int> neighbours = GetNeighbours(current);
            Vector2Int neighbour = neighbours[RNG.Range(0, neighbours.Count)];

            // If the chosen neighbour has not been visited:
            if (unvisited.Contains(neighbour))
            {
                // Remove the wall between the current cell and the chosen neighbour.
                Maze.RemoveWall(current, neighbour);
                // Mark the chosen neighbour as visited.
                unvisited.Remove(neighbour);

                await GenerationStep(current, neighbour);
            }
            // Make the chosen neighbour the current cell.
            current = neighbour;
        }

    }
}
