using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine;

namespace Utils
{
    public static class ExLog
    {
        static DevLogConfig config;
        
        public static void Log(this string s)
        {
            Debug.Log(s);
            
        }
    }

}
