
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class PriorityQueue
{
    private readonly List<object> _data;
    /// <summary>
    /// The default priority generator.
    /// </summary>
    //internal static readonly Func<object, int> DefaultPriorityCalculator = message => 1;
    private Func<object, object, int> _priorityCalculator;
    /// <summary>
    /// DEPRECATED. Should always specify priority calculator instead.
    /// </summary>
    /// <param name="initialCapacity">The current capacity of the priority queue.</param>
    public PriorityQueue(int initialCapacity)
    {
        _data = new List<object>(initialCapacity);
    }

    /// <summary>
    /// Creates a new priority queue.
    /// </summary>
    /// <param name="initialCapacity">The initial capacity of the queue.</param>
    /// <param name="priorityCalculator">The calculator function for assigning message priorities.</param>
    public PriorityQueue(int initialCapacity, Func<object, object, int> priorityCalculator)
    {
        _data = new List<object>(initialCapacity);
        _priorityCalculator = priorityCalculator;
    }

    /// <summary>
    /// DEPRECATED. Sets a new priority calculator.
    /// </summary>
    /// <param name="priorityCalculator">The calculator function for assigning message priorities.</param>
    /// <remarks>
    /// WARNING: SHOULD NOT BE USED. Use the constructor to set priority instead.
    /// </remarks>
    [Obsolete("Use the constructor to set the priority calculator instead. [1.1.3]")]
    public void SetPriorityCalculator(Func<object, object, int> priorityCalculator)
    {
        _priorityCalculator = priorityCalculator;
    }

    public void Clear()
    {
        this._data.Clear();
    }

    public bool Contains(object _node)
    {
        return _data.Contains(_node);
    }
    /// <summary>
    /// Enqueues a message into the priority queue.
    /// </summary>
    /// <param name="item">The item to enqueue.</param>
    public void Enqueue(object item)
    {

        _data.Add(item);
        var ci = _data.Count - 1; // child index; start at end
        while (ci > 0)
        {
            var pi = (ci - 1) / 2; // parent index
                                   //if (_priorityCalculator(_data[ci]).CompareTo(_priorityCalculator(_data[pi])) >= 0) break; // child item is larger than (or equal) parent so we're done
            if (_priorityCalculator(_data[ci], _data[pi]) >= 0) break;
            var tmp = _data[ci]; _data[ci] = _data[pi]; _data[pi] = tmp;
            ci = pi;
        }
    }

    /// <summary>
    /// Dequeues the highest priority message at the front of the priority queue.
    /// </summary>
    /// <returns>The highest priority message <see cref="Envelope"/>.</returns>
    public object Dequeue()
    {
        // assumes pq is not empty; up to calling code
        var li = _data.Count - 1; // last index (before removal)
        var frontItem = _data[0];   // fetch the front
        _data[0] = _data[li];
        _data.RemoveAt(li);

        --li; // last index (after removal)
        var pi = 0; // parent index. start at front of pq
        while (true)
        {
            var ci = pi * 2 + 1; // left child index of parent
            if (ci > li) break;  // no children so done
            var rc = ci + 1;     // right child
            if (rc <= li && _priorityCalculator(_data[rc], _data[ci]) < 0) // if there is a rc (ci + 1), and it is smaller than left child, use the rc instead
                ci = rc;
            if (_priorityCalculator(_data[pi], _data[ci]) <= 0) break; // parent is smaller than (or equal to) smallest child so done
            var tmp = _data[pi]; _data[pi] = _data[ci]; _data[ci] = tmp; // swap parent and child
            pi = ci;
        }
        return frontItem;
    }
    public object GetIndex(int _idx)
    {
        return this._data[_idx];
    }
    /// <summary>
    /// Peek at the message at the front of the priority queue.
    /// </summary>
    /// <returns>The highest priority message <see cref="Envelope"/>.</returns>
    public object Peek()
    {
        var frontItem = _data[0];
        return frontItem;
    }

    /// <summary>
    /// Counts the number of items in the priority queue.
    /// </summary>
    /// <returns>The total number of items in the queue.</returns>
    public int Count()
    {
        return _data.Count;
    }

    /// <summary>
    /// Converts the queue to a string representation.
    /// </summary>
    /// <returns>A string representation of the queue.</returns>
    public override string ToString()
    {
        var s = "";
        for (var i = 0; i < _data.Count; ++i)
            s += _data[i].ToString() + " ";
        s += "count = " + _data.Count;
        return s;
    }

    /// <summary>
    /// TBD
    /// </summary>
    /// <returns>TBD</returns>
    public bool IsConsistent()
    {
        // is the heap property true for all data?
        if (_data.Count == 0) return true;
        var li = _data.Count - 1; // last index
        for (var pi = 0; pi < _data.Count; ++pi) // each parent index
        {
            var lci = 2 * pi + 1; // left child index
            var rci = 2 * pi + 2; // right child index

            if (lci <= li && _priorityCalculator(_data[pi], _data[lci]) > 0) return false; // if lc exists and it's greater than parent then bad.
            if (rci <= li && _priorityCalculator(_data[pi], _data[rci]) > 0) return false; // check the right child too.
        }
        return true; // passed all checks
    } // IsConsistent
} // ListPriorityQueue
