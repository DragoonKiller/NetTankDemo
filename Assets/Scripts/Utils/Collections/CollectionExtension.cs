using System;
using System.Collections;
using System.Collections.Generic;

using System.Runtime;
using System.Runtime.CompilerServices;

using UnityEngine;

namespace Utils
{
    public static partial class Collections
    {
        /// <summary>
        /// 获取迭代器的第一个元素.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T First<T>(this IEnumerable<T> x)
        {
            var i = x.GetEnumerator();
            if(!i.MoveNext()) throw new ArgumentNullException();
            return i.Current;
        }
        
        /// <summary>
        /// 获取一个元素; 如果没有, 就使用默认值创建它.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetOrDefault<R, T>(this SortedList<R, T> dict, R key, T defaultVal)
        {
            if(dict.TryGetValue(key, out T val)) return val;
            dict.Add(key, defaultVal);
            return defaultVal;
        }

        /// <summary>
        /// 获取一个元素; 如果没有, 就使用无参构造函数创建它.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetOrDefault<R, T>(this SortedList<R, T> dict, R key)
            where T : new()
        {
            if(dict.TryGetValue(key, out T val)) return val;
            var res = new T();
            dict.Add(key, res);
            return res;
        }

        /// <summary>
        /// 获取一个元素; 如果没有, 就使用默认值创建它.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetOrDefault<R, T>(this Dictionary<R, T> dict, R key, T defaultVal)
        {
            if(dict.TryGetValue(key, out T val)) return val;
            dict.Add(key, defaultVal);
            return defaultVal;
        }

        /// <summary>
        /// 获取一个元素; 如果没有, 就使用无参构造函数创建它.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetOrDefault<R, T>(this Dictionary<R, T> dict, R key)
            where T : new()
        {
            if(dict.TryGetValue(key, out T val)) return val;
            var res = new T();
            dict.Add(key, res);
            return res;
        }
        
        /// <summary>
        /// 向列表添加从from到to的整数.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddIntRange(this List<int> ls, int from, int to)
        {
            for(int i = from; i <= to; i++) ls.Add(i);
        }
        
        /// <summary>
        /// 向列表中添加元素.
        /// </summary>
        // 该函数不会覆盖单参数的 Add 函数.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<T> Add<T>(this List<T> x, T a, T b, params T[] c)
        {
            x.Add(a);
            x.Add(b);
            foreach(var i in c) x.Add(i);
            return x;
        }
        
        /// <summary>
        /// 找到第一个符合条件的元素的下标.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FindIndex<T>(this T[] s, Predicate<T> f)
        {
            for(int i = 0; i < s.Length; i++) if(f(s[i])) return i;
            return -1;
        }

        /// <summary>
        /// 把迭代器包装成容器.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> ToEnumerable<T>(this IEnumerator<T> v)
        {
            while(v.MoveNext())
            {
                yield return v.Current;
            }
        }

        /// <summary>
        /// 返回从数组的第begin个元素到数组的第end-1个元素的迭代器.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> ToEnumerable<T>(this T[] arr, int begin, int end)
        {
            for(int i = begin; i < end; i++) yield return arr[i];
        }
        
        /// <summary>
        /// 返回从数组的第0个元素到数组的第end-1个元素的迭代器.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> ToEnumerable<T>(this T[] arr, int end)
        {
            for(int i = 0; i < end; i++) yield return arr[i];
        }
        
        /// <summary>
        /// 取最后一个元素.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Last<T>(this List<T> lst)
        {
            return lst[lst.Count - 1];
        }
        
        /// <summary>
        /// 删除最后一个元素.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveLast<T>(this List<T> lst)
        {
            lst.RemoveAt(lst.Count - 1);
        }
        
        /// <summary>
        /// 比较 a 和 b, 找出相对于 a, 集合 b 添加了哪些元素, 删除了哪些元素.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (List<T> added, List<T> removed) FowardComapre<T>(this ICollection<T> a, ICollection<T> b)
        {
            var added = new List<T>();
            var removed = new List<T>();
            foreach(var i in a) if(!b.Contains(i)) removed.Add(i);
            foreach(var i in b) if(!a.Contains(i)) added.Add(i);
            return (added, removed);
        }

    }
}
