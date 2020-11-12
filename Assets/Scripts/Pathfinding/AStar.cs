using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Grid-based A* Implementation
/// </summary>
class AStar
{
    bool[,] walkable;
    float[,] gScores;
    Vector2Int[,] cameFrom;
    bool[,] visited;
    PriorityQueue<Vector2Int> queue;

    Vector2Int start;
    Vector2Int dest;

    List<Vector2Int> path;

    private float minDistance;

    private static readonly Vector2Int[] offsets = new Vector2Int[]
        {
            new Vector2Int(-1, 0), //Right

            new Vector2Int(0,-1), //Down
            new Vector2Int(0, 1), //Up
            new Vector2Int(1, 0), //Left

            new Vector2Int(1,-1), //Diagonal
            new Vector2Int(1, 1), //Diagonal
            new Vector2Int(-1,-1), //Diagonal
            new Vector2Int(-1, 1), //Diagonal
        };


    public AStar (bool[,] walkable, Vector2Int start, Vector2Int dest)
    {
        this.walkable = walkable;
        int x = walkable.GetLength(0);
        int y = walkable.GetLength(1);

        this.start = start;
        this.dest = dest;

        gScores = new float[x, y];

        for(int i = 0; i < x; i++)
        {
            for(int j = 0; j < y; j++)
            {
                gScores[i, j] = Mathf.Infinity;
            }
        }

        cameFrom = new Vector2Int[x, y];
        visited = new bool[x, y];
        queue = new PriorityQueue<Vector2Int>(64);
        minDistance = Vector2.Distance(start, dest);
    }

    public List<Vector2Int> GetPath ()
    {
        if(path != null)
        {
            return path;
        }

        if(FindPath())
        {
            List<Vector2Int> path = new List<Vector2Int>() { dest };
            Vector2Int node = dest;
            while(node != start)
            {                
                node = cameFrom[node.x, node.y];
                path.Add(node);
            }

            path.Reverse();
            this.path = path;

            return path;
        }
        else
        {
            return null;
        }


    }

    private bool FindPath ()
    {
        queue.Enqueue(start, 0);
        gScores[start.x, start.y] = 0;
        Vector2Int node;


        while(queue.Count > 0)
        {
            node = queue.Dequeue();

            while(visited[node.x, node.y])
            {
                node = queue.Dequeue();
            }
            if(node == dest)
            {
                return true;
            }

            visited[node.x, node.y] = true;

            for(int i = 0; i < offsets.Length; i++)
            {
                var offset = offsets[i];

                Vector2Int neighbour = node + offset;
                if(InBounds(neighbour) && walkable[neighbour.x, neighbour.y] && !visited[neighbour.x, neighbour.y])
                {
                    float gScore = gScores[node.x, node.y] + offset.magnitude + offset.x * 0.1f + offset.y * 0.01f;

                    if(gScore < gScores[neighbour.x, neighbour.y])
                    {
                        gScores[neighbour.x, neighbour.y] = gScore;
                        cameFrom[neighbour.x, neighbour.y] = node;
                        float fScore = gScore + Distance(neighbour);
                        queue.Enqueue(neighbour, fScore);
                    }
                }
            }
        }

        return false;
    }

    private bool InBounds(Vector2Int node)
    {
        return node.x >= 0 && node.y >= 0 && node.x < walkable.GetLength(0) && node.y < walkable.GetLength(1);
    }

    private float Distance (Vector2Int node)
    {
        return Vector2.Distance(node, dest);
    }
}

