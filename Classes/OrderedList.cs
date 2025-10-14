using System.Collections;
using System.Collections.Generic;

namespace CloverAPI.Classes;

public class OrderedList<T, TComparer> : IEnumerable<T> where TComparer : IComparer<T>, new() 
{
    private List<T> items;
    private TComparer comparer;
    private bool isSorted;
    private bool losesOrderOnRemove;
    
    public OrderedList(bool losesOrderOnRemove = false)
    {
        items = new List<T>();
        comparer = new TComparer();
        isSorted = true;
        this.losesOrderOnRemove = losesOrderOnRemove;
    }
    
    public void Add(T item)
    {
        items.Add(item);
        isSorted = false;
    }
    
    public bool Remove(T item)
    {
        bool removed = items.Remove(item);
        if (removed && losesOrderOnRemove)
        {
            isSorted = false;
        }
        return removed;
    }
    
    public void Clear()
    {
        items.Clear();
        isSorted = true;
    }
    
    public int Count => items.Count;
    
    public T this[int index]
    {
        get
        {
            EnsureSorted();
            return items[index];
        }
        set
        {
            items[index] = value;
            isSorted = false;
        }
    }
    
    public List<T> ToList()
    {
        EnsureSorted();
        return new List<T>(items);
    }
    
    private void EnsureSorted()
    {
        if (!isSorted)
        {
            items.Sort(comparer);
            isSorted = true;
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        EnsureSorted();
        return items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}