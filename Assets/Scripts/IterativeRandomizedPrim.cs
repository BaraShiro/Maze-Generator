using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class IterativeRandomizedPrim : MazeGenerator
{
    public IterativeRandomizedPrim(Vector2Int initial, int width, int height, float stepDuration, CancellationToken token)
        : base(initial, width, height, stepDuration, token) { }

    // A hash set is not ideal, but better than a list, as we make more calls to contains than to retrieve
    // The ideal would be a data structure with O(1) operations for both contains and retrieve
    private readonly HashSet<Vector2Int> frontier = new HashSet<Vector2Int>();
    private readonly Dictionary<Vector2Int, bool> inMaze = new Dictionary<Vector2Int, bool>();

    public override async Awaitable Generate()
    {
        // Mark a random cell as being “in” the maze
        MarkAsIn(Initial);
        GenerationStepEventArgs e = new GenerationStepEventArgs
        {
            Changes = new List<(Vector2Int position, Maze.MazeTile tile)>(1) { (Initial, Maze.Tile(Initial)) }
        };
        OnGenerationStep(e);
        await Awaitable.WaitForSecondsAsync(StepDuration, CancellationToken);

        // Iterate until the frontier set is empty
        while (frontier.Count > 0)
        {
            // Choose a frontier cell at random
            Vector2Int frontierCell = frontier.ElementAt(RNG.Range(0, frontier.Count)); // Retrieve in O(n), not ideal
            frontier.Remove(frontierCell);

            // Choose a random “in” neighbour of that frontier cell
            List<Vector2Int> neighbours = GetInNeighbours(frontierCell);
            Vector2Int neighbourCell = neighbours[RNG.Range(0, neighbours.Count)];

            // Remove the wall between the neighbour cell and the frontier cell
            Maze.RemoveWall(neighbourCell, frontierCell);

            // Mark the frontier cell as being “in” the maze
            MarkAsIn(frontierCell);

            e.Changes = new List<(Vector2Int position, Maze.MazeTile tile)>(2)
            {
                (neighbourCell, Maze.Tile(neighbourCell)),
                (frontierCell, Maze.Tile(frontierCell))
            };
            OnGenerationStep(e);
            await Awaitable.WaitForSecondsAsync(StepDuration, CancellationToken);
        }
    }

    private void AddToFrontier(Vector2Int cell)
    {
        if (cell is { x: >= 0, y: >= 0 } && cell.x < Width && cell.y < Height && !inMaze.ContainsKey(cell))
        {
            frontier.Add(cell); // Contains in O(1), ideal
        }
    }

    private void MarkAsIn(Vector2Int cell)
    {
        inMaze.Add(cell, true);
        AddToFrontier(cell + Vector2Int.up);
        AddToFrontier(cell + Vector2Int.right);
        AddToFrontier(cell + Vector2Int.down);
        AddToFrontier(cell + Vector2Int.left);
    }

    private List<Vector2Int> GetInNeighbours(Vector2Int frontierCell)
    {
        List<Vector2Int> neighbours = new List<Vector2Int>(4);

        AddToNeighbours(frontierCell + Vector2Int.up);
        AddToNeighbours(frontierCell + Vector2Int.right);
        AddToNeighbours(frontierCell + Vector2Int.down);
        AddToNeighbours(frontierCell + Vector2Int.left);

        return neighbours;

        void AddToNeighbours(Vector2Int cell)
        {
            if (cell is { x: >= 0, y: >= 0 } && cell.x < Width && cell.y < Height && inMaze.ContainsKey(cell))
            {
                neighbours.Add(cell);
            }
        }
    }
}
