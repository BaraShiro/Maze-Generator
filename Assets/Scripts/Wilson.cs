using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class Wilson : MazeGenerator
{
    private readonly HashSet<Vector2Int> unvisited = new HashSet<Vector2Int>();
    private readonly Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
    private readonly List<Vector2Int> walk = new List<Vector2Int>();

    public Wilson(Vector2Int initial, int width, int height, float stepDuration, CancellationToken token)
        : base(initial, width, height, stepDuration, token) { }

    public override async Awaitable<Maze> Generate()
    {
        unvisited.EnsureCapacity(Width * Height);
        ForAllPositions((Vector2Int position) => unvisited.Add(position));

        // Add the starting position to the maze
        unvisited.Remove(Initial);

        // While there are unvisited positions, add a loop-erased random walk to the maze
        while (unvisited.Count > 0)
        {
            // Pick a position not yet in the maze
            Vector2Int position = unvisited.ElementAt(RNG.Range(0, unvisited.Count));

            // Perform a random walk starting at this location
            RandomWalk(position);

            // Add the random walk to the maze
            for (int i = 0; i < walk.Count - 1; i++)
            {
                unvisited.Remove(walk[i]);
                Maze.RemoveWall(walk[i], walk[i + 1]);
                await GenerationStep(walk[i], walk[i + 1]);
            }
            unvisited.Remove(walk[^1]);
        }

        return Maze;
    }

    private void RandomWalk(Vector2Int position)
    {
        // Initialise walk and cameFrom
        walk.Clear();
        cameFrom.Clear();
        walk.Add(position);
        cameFrom.Add(position, position);

        // Until we find a position in the maze
        while (unvisited.Contains(position))
        {
            // pick a random direction that's not the where we came from
            Vector2Int origin = position; // Suppress "Access to modified captured variable" code inspection
            List<Vector2Int> neighbours = GetNeighbours(position, (Vector2Int neighbour) => neighbour != cameFrom[origin]);
            Vector2Int neighbour = neighbours[RNG.Range(0, neighbours.Count)];

            // If this neighbouring position was visited previously during this walk
            if (walk.Contains(neighbour))
            {
                // Erase the loop, rewinding the path to its earlier state
                int loopIndex = walk.IndexOf(neighbour) + 1;
                for (int i = walk.Count - 1; i >= loopIndex; i--)
                {
                    cameFrom.Remove(walk[i]);
                    walk.RemoveAt(i);
                }
            }
            else
            {
                // Otherwise, add it to the walk and keep track of where we came from
                walk.Add(neighbour);
                cameFrom.Add(neighbour, position);
            }

            // Repeat with the neighbouring position
            position = neighbour;
        }

        // If this position is part of the maze, we’re done walking
        return;
    }
}
