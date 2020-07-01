using System;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Generic;

namespace Utils
{
    /// <summary>
    /// 一个可以直接获取到底层数组的 List.
    /// </summary>
    public class ArrayList<T> : IList<T>
    {
        /// <summary>
        /// 内部数据数组.
        /// </summary>
        T[] data = new T[4];
        
        /// <summary>
        /// 元素计数.
        /// </summary>
        int cnt;
        
        public T this[int k]
        { 
            get => data[k];
            set => data[k] = value;
        }
        
        public T this[long k]
        { 
            get => data[k];
            set => data[k] = value;
        }
        
        public T this[uint k]
        { 
            get => data[k];
            set => data[k] = value;
        }
        
        public T this[ulong k]
        { 
            get => data[k];
            set => data[k] = value;
        }
        

        public int Count => cnt;

        public bool IsReadOnly => false;

        public void Add(T x)
        {
            if(data.Length == cnt) Resize(cnt * 2);
            data[cnt++] = x;
        }

        public void Clear()
        {
            cnt = 0;
        }

        public bool Contains(T item)
        {
            foreach(var i in data.ToEnumerable(0, cnt)) if(item.Equals(i)) return true;
            return false;
        }
        
        public void CopyTo(T[] array, int targetBegin) => CopyTo(array, targetBegin, 0);
        
        /// <summary>
        /// 把自己的从 curBegin 开始的数据拷入目标数组从 targetBegin 开始的部分.
        /// </summary>
        public void CopyTo(T[] array, int targetBegin, int curBegin) => CopyTo(array, targetBegin, curBegin, cnt);
        
        /// <summary>
        /// 把自己数组中, 以 arrayIndex 为起始下标的数据拷入 array.
        /// 左闭右开区间.
        /// 如果目标数组方不下则只拷贝一部分数据.
        /// </summary>
        public void CopyTo(T[] array, int targetBegin, int curBegin, int curEnd)
        {
            int cc = Math.Min(curEnd - curBegin, array.Length - targetBegin);
            for(int i = 0; i < cc; i++) array[i + targetBegin] = data[i + curBegin];
        }

        public IEnumerator<T> GetEnumerator() => data.ToEnumerable(0, cnt).GetEnumerator();
        
        /// <summary>
        /// 搜索元素对应的下标.
        /// </summary>
        public int IndexOf(T item)
        {
            for(int i = 0; i < cnt; i++) if(item.Equals(data[i])) return i;
            return -1;
        }

        /// <summary>
        /// 向某个下标的元素前面插入元素.
        /// 插入后, 插入元素的下标为给定下标.
        /// </summary>
        public void Insert(int index, T item)
        {
            if(data.Length == cnt) Resize(cnt * 2);
            for(int i = cnt - 1; i >= index; i--) data[i + 1] = data[i];
            data[index] = item;
            cnt++;
        }

        /// <summary>
        /// 删除元素.
        /// </summary>
        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if(index == -1) return false;
            RemoveAt(index);
            return true;
        }

        /// <summary>
        /// 删除指定位置的元素.
        /// </summary>
        public void RemoveAt(int index)
        {
            for(int i = index; i < cnt; i++) data[i] = data[i + 1];
            cnt--;
        }

        IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();
        
        // ====================================================================
        // 扩展功能.
        // ====================================================================
        
        public T[] array => data;
        
        /// <summary>
        /// 预留给定数值的空间.
        /// 如果空间已经足够则什么都不做.
        /// </summary>
        public ArrayList<T> Reserve(int size)
        {
            if(size > data.Length) Resize(size);
            return this;
        }
        
        // ====================================================================
        // 内部工具函数
        // ====================================================================
        
        /// <summary>
        /// 重新申请此大小的数组.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Resize(int cnt)
        {
            var prevData = data;
            data = new T[cnt];
            var size = Math.Min(prevData.Length, cnt);
            for(int i = 0; i < size; i++) data[i] = prevData[i];
        }
    }
}
