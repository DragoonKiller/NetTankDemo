using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Utils
{

    using static Maths;

    //
    // 这个文件存储一些计算几何算法.
    // 

    public static partial class Algorthms
    {
        /// <summary>
        /// 多边形绕转次数.
        /// 按照顶点给出的顺序, 累计每个向量的绕转角度.
        /// 绕转次数是该角度和周角(2pi)的比值.
        /// 其符号由叉积正方向决定.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RevolveCount(this List<Vector2> p)
        {
            float angle = 0;
            for(int i = 0; i < p.Count; i++)
            {
                var pre = p[(i - 1).ModSys(p.Count)];
                var cur = p[i];
                var nxt = p[(i + 1).ModSys(p.Count)];
                var a = pre.To(cur).normalized;
                var b = cur.To(nxt).normalized;
                var x = a.Dot(b);
                var y = a.Cross(b);
                angle += Mathf.Atan2(y, x);
            }
            return (angle / (2 * Mathf.PI)).RoundToInt();
        }
        
        /// <summary>
        /// 如果给定顶点列表的顶点绕转方向总是不变, 返回 ture.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDirectional(this List<Vector2> p)
        {
            int dir = 0;
            for(int i = 0; i < p.Count; i++)
            {
                var pre = p[(i - 1).ModSys(p.Count)];
                var cur = p[i];
                var nxt = p[(i + 1).ModSys(p.Count)];
                var a = pre.To(cur).normalized;
                var b = cur.To(nxt).normalized;
                var res = a.Cross(b).Sgn();
                if(res == 0) continue;
                if(dir == 0) dir = res;
                else if(dir != res) return false;
            }
            return true;
        }
        
        /// <summary>
        /// 删除共线顶点.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<Vector2> RemoveColinear(this IList<Vector2> p)
        {
            var res = new List<Vector2>();
            for(int i = 0; i < p.Count; i++)
            {
                var pre = p[(i - 1).ModSys(p.Count)];
                var cur = p[i];
                var nxt = p[(i + 1).ModSys(p.Count)];
                var a = pre.To(cur).normalized;
                var b = cur.To(nxt).normalized;
                if(a.Cross(b).Sgn() != 0) res.Add(cur);
            }
            return res;
        }
        
        /// <summary>
        /// 把一个闭合的, 没有"洞"的多边形的边界转化成三角形的网格. <br/>
        /// "割耳"算法.
        /// 复杂度 n^2, n 是点数.
        /// </summary>
        public static List<Triangle> Triangulation(this List<Vector2> p)
        {
            if(p.Count < 3) return new List<Triangle>();
            if(p[p.Count - 1] == p[0]) p.RemoveAt(p.Count - 1);
            if(p.Count < 3) return new List<Triangle>();

            var res = new List<Triangle>();

            (Vector2 lv, Vector2 cv, Vector2 rv) GetTriangle(int id)
            {
                var lv = p[(id - 1).ModSys(p.Count)];
                var cv = p[id];
                var rv = p[(id + 1).ModSys(p.Count)];
                return (lv, cv, rv);
            }

            int curveSign = p.RevolveCount();
            curveSign /= curveSign.Abs();
            bool IsEar(int id)
            {
                var (lv, cv, rv) = GetTriangle(id);
                var tr = new Triangle(lv, cv, rv);
                // "耳朵"不能是凹的.
                if(tr.sign != curveSign) return false;
                // "耳朵"内部不能包含任何点.
                for(int i = 2; i < p.Count - 1; i++)
                {
                    if(tr.Contains(p[(id + i).ModSys(p.Count)], false)) return false;
                }
                return true;
            }

            int triangleCount = p.Count - 2;
            for(int t = 0; t < triangleCount; t++)
            {
                for(int i = 0; i < p.Count; i++)
                {
                    var (lv, cv, rv) = GetTriangle(i);
                    if(IsEar(i))
                    {
                        res.Add(new Triangle(lv, cv, rv));
                        p.RemoveAt(i);
                        break;
                    }
                }
            }

            return res;
        }
    }
}
