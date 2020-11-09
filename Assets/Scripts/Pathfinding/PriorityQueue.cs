using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Min-Heap implementation of a priority queue
/// </summary>
/// <typeparam name="T"></typeparam>
public class PriorityQueue<T>
{
    Tuple<T,float>[] items;
    public int Count { get; private set; }

    public PriorityQueue(int defaultCapacity)
    {
        items = new Tuple<T, float>[defaultCapacity];
    }

    public void Enqueue (T item, float priority)
    {
        if(Count >= items.Length)
        {
            Array.Resize(ref items, items.Length * 2);
        }
        items[Count] = new Tuple<T, float>(item, priority);

        //Sort heap
        int currentIndex = Count;

        while(currentIndex > 0)
        {
            int parentIndex = GetParent(currentIndex);

            if(items[parentIndex].Item2 > items[currentIndex].Item2)
            {
                Swap(currentIndex, parentIndex);
            }
            currentIndex = parentIndex;
        }

        Count++;
    }


    public T Dequeue ()
    {
        T item = items[0].Item1;

        //Sort heap
        items[0] = items[Count - 1];

        Count--;

        int index = 0;

        while(true)
        {
            int lowest = index;
            int left = GetLeftChild(index);
            int right = GetRightChild(index);

            if(left < Count && items[left].Item2 < items[lowest].Item2)
            {
                lowest = left;
            }
            if(right < Count && items[right].Item2 < items[lowest].Item2)
            {
                lowest = right;
            }
            if(lowest != index)
            {
                Swap(lowest, index);
                index = lowest;
            }
            else
            {
                break;
            }
        }

        return item;
    }


    int GetParent (int index)
    {
        return (index - 1) / 2;
    }

    int GetLeftChild (int index)
    {
        return index * 2 + 1;
    }

    int GetRightChild (int index)
    {
        return index * 2 + 2;
    }

    private void Swap (int a, int b)
    {
        var temp = items[a];
        items[a] = items[b];
        items[b] = temp;
    }
}
