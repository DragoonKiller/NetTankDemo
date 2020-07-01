using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Systems
{
    using Utils;
    
    /// <summary>
    /// 全局广播信号系统.
    /// </summary>
    public static class Signal<T>
    {
        readonly static SortedDictionary<Identity, Func<T, bool>> _listeners = new SortedDictionary<Identity, Func<T, bool>>();
        
        public static IReadOnlyCollection<Func<T, bool>> listeners => _listeners.Values;
        
        static Dictionary<Func<T, bool>, Identity> identifier = new Dictionary<Func<T, bool>, Identity>();
        
        static Dictionary<Action<T>, Func<T, bool>> actionWrapper = new Dictionary<Action<T>, Func<T, bool>>();
        
        struct Identity : IComparable<Identity>
        {
            static int counter = 0;
            public readonly int id;
            public readonly int priority;
            public Identity(int priority) => (this.priority, this.id) = (priority, unchecked(counter = counter + 1));

            public int CompareTo(Identity other)
                => this.priority != other.priority
                ? this.priority - other.priority
                : this.id - other.id;
            
            public static bool operator<(Identity a, Identity b) 
                => a.priority != b.priority
                ? a.priority < b.priority
                : a.id < b.id;
            
            public static bool operator>(Identity a, Identity b) => !(a < b);
        }
        
        /// <summary>
        /// 向该信号添加一个回调.
        /// </summary>
        public static void Listen(Action<T> f) => CreateListener(new Identity(0), f);
        
        /// <summary>
        /// 向该信号添加一个回调, 并指定回调的优先级顺序.
        /// </summary>
        public static void Listen(Action<T> f, int priority) => CreateListener(new Identity(priority), f);
        
        /// <summary>
        /// 向该信号添加一个回调.
        /// </summary>
        public static void Listen(Func<T, bool> f) => CreateListener(new Identity(0), f);
        
        /// <summary>
        /// 向该信号添加一个回调, 并指定回调的优先级顺序.
        /// </summary>
        public static void Listen(Func<T, bool> f, int priority) => CreateListener(new Identity(priority), f);
        
        /// <summary>
        /// 撤销该信号的一个回调.
        /// </summary>
        public static void Remove(Action<T> f)
        {
            var actualAction = actionWrapper[f];
            Remove(actualAction);
            actionWrapper.Remove(f);
        }
        
        /// <summary>
        /// 撤销该信号的一个回调.
        /// </summary>
        public static void Remove(Func<T, bool> f)
        {
            var id = identifier[f];
            _listeners.Remove(id);
            identifier.Remove(f);
        }
        
        /// <summary>
        /// 绑定回调, 存储回调信息.
        /// </summary>
        static void CreateListener(Identity id, Action<T> f)
        {
            CreateListener(id, CreateDefaultWrapper(f));
        }
        
        /// <summary>
        /// 绑定回调, 存储回调信息.
        /// </summary>
        static void CreateListener(Identity id, Func<T, bool> f)
        {
            identifier[f] = id;
            _listeners.Add(id, f);
        }
        
        /// <summary>
        /// 创建一个默认返回 false 的闭包.
        /// </summary>
        static Func<T, bool> CreateDefaultWrapper(Action<T> a)
        {
            Func<T, bool> wrapped = e => { a(e); return false; };
            actionWrapper.Add(a, wrapped);
            return wrapped;
        }
        
        
    }

    public static class Signal
    {
        /// <summary>
        /// 发出该信号并调用函数.
        /// </summary>
        public static void Emit<T>(T v)
        {
            foreach(var i in Signal<T>.listeners)
            {
                var shouldStop = i(v);
                if(shouldStop) break;
            }
        }
    }
}
