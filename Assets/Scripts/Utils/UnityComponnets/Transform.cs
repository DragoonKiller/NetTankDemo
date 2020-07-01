using UnityEngine;
using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;

namespace Utils
{
    public static class TransformExt
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Transform Clear(this Transform x)
        {
            x.localPosition = Vector3.zero;
            x.localScale = Vector3.one;
            x.localRotation = Quaternion.identity;
            return x;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<Transform> AllChildren(this Transform x, bool recursive = false)
        {
            for(int i = 0; i < x.childCount; i++)
            {
                yield return x.GetChild(i);
                if(recursive) foreach(var j in x.GetChild(i).AllChildren()) yield return j;
            }
        }
    }
}
