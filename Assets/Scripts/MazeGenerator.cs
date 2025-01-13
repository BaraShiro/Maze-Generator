using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

/// <summary>
/// A maze generating algorithm.
/// </summary>
public abstract class MazeGenerator
{
    /// <summary>
    /// Event args for a generation step event, containing a list of changes to the maze.
    /// </summary>
    public class GenerationStepEventArgs : EventArgs
    {
        public List<(Vector2Int position, Maze.MazeTile tile, bool mark)> Changes;
        public bool Reset = false;
    }

    /// <summary>
    /// The generation step event.
    /// </summary>
    public event EventHandler<GenerationStepEventArgs> GenerationStepEvent;

    /// <summary>
    /// Raises the generation step event.
    /// </summary>
    /// <param name="e">The event args for the handler.</param>
    protected virtual void OnGenerationStepEvent(GenerationStepEventArgs e)
    {
        GenerationStepEvent?.Invoke(this, e);
    }

    /// <summary>
    /// The envent args used in generation steps.
    /// </summary>
    private GenerationStepEventArgs EventArgs { get; } = new GenerationStepEventArgs();

    /// <summary>
    /// The initial position of the maze generation.
    /// </summary>
    protected Vector2Int Initial { get; }

    /// <summary>
    /// The width of the maze to be generated.
    /// </summary>
    protected int Width { get; }

    /// <summary>
    /// The height of the maze to be generated.
    /// </summary>
    protected int Height { get; }

    /// <summary>
    /// Duration of one generation step, i.e. how long the generator waits in between generating steps, in seconds.
    /// If set to 0, no waiting is done.
    /// </summary>
    private float StepDuration { get; }

    /// <summary>
    /// A cancellation token to signal a cancellation of the generation operation.
    /// </summary>
    private CancellationToken CancellationToken { get; }

    /// <summary>
    /// The generated maze.
    /// </summary>
    protected Maze Maze { get; }

    /// <summary>
    /// Constructs a new instance of a maze generator.
    /// </summary>
    /// <param name="initial">The initial position of the generation.</param>
    /// <param name="width">The width of the maze.</param>
    /// <param name="height">The height of the maze.</param>
    /// <param name="stepDuration">The duration to wait between generation steps, in seconds.</param>
    /// <param name="token">A cancellation token for cancelling the generation operation.</param>
    protected MazeGenerator(Vector2Int initial, int width, int height, float stepDuration, CancellationToken token)
    {
        Initial = initial;
        Width = width;
        Height = height;
        StepDuration = stepDuration;
        CancellationToken = token;
        Maze = new Maze(width, height);
    }

    /// <summary>
    /// Generates a maze.
    /// </summary>
    /// <returns>The generated maze.</returns>
    public abstract Awaitable<Maze> Generate();

    /// <summary>
    /// Performs <paramref name="action"/> on all positions in the maze.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    protected void ForAllPositions(Action<Vector2Int> action)
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                action(new Vector2Int(x, y));
            }
        }
    }

    /// <summary>
    /// Gets all neighbouring positions of <paramref name="position"/>.
    /// </summary>
    /// <param name="position">The position to get neighbours from.</param>
    /// <returns>A list of all neighbours of <paramref name="position"/>.</returns>
    protected List<Vector2Int> GetNeighbours(Vector2Int position) => GetNeighbours(position, (_) => true);

    /// <summary>
    /// Gets all neighbouring positions of <paramref name="position"/> that fulfills <paramref name="predicate"/>.
    /// </summary>
    /// <param name="position">The position to get neighbours from.</param>
    /// <param name="predicate">Whether a neighbour should be included or not.</param>
    /// <returns>A list of all neighbours of <paramref name="position"/> that fulfills <paramref name="predicate"/>.</returns>
    protected List<Vector2Int> GetNeighbours(Vector2Int position, Func<Vector2Int, bool> predicate)
    {
        List<Vector2Int> neighbours = new List<Vector2Int>(4);

        Vector2Int up = position + Vector2Int.up;
        if(up.y < Height && predicate(up)) neighbours.Add(up);

        Vector2Int right = position + Vector2Int.right;
        if(right.x < Width && predicate(right)) neighbours.Add(right);

        Vector2Int down = position + Vector2Int.down;
        if(down.y >= 0 && predicate(down)) neighbours.Add(down);

        Vector2Int left = position + Vector2Int.left;
        if(left.x >= 0 && predicate(left)) neighbours.Add(left);

        return neighbours;
    }

    /// <summary>
    /// A generation step representing the initial change in an empty maze.
    /// </summary>
    protected async Awaitable InitialGenerationStep(bool mark = false) => await GenerationStep(Initial, true);

    /// <summary>
    /// A generation step representing a change in a single position in the maze.
    /// </summary>
    /// <param name="position">The position that has changed.</param>
    /// <param name="mark">Whether to paint the tile at <paramref name="position"/> as marked.</param>
    /// <param name="noWait">Whether to skip the waiting time and not animate the change.</param>
    protected async Awaitable GenerationStep(Vector2Int position, bool mark = false, bool noWait = false)
    {
        EventArgs.Changes = new List<(Vector2Int position, Maze.MazeTile tile, bool mark)>(1)
        {
            (position, Maze.Tile(position), mark),
        };
        await GenerationStep(noWait);
    }

    /// <summary>
    /// A generation step representing a change in two positions in the maze.
    /// </summary>
    /// <param name="firstPosition">The first position that has changed.</param>
    /// <param name="secondPosition">The second position that has changed.</param>
    /// <param name="markFirst">Whether to paint the tile at <paramref name="firstPosition"/> as marked.</param>
    /// <param name="markSecond">Whether to paint the tile at <paramref name="secondPosition"/> as marked.</param>
    protected async Awaitable GenerationStep(Vector2Int firstPosition, Vector2Int secondPosition, bool markFirst = false, bool markSecond = false)
    {
        EventArgs.Changes = new List<(Vector2Int position, Maze.MazeTile tile, bool mark)>(2)
        {
            (firstPosition, Maze.Tile(firstPosition), markFirst),
            (secondPosition, Maze.Tile(secondPosition), markSecond)
        };
        await GenerationStep();
    }

    /// <summary>
    /// A generation step representing a change in several positions in the maze.
    /// </summary>
    /// <param name="positions">The positions that have changed.</param>
    /// <param name="reset">Whether to reset the graphics for the tiles in <paramref name="positions"/>
    /// to the default empty state</param>
    protected async Awaitable GenerationStep(List<(Vector2Int position, Maze.MazeTile tile, bool mark)> positions, bool reset = false)
    {
        EventArgs.Changes = positions;
        EventArgs.Reset = reset;
        await GenerationStep();
        EventArgs.Reset = false;
    }

    /// <summary>
    /// A generation step representing a change in the maze.
    /// </summary>
    private async Awaitable GenerationStep(bool noWait = false)
    {
        OnGenerationStepEvent(EventArgs);

        if (StepDuration <= 0 || noWait) return;
        await Awaitable.WaitForSecondsAsync(StepDuration, CancellationToken);
    }
}