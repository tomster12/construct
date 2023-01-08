
//xusing System;

//struct Item
//{
//    int element;
//    int priority;
//}

//public interface IPriorityQueue<T>
//{
//    /// Inserts and item with a priority
//    void Insert(T item, int priority);

//    /// Returns the element with the highest priority
//    T Top();

//    /// Deletes and returns the element with the highest priority
//    T Pop();
//}

//public class PriorityQueue<TElement, TPriority> : IPriorityQueue<TElement, TPriority>
//    where TPriority : IComparable<TPriority>
//{
//    private readonly FibonacciHeap<TElement, TPriority> heap;

//    public PriorityQueue(TPriority minPriority)
//    {
//        heap = new FibonacciHeap<TElement, TPriority>(minPriority);
//    }

//    public void Insert(TElement item, TPriority priority)
//    {
//        heap.Insert(new FibonacciHeapNode<TElement, TPriority>(item, priority));
//    }

//    public TElement Top()
//    {
//        return heap.Min().Data;
//    }

//    public TElement Pop()
//    {
//        return heap.RemoveMin().Data;
//    }
//}

//public class PVar<T>
//{
//    struct Item { int p; T v; }


//    // Need to
//    // - sort by priority
//    // - access by name
//    // - get value of highest
//    SortedDictionary<string, Item> dict = new SortedDictionary<string, Item>();

//    public void add(string name, T v, int p)
//    {

//    }

//    public T get()
//    {
//        return default;
//    }

//    public T v
//    {
//        get { return get(); }
//        set { }
//    }
//}