using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    
    public static class DebugDraw
    {
        public static void Line(Vector3 from, Vector3 to, Color color) => Debug.DrawLine(from, to, color);
        public static void Line(Vector3 from, Vector3 to) => Debug.DrawLine(from, to);
        public static void Draw(this IReadOnlyList<Vector3> list, Color c)
        {
            for(int i = 1; i < list.Count; i++) Debug.DrawLine(list[i - 1], list[i], c);
        }
    }

}
