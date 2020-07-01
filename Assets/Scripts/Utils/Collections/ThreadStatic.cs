using System;
using UnityEngine;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// 这里提供一个类来实现 ThreadStatic.
/// 并且直接实现懒加载.
/// 用的时候把变量设为 static.
/// </summary>
public class ThreadStatic<T>
{
    readonly Dictionary<int, T> data = new Dictionary<int, T>();
    
    readonly Func<T> generator;
    
    public ThreadStatic(Func<T> gen)
    {
        generator = gen;
    }
    
    public T inst
    {
        get
        {
            var id = Thread.CurrentThread.ManagedThreadId;
            if(!data.ContainsKey(id))
            {
                data[id] = generator();
            }
            return data[id];
        }
    }
}
