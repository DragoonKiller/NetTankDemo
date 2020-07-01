using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Utils
{
    public static partial class Maths
    {
        /// <summary>
        /// 用于优化积分速度的数组.
        /// </summary>
        static List<float> internalIntegralStorage = new List<float>();
        
        /// <summary>
        /// 数值求导.
        /// </summary>
        public static float Derivative(this AnimationCurve curve, float x, float delta = 1e-4f)
        {
            var l = curve.Evaluate(x - delta * 0.5f);
            var r = curve.Evaluate(x + delta * 0.5f);
            return (r - l) / delta;
        }
        
        /// <summary>
        /// 一阶牛顿积分.
        /// </summary>
        public static float Integral(this AnimationCurve curve, float from, float to, int iteration = 20)
        {
            while(internalIntegralStorage.Count < iteration + 1) internalIntegralStorage.Add(0);
            var step = (to - from) / iteration;
            for(int i = 0; i <= iteration; i++) internalIntegralStorage[i] = curve.Evaluate(from + step * i);
            float res = 0;
            for(int i = 0; i < iteration; i++) res += (internalIntegralStorage[i] + internalIntegralStorage[i + 1]) * step;
            res *= 0.5f;
            return res;
        }
        
    }
}
