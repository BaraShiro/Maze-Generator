using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public abstract class MazeSolver
{
    public class SolveStepEventArgs : EventArgs
    {
        public Vector2Int Position;
        public bool Paint;
    }

    public event EventHandler<SolveStepEventArgs> SolveStepEvent;

    private void OnSolveStepEvent(SolveStepEventArgs e)
    {
        SolveStepEvent?.Invoke(this, e);
    }

    protected SolveStepEventArgs EventArgs { get; set; } = new SolveStepEventArgs();

    protected Maze Maze { get; }
    protected Vector2Int Start { get; }
    protected Vector2Int Goal { get; }
    protected float StepDuration { get; }
    protected CancellationToken CancellationToken { get; }

    protected MazeSolver(Maze maze, Vector2Int start, Vector2Int goal, float stepDuration, CancellationToken token)
    {
        Maze = maze;
        Start = start;
        Goal = goal;
        StepDuration = stepDuration;
        CancellationToken = token;
    }

    protected async Awaitable GenerationStep(Vector2Int position, bool paint)
    {
        EventArgs.Position = position;
        EventArgs.Paint = paint;

        OnSolveStepEvent(EventArgs);

        if(StepDuration <= 0) return;
        await Awaitable.WaitForSecondsAsync(StepDuration, CancellationToken);
    }

    protected List<Vector2Int> GetNeighbours(Vector2Int position, Maze.MazeTile tile)
    {
        List<Vector2Int> edges = new List<Vector2Int>(4);

        if (tile.Up) edges.Add(position + Vector2Int.up);
        if (tile.Right) edges.Add(position + Vector2Int.right);
        if (tile.Down) edges.Add(position + Vector2Int.down);
        if (tile.Left) edges.Add(position + Vector2Int.left);

        return edges;
    }
}
