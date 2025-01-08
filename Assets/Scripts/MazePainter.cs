using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Slider = UnityEngine.UI.Slider;

public class MazePainter : MonoBehaviour
{
    [SerializeField] private MazeTileVisual mazeTilePrefab;
    [SerializeField] private MazeTileVisual startTilePrefab;
    [SerializeField] private MazeTileVisual goalTilePrefab;
    [SerializeField] private MazeTileVisual houseTilePrefab;
    // [SerializeField] private Vector2 offset;
    [Space(10)]
    [SerializeField] private Sprite notConnectedSprite;
    [SerializeField] private Sprite upDeadEndSprite;
    [SerializeField] private Sprite rightDeadEndSprite;
    [SerializeField] private Sprite downDeadEndSprite;
    [SerializeField] private Sprite leftDeadEndSprite;
    [SerializeField] private Sprite upLeftCornerSprite;
    [SerializeField] private Sprite upRightCornerSprite;
    [SerializeField] private Sprite downRightCornerSprite;
    [SerializeField] private Sprite downLeftCornerSprite;
    [SerializeField] private Sprite upTCrossingSprite;
    [SerializeField] private Sprite rightTCrossingSprite;
    [SerializeField] private Sprite downTCrossingSprite;
    [SerializeField] private Sprite leftTCrossingSprite;
    [SerializeField] private Sprite crossingSprite;
    [SerializeField] private Sprite verticalSprite;
    [SerializeField] private Sprite verticalSpriteAlt;
    [SerializeField] private Sprite horizontalSprite;
    [SerializeField] private Sprite horizontalSpriteAlt;
    [Space(10)]
    [SerializeField] private SeedInput seedInput;
    [SerializeField] private Slider widthSlider;
    [SerializeField] private Slider heightSlider;
    [SerializeField] private Slider durationSlider;
    [SerializeField] private TMP_Dropdown algoDropdown;
    [SerializeField] private Button generateButton;
    [SerializeField] private TMP_Text generateButtonText;
    [SerializeField] private Button solveButton;
    [SerializeField] private TMP_Text solveButtonText;


    private MazeTileVisual[,] mazeVisualMatrix = new MazeTileVisual[0,0];
    private MazeTileVisual startDecoration;
    private MazeTileVisual goalDecoration;
    private List<MazeTileVisual> houseDecorations = new List<MazeTileVisual>();
    private Maze solvedMaze;

    private bool generating = false;
    private bool generated = false;
    private bool solving = false;

    private CancellationTokenSource internalTokenSource = new CancellationTokenSource();
    private CancellationToken internalToken;

    public async void GenerateMaze()
    {
        // If the solver is running, don't do anything
        if (solving) return;

        // If the generator is running, cancel the generator
        if (generating)
        {
            internalTokenSource.Cancel();
            internalTokenSource = new CancellationTokenSource();
            return;
        }

        // Setup cancellation token
        internalToken = internalTokenSource.Token;
        using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(internalToken, Application.exitCancellationToken);

        // Set state
        generating = true;
        generated = false;
        SetGenerateButtonText();
        SetSolveButtonState();

        // Setup generator data
        RNG.InitState(seedInput.SeedHash);
        int width = Mathf.FloorToInt(widthSlider.value);
        int height = Mathf.FloorToInt(heightSlider.value);
        Vector2Int initial = new Vector2Int(RNG.Range(0, width), RNG.Range(0, height));
        float stepDuration = durationSlider.value;

        // Move painter so the maze is in the center
        Vector3 offset = new Vector3(0.5f, -(height / 4f));
        transform.position = new Vector3(-(width / 2f), -(height / 2f)) + offset;

        // Set camera size to include the entire maze
        if (Camera.main) Camera.main.orthographicSize = Mathf.Max(width, height);

        // Clean up old maze if it exists
        CleanUpMaze();

        // Create new empty maze
        mazeVisualMatrix = new MazeTileVisual[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                mazeVisualMatrix[x,y] = Instantiate(mazeTilePrefab, transform.position + new Vector3(x, y), Quaternion.identity, transform);
            }
        }

