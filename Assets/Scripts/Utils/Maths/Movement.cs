using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Utils
{
    public static partial class Maths
    {
        // 速度随时间变化公式, 其中 H是额定速度, a是功率, C是当前速度:
        // f(t) = H - 2^-at * (H - C)
        // 就有:
        // f(r) - f(l) = (H - C) (- 2^-ar + 2^-al)
        //   = (H - C) a^-l (- 2^-a(r-l) + 1 )
        //   = (H - f(l)) (-2^-adt + 1)
        // 所以欧拉插值法用递推式: v(r) = v(l) + (H - v(l)) (1 - 2^-adt)
        
        /// <summary>
        /// 功率曲线欧拉插值. targetV 是目标速度, curV 是当前速度, acc 是功率, dt 是插值时间. 
        /// </summary>
        public static float PowerStep(float curV, float targetV, float acc, float dt) => curV + (targetV - curV) * (1f - 2.Pow(-acc * dt));
        
        /// <summary>
        /// 功率曲线欧拉插值. targetV 是目标速度, curV 是当前速度, acc 是功率, dt 是插值时间. 
        /// </summary>
        public static Vector2 PowerStep(Vector2 curV, Vector2 targetV, float acc, float dt) => curV + (targetV - curV) * (1f - 2.Pow(-acc * dt));
        
    }
}
