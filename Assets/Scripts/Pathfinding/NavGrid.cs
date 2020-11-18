using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NavGrid : MonoBehaviour
{
    public static NavGrid Instance { get; private set; }

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

    [SerializeField]
    private float collisionTestSize = 0.8f;


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

        Vector2 boxSize = new Vector2(spacing, spacing) * collisionTestSize;
        int mask = LayerMask.GetMask("Obstructions");

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

    private Vector2Int GetWalkableIndices (Vector2 worldPos)
    {
        Vector2Int closest = GetIndices(worldPos);

        if(walkableNodes[closest.x, closest.y])
        {
            return closest;
        }
        Vector2Int[] neighours = new Vector2Int[]
            {
                closest + new Vector2Int(-1,-1), //Diagonal
                closest + new Vector2Int(-1, 1), //Diagonal
                closest + new Vector2Int(-1, 0),
                closest + new Vector2Int(0,-1),
                closest + new Vector2Int(0, 1),
                closest + new Vector2Int(1,-1), //Diagonal
                closest + new Vector2Int(1, 1), //Diagonal
                closest + new Vector2Int(1, 0),
            };

        return neighours.Where(n => walkableNodes[n.x, n.y])
            .OrderBy(n => (worldPos - GetWorldPos(n.x, n.y)).sqrMagnitude)
            .First();
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

    public Vector2[] GetPath (Vector2 start, Vector2 target)
    {
        Vector2Int startIndex = GetWalkableIndices(start);
        Vector2Int destIndices = GetWalkableIndices(target);

        AStar pathFinder = new AStar(walkableNodes, startIndex, destIndices);
        List<Vector2Int> pathIndices = pathFinder.GetPath();

        if(pathIndices == null)
        {
            return null;
        }

        Vector2[] path = pathIndices.Select(i => GetWorldPos(i.x, i.y)).ToArray();
        path[0] = start;
        path[path.Length - 1] = target;

        return path;
    }
}
