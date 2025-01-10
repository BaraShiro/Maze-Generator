using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

/// <summary>
/// A maze solving algorithm.
/// </summary>
public abstract class MazeSolver
{
    /// <summary>
    /// Event args for a solve step event, containing a position for a tile, and whether to paint or unpaint it.
    /// </summary>
    public class SolveStepEventArgs : EventArgs
    {
        public Vector2Int Position;
        public bool Paint;
    }

    /// <summary>
    /// The solve step event.
    /// </summary>
    public event EventHandler<SolveStepEventArgs> SolveStepEvent;

    /// <summary>
    /// Raises the solve step event.
    /// </summary>
    /// <param name="e">The envent args for the handler.</param>
    protected virtual void OnSolveStepEvent(SolveStepEventArgs e)
    {
        SolveStepEvent?.Invoke(this, e);
    }

    /// <summary>
    /// The envent args used in solve steps.
    /// </summary>
    private SolveStepEventArgs EventArgs { get; } = new SolveStepEventArgs();

    /// <summary>
    /// The maze to solve
    /// </summary>
    protected Maze Maze { get; }

    /// <summary>
    /// The starting position.
    /// </summary>
    protected Vector2Int Start { get; }

    /// <summary>
    /// The goal position.
    /// </summary>
    protected Vector2Int Goal { get; }

    /// <summary>
    /// Duration of one solve step, i.e. how long the solver waits in between solve steps, in seconds.
    /// If set to 0, no waiting is done.
    /// </summary>
    private float StepDuration { get; }

    /// <summary>
    /// A cancellation token to signal a cancellation of the solving operation.
    /// </summary>
    private CancellationToken CancellationToken { get; }

    /// <summary>
    /// Constructs a new instance of a maze solver.
    /// </summary>
    /// <param name="maze">The maze to solve.</param>
    /// <param name="start">The starting position.</param>
    /// <param name="goal">The goal position.</param>
    /// <param name="stepDuration">The duration to wait between generation steps, in seconds.</param>
    /// <param name="token">A cancellation token for cancelling the solving operation.</param>
    protected MazeSolver(Maze maze, Vector2Int start, Vector2Int goal, float stepDuration, CancellationToken token)
    {
        Maze = maze;
        Start = start;
        Goal = goal;
        StepDuration = stepDuration;
        CancellationToken = token;
    }

    /// <summary>
    /// Solves the maze.
    /// </summary>
    public abstract Awaitable Solve();

    /// <summary>
    /// A generation step representing a step at a positions in the maze.
    /// </summary>
    /// <param name="position">The position of the maze tile the step was made at.</param>
    /// <param name="paint">Whether to paint or unpaint the maze tile.</param>
    protected async Awaitable SolveStep(Vector2Int position, bool paint)
    {
        EventArgs.Position = position;
        EventArgs.Paint = paint;

        OnSolveStepEvent(EventArgs);

        if(StepDuration <= 0) return;
        await Awaitable.WaitForSecondsAsync(StepDuration, CancellationToken);
    }

    /// <summary>
    /// Gets all connected neighbouring positions of <paramref name="position"/>.
    /// </summary>
    /// <param name="position">The position to get connected neighbours from.</param>
    /// <returns>A list of all connected neighbours of <paramref name="position"/>.</returns>
    protected List<Vector2Int> GetNeighbours(Vector2Int position)
    {
        List<Vector2Int> edges = new List<Vector2Int>(4);

        Maze.MazeTile tile = Maze.Tile(position);

        if (tile.Up) edges.Add(position + Vector2Int.up);
        if (tile.Right) edges.Add(position + Vector2Int.right);
        if (tile.Down) edges.Add(position + Vector2Int.down);
        if (tile.Left) edges.Add(position + Vector2Int.left);

        return edges;
    }
}