        // Setup generator
        MazeGenerator mazeGenerator = ((AlgoDropdown.Algos)algoDropdown.value) switch
        {
            AlgoDropdown.Algos.DFS => new IterativeRandomizedDFS(initial, width, height, stepDuration, linkedCts.Token),
            AlgoDropdown.Algos.Kruskal => new IterativeRandomizedKruskal(initial, width, height, stepDuration, linkedCts.Token),
            AlgoDropdown.Algos.Prim => new IterativeRandomizedPrim(initial, width, height, stepDuration, linkedCts.Token),
            AlgoDropdown.Algos.AldousBroder => new AldousBroder(initial, width, height, stepDuration, linkedCts.Token),
            AlgoDropdown.Algos.Wilson => new Wilson(initial, width, height, stepDuration, linkedCts.Token),
            _ => throw new ArgumentOutOfRangeException()
        };
        mazeGenerator.GenerationStepEvent += MazeGenerationStepEvent;

        // Run generator
        try
        {
            solvedMaze = await mazeGenerator.Generate();
            await DecorateMaze(stepDuration, linkedCts.Token);
        }
        catch (OperationCanceledException)
        {
            if (Application.exitCancellationToken.IsCancellationRequested)
            {
                Debug.Log("Generation operation cancelled from application exit.");
                return;
            }
            else if (internalToken.IsCancellationRequested)
            {
                Debug.Log("Generation operation cancelled by user.");

                // Set state
                generating = false;
                generated = false;
                SetGenerateButtonText();
                SetSolveButtonState();
                return;
            }
        }

