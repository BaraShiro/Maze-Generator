using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Slider = UnityEngine.UI.Slider;

/// <summary>
/// Paints a visual representation of a maze on the game canvas.
/// </summary>
public class MazePainter : SingletonMonoBehaviour<MazePainter>
{
    [Header("Prefabs")]
    [SerializeField] private MazeTileVisual mazeTilePrefab;
    [SerializeField] private MazeTileVisual startTilePrefab;
    [SerializeField] private MazeTileVisual goalTilePrefab;
    [SerializeField] private MazeTileVisual houseTilePrefab;
    [Space(10)]
    [Header("Colours")]
    [SerializeField] private Color unpaintedColor;
    [SerializeField] private Color paintedColor;
    [SerializeField] private Color markedColor;
    [Space(10)]
    [Header("Graphics")]
    [SerializeField] private Sprite emptySprite;
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
    [Header("UI references")]
    [SerializeField] private SeedInput seedInput;
    [SerializeField] private Slider widthSlider;
    [SerializeField] private Slider heightSlider;
    [SerializeField] private Slider durationSlider;
    [SerializeField] private TMP_Dropdown algoDropdown;
    [SerializeField] private Button generateButton;
    [SerializeField] private TMP_Text generateButtonText;
    [SerializeField] private Button solveButton;
    [SerializeField] private TMP_Text solveButtonText;
    [SerializeField] private RectTransform paintCanvasTransform;


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

    public Color UnpaintedColor => unpaintedColor;
    public Color PaintedColor => paintedColor;
    public Color MarkedColor => markedColor;
    public Sprite EmptySprite => emptySprite;

    /// <summary>
    /// Sets up the generator by reading values from the UI, runs the generator, and decorates the generated maze.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <see cref="AlgoDropdown.Algos"/> value from the
    /// algos dropdown does not represent a valid maze generation algorithm.</exception>
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

        // Set camera size to include the entire maze
        if (Camera.main) Camera.main.orthographicSize = Mathf.Ceil(Mathf.Max(width, height) / 2f);

        // Move painter so the maze is in the center
        if (Camera.main)
        {
            Vector3 offset = new Vector3(-(width / 2f) + 0.5f, -(height / 2f) + 0.5f);
            Vector3 painterPosition = Camera.main.ScreenToWorldPoint(paintCanvasTransform.position);
            painterPosition.z = 0;
            painterPosition += offset;
            transform.position = painterPosition;
        }

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
        mazeGenerator.GenerationStepEvent += mazeGenerator_MazeGenerationStepEvent;

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

    /// <summary>
    /// Sets up the solver by reading values from the UI, and then runs the solver.
    /// </summary>
    public async void SolveMaze()
    {
        // If the generator is running, don't do anything
        if (generating) return;

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
            mazeTileVisual.Unpaint();
        }

        // Setup solver data
        float stepDuration = durationSlider.value;
        Vector2Int start = Vector2Int.zero;
        Vector2Int goal = new Vector2Int(solvedMaze.Width - 1, solvedMaze.Height - 1);

        // Setup solver
        MazeSolver mazeSolver = new DFSSolver(solvedMaze, start, goal, stepDuration, linkedCts.Token);
        mazeSolver.SolveStepEvent += mazeSolver_MazeSolveStepEvent;

        // Run solver
        try
        {
            await mazeSolver.Solve();
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

    /// <summary>
    /// The maze generation step event handler.
    /// Changes the maze visual according to the list of changes contained in <paramref name="e"/>.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event args containing a list of changes to the maze.</param>
    private void mazeGenerator_MazeGenerationStepEvent(object sender, MazeGenerator.GenerationStepEventArgs e)
    {
        foreach ((Vector2Int position, Maze.MazeTile tile) in e.Changes)
        {
            mazeVisualMatrix[position.x, position.y].ChangeSprite(GetSpriteFromMazeTile(tile));
        }
    }

    /// <summary>
    /// The maze solve step event handler.
    /// Changes the maze visual by painting or unpainting a maze tile based on information contained in <paramref name="e"/>.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event args containing the position of the tile,
    /// and whether it should be painted or unpainted.</param>
    private void mazeSolver_MazeSolveStepEvent(object sender, MazeSolver.SolveStepEventArgs e)
    {
        if (e.Paint)
        {
            mazeVisualMatrix[e.Position.x, e.Position.y].Paint();
        }
        else
        {
            mazeVisualMatrix[e.Position.x, e.Position.y].Unpaint();
        }
    }

    /// <summary>
    /// Transforms a tile value to a sprite representing that tile.
    /// </summary>
    /// <param name="tile">The tile to be transformed.</param>
    /// <returns>A sprite representing the tile.</returns>
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

    /// <summary>
    /// Places a start, a goal, and several house decorations on top of the maze.
    /// </summary>
    /// <param name="stepDuration">The duration to wait between placing decoration, in seconds.</param>
    /// <param name="token">A cancellation token to cancel the operation.</param>
    private async Awaitable DecorateMaze(float stepDuration, CancellationToken token)
    {
        // Get all possible house positions, i.e. dead ends
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

        // Shuffle the positions
        housePositions.Shuffle();

        // Place just the right number of houses in the maze
        int numberOfHouses = Mathf.FloorToInt((solvedMaze.Width + solvedMaze.Height) * 0.4f);
        houseDecorations = new List<MazeTileVisual>(numberOfHouses);
        foreach (Vector3 position in housePositions.Take(numberOfHouses))
        {
            MazeTileVisual houseDecoration = Instantiate(houseTilePrefab, transform.position + position, Quaternion.identity, transform);
            houseDecorations.Add(houseDecoration);
            if (stepDuration > 0) await Awaitable.WaitForSecondsAsync(stepDuration, token);
        }

        // Add the start
        startDecoration = Instantiate(startTilePrefab, transform.position, Quaternion.identity, transform);
        if (stepDuration > 0) await Awaitable.WaitForSecondsAsync(stepDuration, token);

        // Add the goal
        Vector3 goalOffset = new Vector3(solvedMaze.Width - 1, solvedMaze.Height - 1);
        goalDecoration = Instantiate(goalTilePrefab, transform.position + goalOffset, Quaternion.identity, transform);
        if (stepDuration > 0) await Awaitable.WaitForSecondsAsync(stepDuration, token);
    }

    /// <summary>
    /// Disposes of all maze visuals, preparing for a new maze to be generated
    /// </summary>
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

    /// <summary>
    /// Sets the text of the generate-button based on whether the generator is running or not.
    /// </summary>
    private void SetGenerateButtonText()
    {
        generateButtonText.text = generating ? "Cancel" : "Generate" ;
    }

    /// <summary>
    /// Sets the text of the solve-button based on whether the solver is running or not.
    /// </summary>
    private void SetSolveButtonText()
    {
        solveButtonText.text = solving ? "Cancel" : "Solve" ;
    }

    /// <summary>
    /// Sets the interactable state of the generate-button based on whether the solver is running or not.
    /// </summary>
    private void SetGenerateButtonState()
    {
        generateButton.interactable = !solving;
    }

    /// <summary>
    /// Sets the interactable state of the solve-button based on whether the generator is running or not,
    /// and if a generated maze is available for solving.
    /// </summary>
    private void SetSolveButtonState()
    {
        solveButton.interactable = (!generating) && generated;
    }
}
