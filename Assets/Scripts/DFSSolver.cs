using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class DFSSolver : MazeSolver
{
    private readonly HashSet<Vector2Int> discovered = new HashSet<Vector2Int>();

    public DFSSolver(Maze maze, Vector2Int start, Vector2Int goal, float stepDuration, CancellationToken token)
        : base(maze, start, goal, stepDuration, token) { }

    public async Awaitable Solve()
    {
        await SolveAuxiliary(Start);
    }

    private async Awaitable<bool> SolveAuxiliary(Vector2Int position)
    {
        // Mark position as discovered
        discovered.Add(position);

        // Paint the tile when searching forward
        await GenerationStep(position, true);

        // If the position equals the goal, we are done and don't need to search more
        if (position == Goal) return true;

        // Search all neighbours
        foreach (Vector2Int edge in GetNeighbours(position, Maze.Tile(position)))
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
        await GenerationStep(position, false);

        // We didn't find the goal in this branch
        return false;
    }
}
