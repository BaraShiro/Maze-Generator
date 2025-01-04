using System;
using System.Threading;
using TMPro;
using UnityEngine;
using Slider = UnityEngine.UI.Slider;

public class MazePainter : MonoBehaviour
{
    [SerializeField] private MazeTileVisual mazeTilePrefab;
    [SerializeField] private Vector2 offset;
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
    [SerializeField] private TMP_Text generateButtonText;
    [SerializeField] private Slider widthSlider;
    [SerializeField] private Slider heightSlider;
    [SerializeField] private Slider durationSlider;
    [SerializeField] private TMP_Dropdown algoDropdown;

    private MazeTileVisual[,] mazeVisualMatrix = new MazeTileVisual[0,0];

    private bool running = false;

    private CancellationTokenSource internalTokenSource = new CancellationTokenSource();
    private CancellationToken internalToken;

    public async void GenerateMaze()
    {
        if (running)
        {
            internalTokenSource.Cancel();
            internalTokenSource = new CancellationTokenSource();
            return;
        }

        internalToken = internalTokenSource.Token;
        using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(internalToken, Application.exitCancellationToken);
        running = true;
        SetButtonText();

        RNG.InitState(seedInput.SeedHash);
        int width = Mathf.FloorToInt(widthSlider.value);
        int height = Mathf.FloorToInt(heightSlider.value);
        Vector2Int initial = new Vector2Int(RNG.Range(0, width), RNG.Range(0, height));
        float stepDuration = durationSlider.value;

        // Clean up old maze if it exists
        foreach (MazeTileVisual mazeTileVisual in mazeVisualMatrix)
        {
            Destroy(mazeTileVisual.gameObject);
        }

        // Move painter so maze is in center
        transform.position = (Vector3) offset + new Vector3(-(width / 2), -(height / 2));

        // Create new empty maze
        mazeVisualMatrix = new MazeTileVisual[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                mazeVisualMatrix[x,y] = Instantiate(mazeTilePrefab, transform.position + new Vector3(x, y), Quaternion.identity, transform);
            }
        }

        MazeGenerator mazeGenerator = ((AlgoDropdown.Algos)algoDropdown.value) switch
        {
            AlgoDropdown.Algos.DFS => new IterativeRandomizedDFS(initial, width, height, stepDuration, linkedCts.Token),
            AlgoDropdown.Algos.Kruskal => new IterativeRandomizedKruskal(initial, width, height, stepDuration, linkedCts.Token),
            AlgoDropdown.Algos.Prim => new IterativeRandomizedPrim(initial, width, height, stepDuration, linkedCts.Token),
            AlgoDropdown.Algos.AldousBroder => new AldousBroder(initial, width, height, stepDuration, linkedCts.Token),
            _ => throw new ArgumentOutOfRangeException()
        };

        mazeGenerator.GenerationStepEvent += MazeGenerationStepEvent;

        try
        {
            await mazeGenerator.Generate();
        }
        catch (OperationCanceledException)
        {
            if (Application.exitCancellationToken.IsCancellationRequested)
            {
                Debug.Log("Operation cancelled from application exit.");
            }
            else if (internalToken.IsCancellationRequested)
            {
                Debug.Log("Operation cancelled by user.");
                running = false;
                SetButtonText();
            }
        }

        running = false;
        SetButtonText();
    }

    private void MazeGenerationStepEvent(object sender, MazeGenerator.GenerationStepEventArgs e)
    {
        foreach ((Vector2Int position, Maze.MazeTile tile) in e.Changes)
        {
            mazeVisualMatrix[position.x, position.y].ChangeSprite(GetSpriteFromMazeTile(tile));
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

    private void SetButtonText()
    {
        generateButtonText.text = running ? "Cancel" : "Generate" ;
    }
}
