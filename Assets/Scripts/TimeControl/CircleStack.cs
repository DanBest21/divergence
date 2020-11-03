using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class CircleStack<T>
{
    private T[] buffer;

    private int defaultCapacity;

    private int top = -1;
    private int size = 0;
    public int Size { get { return size; } }

    public CircleStack (int defaultCapacity)
    {
        this.defaultCapacity = defaultCapacity;
        buffer = new T[defaultCapacity];
    }

    /// <summary>
    /// Get the newest element in the buffer
    /// </summary>
    /// <returns>Returns zero struct/null if buffer is empty</returns>
    public T GetLast()
    {
        if(size == 0)
        {
            throw new IndexOutOfRangeException("Buffer is empty");
        }
        return buffer[top];
    }

    public T GetRecent (int index)
    {
        int target = top - index;
        if(index >= size)
        {
            throw new IndexOutOfRangeException("Fewer items in buffer than request needs");
        }
        if (target < 0)
        {
            target += buffer.Length;
        }
        return buffer[target];
    }


    public void RemoveLast ()
    {
        if(size == 0)
        {
            throw new IndexOutOfRangeException("Buffer is empty");
        }
        top--;
        size--;
        if(top < 0)
        {
           top += buffer.Length;
        }

    }

    public void Add (T value)
    {
        top = (top + 1) % buffer.Length;

        buffer[top] = value;
        if(size < buffer.Length)
        {
            size++;
        }
    }

    public bool IsFull ()
    {
        return size == buffer.Length;
    }

    public void Resize ()
    {
        T[] newBuffer = new T[buffer.Length * 2];

        for(int i = 0; i < size; i++)
        {
            newBuffer[newBuffer.Length - size] = GetRecent(i);
        }

        buffer = newBuffer;
        top = newBuffer.Length - size;
    }

    public bool GetNextOverwrite (out T overwrite)
    {
        overwrite = buffer[(top + 1) % buffer.Length];
        return size == buffer.Length;
    }
}

