using UnityEngine;

public class MazePainter : MonoBehaviour
{
    [SerializeField] private MazeTileVisual mazeTilePrefab;

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
    [SerializeField] private Sprite horizontalSprite;

    private MazeTileVisual[,] mazeVisualMatrix;

    public async void GenerateMaze()
    {
        int width = 10;
        int height = 10;

        mazeVisualMatrix = new MazeTileVisual[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                mazeVisualMatrix[x,y] = Instantiate(mazeTilePrefab, new Vector3(x, y), Quaternion.identity, transform);
            }
        }

        RNG.InitState(42);
        IterativeRandomizedDFS iterativeRandomizedDFS = new IterativeRandomizedDFS(Vector2Int.zero, 10, 10);
        iterativeRandomizedDFS.GenerationStep += MazeGenerator_GenerationStep;
        await iterativeRandomizedDFS.Generate();
    }

    private void MazeGenerator_GenerationStep(object sender, MazeGenerator.GenerationStepEventArgs e)
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

            {Up: true, Right: false, Down: true, Left: false} => verticalSprite,
            {Up: false, Right: true, Down: false, Left: true} => horizontalSprite,

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
}