        // Set state
        generating = false;
        generated = true;
        SetGenerateButtonText();
        SetSolveButtonState();
    }

    public async void SolveMaze()
    {
        // If the generator is running, don't do anything
        if(generating) return;

        // If the solver is running, cancel the solver
        if (solving)
        {
            internalTokenSource.Cancel();
            internalTokenSource = new CancellationTokenSource();
            return;
        }

        // Setup cancellation token
        internalToken = internalTokenSource.Token;
        using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(internalToken, Application.exitCancellationToken);

        // Set state
        solving = true;
        SetSolveButtonText();
        SetGenerateButtonState();

        // Unpaint maze
        foreach (MazeTileVisual mazeTileVisual in mazeVisualMatrix)
        {
            mazeTileVisual.SetColourUnmarked();
        }

        // Setup solver data
        float stepDuration = durationSlider.value;
        Vector2Int start = Vector2Int.zero;
        Vector2Int goal = new Vector2Int(solvedMaze.Width - 1, solvedMaze.Height - 1);

        // Setup solver
        DFSSolver solver = new DFSSolver(solvedMaze, start, goal, stepDuration, linkedCts.Token);
        solver.SolveStepEvent += MazeSolveStepEvent;

        // Run solver
        try
        {
            await solver.Solve();
        }
        catch (OperationCanceledException)
        {
            if (Application.exitCancellationToken.IsCancellationRequested)
            {
                Debug.Log("Solve operation cancelled from application exit.");
                return;
            }
            else if (internalToken.IsCancellationRequested)
            {
                Debug.Log("Solve operation cancelled by user.");

                // Set state
                solving = false;
                SetSolveButtonText();
                SetGenerateButtonState();
                return;
            }
        }

        // Set state
        solving = false;
        SetSolveButtonText();
        SetGenerateButtonState();
    }

    private void MazeGenerationStepEvent(object sender, MazeGenerator.GenerationStepEventArgs e)
    {
        foreach ((Vector2Int position, Maze.MazeTile tile) in e.Changes)
        {
            mazeVisualMatrix[position.x, position.y].ChangeSprite(GetSpriteFromMazeTile(tile));
        }
    }

    private void MazeSolveStepEvent(object sender, MazeSolver.SolveStepEventArgs e)
    {
        if (e.Paint)
        {
            mazeVisualMatrix[e.Position.x, e.Position.y].SetColourMarked();
        }
        else
        {
            mazeVisualMatrix[e.Position.x, e.Position.y].SetColourUnmarked();
        }
    }

    private Sprite GetSpriteFromMazeTile(Maze.MazeTile tile) =>
        tile switch
        {
            {Up: false, Right: false, Down: false, Left: false} => notConnectedSprite,

            {Up: true, Right: false, Down: false, Left: false} => downDeadEndSprite,
            {Up: false, Right: false, Down: true, Left: false} => upDeadEndSprite,
            {Up: false, Right: true, Down: false, Left: false} => leftDeadEndSprite,
            {Up: false, Right: false, Down: false, Left: true} => rightDeadEndSprite,

            {Up: true, Right: false, Down: true, Left: false} => RNG.Value > 0.5 ? verticalSprite : verticalSpriteAlt,
            {Up: false, Right: true, Down: false, Left: true} => RNG.Value > 0.5 ? horizontalSprite : horizontalSpriteAlt,

            {Up: true, Right: true, Down: false, Left: false} => upRightCornerSprite,
            {Up: true, Right: false, Down: false, Left: true} => upLeftCornerSprite,
            {Up: false, Right: true, Down: true, Left: false} => downRightCornerSprite,
            {Up: false, Right: false, Down: true, Left: true} => downLeftCornerSprite,

            {Up: true, Right: true, Down: false, Left: true} => upTCrossingSprite,
            {Up: true, Right: true, Down: true, Left: false} => rightTCrossingSprite,
            {Up: false, Right: true, Down: true, Left: true} => downTCrossingSprite,
            {Up: true, Right: false, Down: true, Left: true} => leftTCrossingSprite,

            {Up: true, Right: true, Down: true, Left: true} => crossingSprite,
        };

    private async Awaitable DecorateMaze(float stepDuration, CancellationToken token)
    {
        List<Vector3> housePositions = new List<Vector3>();
        for (int x = 0; x < solvedMaze.Width; x++)
        {
            for (int y = 0; y < solvedMaze.Height; y++)
            {
                // Don't include start and goal
                if ((x == 0 && y == 0) || (x == solvedMaze.Width - 1 && y == solvedMaze.Height - 1)) continue;

                Maze.MazeTile tile = solvedMaze.Tile(x, y);
                if (tile is
                    { Up: true, Right: false, Down: false, Left: false } or
                    { Up: false, Right: true, Down: false, Left: false } or
                    { Up: false, Right: false, Down: true, Left: false } or
                    { Up: false, Right: false, Down: false, Left: true }
                   )
                {
                    housePositions.Add(new Vector3(x, y));
                }
            }
        }
        housePositions.Shuffle();

        int numberOfHouses = Mathf.FloorToInt((solvedMaze.Width + solvedMaze.Height) * 0.4f);
        houseDecorations = new List<MazeTileVisual>(numberOfHouses);
        foreach (Vector3 position in housePositions.Take(numberOfHouses))
        {
            MazeTileVisual houseDecoration = Instantiate(houseTilePrefab, transform.position + position, Quaternion.identity, transform);
            houseDecorations.Add(houseDecoration);
            if (stepDuration > 0) await Awaitable.WaitForSecondsAsync(stepDuration, token);
        }

        startDecoration = Instantiate(startTilePrefab, transform.position, Quaternion.identity, transform);
        if (stepDuration > 0) await Awaitable.WaitForSecondsAsync(stepDuration, token);

        Vector3 goalOffset = new Vector3(solvedMaze.Width - 1, solvedMaze.Height - 1);
        goalDecoration = Instantiate(goalTilePrefab, transform.position + goalOffset, Quaternion.identity, transform);
        if (stepDuration > 0) await Awaitable.WaitForSecondsAsync(stepDuration, token);
    }

    private void CleanUpMaze()
    {
        foreach (MazeTileVisual mazeTileVisual in mazeVisualMatrix)
        {
            if(mazeTileVisual) Destroy(mazeTileVisual.gameObject);
        }

        foreach (MazeTileVisual houseDecoration in houseDecorations)
        {
            if(houseDecoration) Destroy(houseDecoration.gameObject);
        }

        if(startDecoration) Destroy(startDecoration.gameObject);
        if(goalDecoration) Destroy(goalDecoration.gameObject);
    }

    private void SetGenerateButtonText()
    {
        generateButtonText.text = generating ? "Cancel" : "Generate" ;
    }

    private void SetSolveButtonText()
    {
        solveButtonText.text = solving ? "Cancel" : "Solve" ;
    }

    private void SetGenerateButtonState()
    {
        generateButton.interactable = !solving;
    }

    private void SetSolveButtonState()
    {
        solveButton.interactable = (!generating) && generated;
    }
}
