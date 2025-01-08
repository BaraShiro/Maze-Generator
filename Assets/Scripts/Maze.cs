using System;
using UnityEngine;

/// <summary>
/// A model of a maze in the form of an undirected graph.
/// </summary>
public class Maze
{
    /// <summary>
    /// Represents a maze tile in the form of a graph node with up to four edges.
    /// </summary>
    public struct MazeTile
    {
        public bool Up;
        public bool Right;
        public bool Down;
        public bool Left;
    }

    /// <summary>
    /// The matrix that makes up the maze, i.e. the graph.
    /// It's dimensions are dictated by <see cref="Width"/> and <see cref="Height"/>.
    /// </summary>
    private readonly MazeTile[,] tileMatrix;

    /// <summary>
    /// The width of the maze, corresponding to the number of columns in <see cref="tileMatrix"/>.
    /// </summary>
    public int Width { get; }

    /// <summary>
    ///  The height of the maze, corresponding to the number of rows in <see cref="tileMatrix"/>.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Gets a tile from the maze.
    /// </summary>
    /// <param name="x">The column of the tile.</param>
    /// <param name="y">The row of the tile.</param>
    /// <returns>A tile in the maze located at column <paramref name="x"/> and row <paramref name="y"/>.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown if <paramref name="x"/> is &lt; 0 or &ge; <see cref="Width"/>,
    /// or <paramref name="y"/> is &lt; 0 or &ge; <see cref="Height"/>.</exception>
    public MazeTile Tile(int x, int y) => tileMatrix[x, y];

    /// <summary>
    /// Gets a tile from the maze.
    /// </summary>
    /// <param name="position">The position in the maze matrix.</param>
    /// <returns>A tile in the maze located at <paramref name="position"/>.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown if <paramref name="position.x"/> is &lt; 0 or &ge; <see cref="Width"/>,
    /// or <paramref name="position.y"/> is &lt; 0 or &ge; <see cref="Height"/>.</exception>
    public MazeTile Tile(Vector2Int position) => tileMatrix[position.x, position.y];

    /// <summary>
    /// Constructs a new maze consisting of unconnected tiles,
    /// with dimensions <paramref name="width"/> times <paramref name="height"/>.
    /// </summary>
    /// <param name="width">The width of the maze.</param>
    /// <param name="height">The height of the maze.</param>
    public Maze(int width, int height)
    {
        Width = width;
        Height = height;
        tileMatrix = new MazeTile[width, height];
    }

    /// <summary>
    /// Removes a wall between two tiles in the maze.
    /// </summary>
    /// <param name="first">The first tile.</param>
    /// <param name="second">The second tile.</param>
    /// <remarks>Trying to remove a wall that has already been removed has no effect,
    /// but will also not generate an error.</remarks>
    public void RemoveWall(Vector2Int first, Vector2Int second)
    {
        if (first.x > second.x) // To the left
        {
            GetTile(first).Left = true;
            GetTile(second).Right = true;
        }
        else if (first.x < second.x) // To the right
        {
            GetTile(first).Right = true;
            GetTile(second).Left = true;
        }
        else // Vertical
        {
            if (first.y > second.y) // Below
            {
                GetTile(first).Down = true;
                GetTile(second).Up = true;
            }
            else // Above
            {
                GetTile(first).Up = true;
                GetTile(second).Down = true;
            }
        }

        return;


        ref MazeTile GetTile(Vector2Int position)
        {
            return ref tileMatrix[position.x, position.y];
        }
    }
}
