using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class IterativeRandomizedPrim : MazeGenerator
{
    public IterativeRandomizedPrim(Vector2Int initial, int width, int height, float stepDuration, CancellationToken token)
        : base(initial, width, height, stepDuration, token) { }

    // A HashSet is not ideal for Frontier, but better than a List, as we make more calls to Contains than to Retrieve
    // The ideal would be a data structure with O(1) operations for both Contains and Retrieve
    private readonly HashSet<Vector2Int> frontier = new HashSet<Vector2Int>();
    private readonly HashSet<Vector2Int> inMaze = new HashSet<Vector2Int>();

    public override async Awaitable Generate()
    {
        frontier.EnsureCapacity((Width * Height) / 4); // Rough estimate, probably not ideal
        inMaze.EnsureCapacity(Width * Height);

        // Mark a random cell as being “in” the maze
        MarkAsIn(Initial);
        await InitialGenerationStep();

        // Iterate until the frontier set is empty
        while (frontier.Count > 0)
        {
            // Choose a frontier cell at random
            Vector2Int frontierCell = frontier.ElementAt(RNG.Range(0, frontier.Count)); // Retrieve in O(n), not ideal
            frontier.Remove(frontierCell);

            // Choose a random “in” neighbour of that frontier cell
            List<Vector2Int> neighbours = GetNeighbours(frontierCell, inMaze.Contains);
            Vector2Int neighbourCell = neighbours[RNG.Range(0, neighbours.Count)];

            // Remove the wall between the neighbour cell and the frontier cell
            Maze.RemoveWall(neighbourCell, frontierCell);

            // Mark the frontier cell as being “in” the maze
            MarkAsIn(frontierCell);

            await GenerationStep(neighbourCell, frontierCell);
        }
    }

    private void MarkAsIn(Vector2Int cell)
    {
        inMaze.Add(cell);
        List<Vector2Int> neighbours = GetNeighbours(cell, (Vector2Int position) =>  !inMaze.Contains(position));
        frontier.AddRange(neighbours); // Contains in O(1), ideal
    }
}
