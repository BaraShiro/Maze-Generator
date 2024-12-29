using UnityEngine;

public class Maze
{
    public struct MazeTile
    {
        public bool Up;
        public bool Right;
        public bool Down;
        public bool Left;
    }

    private readonly MazeTile[,] tileMatrix;

    public int Width { get; private set; }
    public int Height { get; private set; }

    public ref MazeTile Tile(int x, int y)
    {
        return ref tileMatrix[x, y];
    }

    public ref MazeTile Tile(Vector2Int position)
    {
        return ref tileMatrix[position.x, position.y];
    }

    public Maze(int width, int height)
    {
        Width = width;
        Height = height;
        tileMatrix = new MazeTile[width, height];
    }

    public void RemoveWall(Vector2Int first, Vector2Int second)
    {
        if (first.x > second.x) // To the left
        {
            Tile(first).Left = true;
            Tile(second).Right = true;
        }
        else if (first.x < second.x) // To the right
        {
            Tile(first).Right = true;
            Tile(second).Left = true;
        }
        else // Vertical
        {
            if (first.y > second.y) // Below
            {
                Tile(first).Down = true;
                Tile(second).Up = true;
            }
            else // Above
            {
                Tile(first).Up = true;
                Tile(second).Down = true;
            }
        }
    }
}
