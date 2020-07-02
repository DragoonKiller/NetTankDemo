using System;
using UnityEngine;

namespace Utils
{
    public static class SerializeExt
    {
        public static string Serialize<T>(this T x) => JsonUtility.ToJson(x, false);
        public static object Deserialize<T>(this string x) where T : class => JsonUtility.FromJson<T>(x);
        public static void SyncFrom<T>(this T x, string jsonData) where T : class => JsonUtility.FromJsonOverwrite(jsonData, x);
        
    }
}
