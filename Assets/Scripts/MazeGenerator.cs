using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public abstract class MazeGenerator
{
    public class GenerationStepEventArgs : EventArgs
    {
        public List<(Vector2Int position, Maze.MazeTile tile)> Changes;
    }

    public event EventHandler<GenerationStepEventArgs> GenerationStepEvent;

    protected void OnGenerationStepEvent(GenerationStepEventArgs e)
    {
        GenerationStepEvent?.Invoke(this, e);
    }


    protected Vector2Int Initial { get; }
    protected int Width { get; }
    protected int Height { get; }
    protected float StepDuration { get; }
    protected CancellationToken CancellationToken { get; }
    protected Maze Maze { get; private set; }

    protected GenerationStepEventArgs EventArgs { get; set; }

    protected MazeGenerator(Vector2Int initial, int width, int height, float stepDuration, CancellationToken token)
    {
        Initial = initial;
        Width = width;
        Height = height;
        StepDuration = stepDuration;
        CancellationToken = token;
        Maze = new Maze(width, height);
        EventArgs = new GenerationStepEventArgs();
    }

    public abstract Awaitable Generate();

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

    protected List<Vector2Int> GetNeighbours(Vector2Int position) => GetNeighbours(position, (_) => true);

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

    protected async Awaitable InitialGenerationStep()
    {
        EventArgs.Changes =  new List<(Vector2Int position, Maze.MazeTile tile)>(1)
        {
            (Initial, Maze.Tile(Initial))

        };
        OnGenerationStepEvent(EventArgs);
        await Awaitable.WaitForSecondsAsync(StepDuration, CancellationToken);
    }

    protected async Awaitable GenerationStep(Vector2Int position)
    {
        EventArgs.Changes = new List<(Vector2Int position, Maze.MazeTile tile)>(1)
        {
            (position, Maze.Tile(position)),
        };
        OnGenerationStepEvent(EventArgs);

        await Awaitable.WaitForSecondsAsync(StepDuration, CancellationToken);
    }

    protected async Awaitable GenerationStep(Vector2Int firstPosition, Vector2Int secondPosition)
    {
        EventArgs.Changes = new List<(Vector2Int position, Maze.MazeTile tile)>(2)
        {
            (firstPosition, Maze.Tile(firstPosition)),
            (secondPosition, Maze.Tile(secondPosition))
        };
        OnGenerationStepEvent(EventArgs);

        await Awaitable.WaitForSecondsAsync(StepDuration, CancellationToken);
    }

    protected async Awaitable GenerationStep(List<(Vector2Int position, Maze.MazeTile tile)> positions)
    {
        EventArgs.Changes = positions;
        OnGenerationStepEvent(EventArgs);

        await Awaitable.WaitForSecondsAsync(StepDuration, CancellationToken);
    }
}