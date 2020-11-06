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
        Instance = this;
        if(Instance != null)
        {
            Debug.LogError("Multiple NavGrids detected. Will use newest.");
        }
        InitializeGrid();
    }

    private void InitializeGrid ()
    {

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
}
