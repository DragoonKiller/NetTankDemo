using System;
using System.Net;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Utils
{

    public static class StringExt
    {
        /// <summary>
        /// 返回 Unity 富文本字符串, 包含颜色标签.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string WithColor(this string x, Color a) => $"<color=#{ColorUtility.ToHtmlStringRGBA(a)}>{x}</color>";
        
        /// <summary>
        /// 返回 Unity 富文本字符串, 包含粗体标签.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Bold(this string x) => $"<b>{x}</b>";
        
        /// <summary>
        /// 返回 Unity 富文本字符串, 包含斜体标签.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Italian(this string x) => $"<i>{x}</i>";
        
        /// <summary>
        /// 判断是否为空或空字符串.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmtpy(this string x) => string.IsNullOrEmpty(x);
        
        /// <summary>
        /// 判断是否为null, 为空串, 或为空白字符串.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrSpaces(this string x) => string.IsNullOrWhiteSpace(x);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ToByte(this string x) => byte.Parse(x);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToInt(this string x) => int.Parse(x);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ToLong(this string x) => long.Parse(x);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ToFloat(this string x) => float.Parse(x);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ToDouble(this string x) => double.Parse(x);
        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IPAddress ToIP(this string x) => IPAddress.Parse(x);
    }
}
