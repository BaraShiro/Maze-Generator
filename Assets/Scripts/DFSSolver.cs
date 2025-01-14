using System.Collections.Generic;
using System.Threading;
using UnityEngine;

/// <summary>
/// A recursive depth-first search maze solver.
/// </summary>
public class DFSSolver : MazeSolver
{
    private readonly HashSet<Vector2Int> discovered = new HashSet<Vector2Int>();

    public DFSSolver(Maze maze, Vector2Int start, Vector2Int goal, float stepDuration, CancellationToken token)
        : base(maze, start, goal, stepDuration, token) { }

    public override async Awaitable Solve()
    {
        await SolveAuxiliary(Start);
    }

    /// <summary>
    /// Recursively searches the maze depth first for the goal, starting at <paramref name="position"/>.
    /// </summary>
    /// <param name="position">The starting position.</param>
    /// <returns>
    /// <see langword="true"/> if the goal was found in this branch of the maze,
    /// otherwise <see langword="false"/>.
    /// </returns>
    private async Awaitable<bool> SolveAuxiliary(Vector2Int position)
    {
        // Mark position as discovered
        discovered.Add(position);

        // Paint the tile when searching forward
        await SolveStep(position, true);

        // If the position equals the goal, we are done and don't need to search more
        if (position == Goal) return true;

        // Search all neighbours
        foreach (Vector2Int edge in GetNeighbours(position))
        {
            // If a neighbour is not discovered, recursively call solver
            if (!discovered.Contains(edge))
            {
                bool foundGoal = await SolveAuxiliary(edge);

                // If we found the goal in this branch, we don't need to search any more neighbours
                if (foundGoal) return true;
            }
        }

        // Unpaint the tile when backtracking
        await SolveStep(position, false);

        // We didn't find the goal in this branch
        return false;
    }
}
