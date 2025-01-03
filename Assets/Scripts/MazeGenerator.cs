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

    public event EventHandler<GenerationStepEventArgs> GenerationStep;

    protected void OnGenerationStep(GenerationStepEventArgs e)
    {
        GenerationStep?.Invoke(this, e);
    }


    protected Vector2Int Initial { get; }
    protected int Width { get; }
    protected int Height { get; }
    protected float StepDuration { get; }
    protected CancellationToken CancellationToken { get; }
    protected Maze Maze { get; private set; }

    protected MazeGenerator(Vector2Int initial, int width, int height, float stepDuration, CancellationToken token)
    {
        Initial = initial;
        Width = width;
        Height = height;
        StepDuration = stepDuration;
        CancellationToken = token;
        Maze = new Maze(width, height);
    }

    public abstract Awaitable Generate();
}