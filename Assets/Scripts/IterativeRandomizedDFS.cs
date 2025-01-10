using System.Collections.Generic;
using System.Threading;
using UnityEngine;

/// <summary>
/// A recursive backtracker algorithm. Has a low branching factor, and contains many long corridors.
/// </summary>
/// <seealso href="https://en.wikipedia.org/wiki/Maze_generation_algorithm#Randomized_depth-first_search"/>
public class IterativeRandomizedDFS : MazeGenerator
{

    private readonly HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
    private readonly Stack<Vector2Int> toSearch = new Stack<Vector2Int>();

    public IterativeRandomizedDFS(Vector2Int initial, int width, int height, float stepDuration, CancellationToken token)
        : base(initial, width, height, stepDuration, token) { }

    public override async Awaitable<Maze> Generate()
    {
        // Choose the initial cell, mark it as visited and push it to the stack
        visited.Add(Initial);
        toSearch.Push(Initial);

        await InitialGenerationStep();

        // While the stack is not empty
        while (toSearch.Count > 0)
        {
            // Pop a cell from the stack and make it a current cell
            Vector2Int current = toSearch.Pop();

            // If the current cell has any neighbours which have not been visited
            List<Vector2Int> unvisitedNeighbours = GetNeighbours(current, (Vector2Int position) => !visited.Contains(position));
            if (unvisitedNeighbours.Count > 0)
            {
                // Push the current cell to the stack
                toSearch.Push(current);

                // Choose one of the unvisited neighbours
                int random = RNG.Range(0, unvisitedNeighbours.Count);
                Vector2Int chosen = unvisitedNeighbours[random];

                // Remove the wall between the current cell and the chosen cell
                Maze.RemoveWall(current, chosen);

                // Mark the chosen cell as visited and push it to the stack
                visited.Add(chosen);
                toSearch.Push(chosen);

                await GenerationStep(current, chosen);
            }
        }

        return Maze;
    }
}
