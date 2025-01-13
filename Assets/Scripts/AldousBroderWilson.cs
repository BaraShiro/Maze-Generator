using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

/// <summary>
/// An attempt to combine Aldous Broder and Wilson to leverage their respective strengths to combat their weaknesses.
/// Inspired by a comment on a post on
/// <see href="https://weblog.jamisbuck.org/2011/1/20/maze-generation-wilson-s-algorithm">The Buckblog</see>.
/// </summary>
public class AldousBroderWilson : MazeGenerator
{
    private readonly HashSet<Vector2Int> unvisited = new HashSet<Vector2Int>();

    private readonly Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
    private readonly List<Vector2Int> walk = new List<Vector2Int>();

    public AldousBroderWilson(Vector2Int initial, int width, int height, float stepDuration, CancellationToken token)
        : base(initial, width, height, stepDuration, token) { }

    public override async Awaitable<Maze> Generate()
    {
        int total = Width * Height;
        int threshold = total - (total / 3); // Supposedly a good cutoff point
        await GenerateAldousBroder(threshold);
        await GenerateWilson();

        return Maze;
    }

    /// <summary>
    /// Runs Aldous Broder until the number of cells in <see cref="unvisited"/> drops below <paramref name="threshold"/>.
    /// </summary>
    /// <param name="threshold">The cutoff threshold before switching over to Wilson</param>
    private async Awaitable GenerateAldousBroder(int threshold)
    {
        unvisited.EnsureCapacity(Width * Height);

        // Mark all cells as unvisited
        ForAllPositions((Vector2Int position) => unvisited.Add(position));

        // Pick a random cell as the current cell and mark it as visited
        Vector2Int current = Initial;
        unvisited.Remove(current);

        await InitialGenerationStep();

        // While there are unvisited cells
        while (unvisited.Count > threshold)
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
            }

            // Paint the new tile and unpaint behind us
            await GenerationStep(current, neighbour, false, true);

            // Make the chosen neighbour the current cell.
            current = neighbour;
        }

        // Unpaint the last cell, with no delay
        await GenerationStep(current, false, true);
    }

    /// <summary>
    /// Runs Wilson on a maze pre-baked by Aldous Broder.
    /// </summary>
    private async Awaitable GenerateWilson()
    {
        // While there are unvisited positions, add a loop-erased random walk to the maze
        while (unvisited.Count > 0)
        {
            // Pick a position not yet in the maze
            Vector2Int position = unvisited.ElementAt(RNG.Range(0, unvisited.Count));

            // Perform a random walk starting at this location
            await RandomWalk(position);

            // Add the random walk to the maze
            for (int i = 0; i < walk.Count - 1; i++)
            {
                unvisited.Remove(walk[i]);
                Maze.RemoveWall(walk[i], walk[i + 1]);
                // await GenerationStep(walk[i], walk[i + 1], true, true);
            }
            unvisited.Remove(walk[^1]);

            // Visualise the walk
            await GenerationStep(new List<(Vector2Int, Maze.MazeTile, bool)>(
                from cell in walk
                select (cell, Maze.Tile(cell), false)
            ));
        }
    }

    /// <summary>
    /// Go on a random walk through the unexplored parts of the maze, starting at <paramref name="position"/>,
    /// until an explored part is discovered, disregarding any loops on the way.
    /// </summary>
    /// /// <remarks>
    /// Is not guaranteed to ever finish, as the RNG values could potentially lead it in a series of never-ending loops.
    /// </remarks>
    /// <param name="position">The starting position of the walk.</param>
    private async Awaitable RandomWalk(Vector2Int position)
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

                List<(Vector2Int position, Maze.MazeTile tile, bool mark)> loop = new (walk.Count - loopIndex);

                for (int i = walk.Count - 1; i >= loopIndex; i--)
                {
                    loop.Add((walk[i], Maze.Tile(walk[i]), false));
                    cameFrom.Remove(walk[i]);
                    walk.RemoveAt(i);
                }

                // Reset the tiles of the loop to empty
                await GenerationStep(loop, reset: true);
            }
            else
            {
                // Otherwise, add it to the walk and keep track of where we came from
                walk.Add(neighbour);
                cameFrom.Add(neighbour, position);
                await GenerationStep(position, neighbour, true, true);
            }

            // Repeat with the neighbouring position
            position = neighbour;
        }

        // If this position is part of the maze, we’re done walking
        return;
    }
}
