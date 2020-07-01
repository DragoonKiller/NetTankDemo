using System;
using System.Collections;
using System.Collections.Generic;

namespace Utils
{
    public struct ArrSlice<T> : IReadOnlyList<T>
    {
        public T[] original { get; private set; }
        public int from { get; private set; }
        public int to { get; private set; }
        
        public ArrSlice(T[] arr, int from, int to)
            => (this.original, this.from, this.to) = (arr, from, to);
        
        public T this[int index] => original[from + index];
        
        public int Count => to - from;
        
        public IEnumerator<T> GetEnumerator()
        {
            for(int i = from; i < to; i++) yield return original[i]; 
        }

        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
    }

    public static class ArraySliceExt
    {
        /// <summary>
        /// 截取数组中的一段. 左闭右开区间.
        /// </summary>
        public static ArrSlice<T> Slice<T>(this T[] v, int from, int to)
            => new ArrSlice<T>(v, from, to);
        
        /// <summary>
        /// 截取数组的一段前缀. n 表示截取几个元素.
        /// </summary>
        public static ArrSlice<T> Prefix<T>(this T[] v, int n)
            => new ArrSlice<T>(v, 0, n);
            
        public static ArrSlice<byte> CopyTo(this ArrSlice<byte> arr, byte[] buffer)
        {
            unsafe
            {
                fixed(byte* src = arr.original)
                fixed(byte* dst = buffer)
                {
                    System.Buffer.MemoryCopy(src + arr.from, dst, buffer.Length, arr.Count);
                }
            }
            
            return arr;
        }
    }
}
