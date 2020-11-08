using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavGrid : MonoBehaviour
{
    public NavGrid Instance { get; private set; }

    [SerializeField]
    private float minX = -100;
    [SerializeField]
    private float maxX = 100;

    [SerializeField]
    private float minY = -100;
    [SerializeField] 
    private float maxY = 100;

    [SerializeField]
    private float spacing = 1;


    bool[,] walkableNodes;

    private void Awake ()
    {
        if(Instance != null)
        {
            Debug.LogError("Multiple NavGrids detected. Will use newest.");
        }
        Instance = this;

        InitializeGrid();
    }

    private void InitializeGrid ()
    {
        int width = Mathf.RoundToInt((maxX - minX) / spacing);
        int height = Mathf.RoundToInt((maxY - minY) / spacing);

        walkableNodes = new bool[width, height];

        Vector2 boxSize = new Vector2(spacing * 1.1f, spacing * 1.1f);
        int mask = LayerMask.GetMask("Solid");

        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                Vector2 pos = GetWorldPos(i, j);

                bool hit = Physics2D.OverlapBox(pos, boxSize, 0, mask);

                walkableNodes[i,j] = !hit;
            }
        }

    }

    private Vector2 GetWorldPos (int xIndex, int yIndex)
    {
        return new Vector2(xIndex * spacing + minX, yIndex * spacing + minY);
    }

    private Vector2Int GetIndices (Vector2 worldPos)
    {
        int x = Mathf.RoundToInt((worldPos.x - minX) / spacing);
        int y = Mathf.RoundToInt((worldPos.y - minY) / spacing);

        return new Vector2Int(x, y);
    }

    private void OnDestroy ()
    {
        Instance = null;
    }

    private void OnDrawGizmosSelected ()
    {
        if(walkableNodes != null)
        {
            for(int i = 0; i < walkableNodes.GetLength(0); i++)
            {
                for(int j = 0; j < walkableNodes.GetLength(1); j++)
                {
                    Vector2 pos = GetWorldPos(i, j);

                    Gizmos.color = walkableNodes[i,j] ? Color.white : Color.red;
                    Gizmos.DrawSphere(pos, 0.2f);
                }
            }

        }
    }
}
