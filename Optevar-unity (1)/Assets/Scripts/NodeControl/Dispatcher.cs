using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Dispatcher : MonoBehaviour
{
    /*
    Dispatcher class for multithreading environment.
    Most UnityEngine object does not works on non-main thread.
    Methods can be invoked in main thread via this class.
    */
    private static Dispatcher singleTon;
    private static List<Action> tasks = new List<Action>();

    void Start()
    {
        singleTon = this;
    }

    public static void Invoke(Action action)
    {
        if (action == null) return;
        lock (tasks)
        {
            tasks.Add(action);
        }
    }

    private void Update()
    {
        if (tasks.Count > 0)
        {
            lock (tasks)
            {
                tasks[0]?.Invoke();
                tasks.RemoveAt(0);
            }
        }
    }
}
