using System.Collections.Generic;
using UnityEngine;

public class MazePainter : MonoBehaviour
{
    [SerializeField] private GameObject upRightCornerPrefab;
    [SerializeField] private GameObject upLeftCornerPrefab;
    [SerializeField] private GameObject downLeftCornerPrefab;
    [SerializeField] private GameObject downRightCornerPrefab;
    [SerializeField] private GameObject upTCrossingPrefab;
    [SerializeField] private GameObject rightTCrossingPrefab;
    [SerializeField] private GameObject downTCrossingPrefab;
    [SerializeField] private GameObject leftTCrossingPrefab;
    [SerializeField] private GameObject crossingPrefab;
    [SerializeField] private GameObject verticalPrefab;
    [SerializeField] private GameObject horizontalPrefab;



    public void PaintMaze(List<(Vector2Int, Vector2Int)> maze)
    {
        foreach ((Vector2Int a, Vector2Int b) in maze)
        {
            GameObject prefab;
            if (a.x < b.x) // Moving right
            {
                prefab = horizontalPrefab;
            }
            else if (a.x > b.x) // Moving left
            {
                prefab = horizontalPrefab;
            }
            else // Moving vertically
            {
                if (a.y < b.y) // Moving up
                {
                    prefab = verticalPrefab;
                }
                else // Moving down
                {
                    prefab = verticalPrefab;
                }
            }

            Instantiate(prefab, new Vector3(a.x, a.y), Quaternion.identity, transform);
        }
    }
}
