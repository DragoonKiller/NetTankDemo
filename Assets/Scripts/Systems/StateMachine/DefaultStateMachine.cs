using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using Utils;

namespace Systems
{
    /// <summary>
    /// 存储一些预定义的通用的状态机.
    /// 有如下约定:
    /// * StateMachine.Run() 应当随着时间推进而不断地执行.
    /// * StateMachine.Run() 应当每帧执行一次.
    /// </summary>
    public partial class StateMachine
    {
        /// <summary>
        /// 等待若干秒. 当等待时长大于等于给定数值时, 状态机结束运行.
        /// </summary>
        public sealed class WaitForSeconds : StateMachine
        {
            float time;
            Action callback;
            
            public WaitForSeconds(float t) => this.time = t;
            public WaitForSeconds(float t, Action callback) => (this.time, this.callback) = (t, callback);
            
            public override IEnumerable<Transfer> Step()
            {
                var beginTime = Time.time;
                while(Time.time - beginTime < time) yield return Pass();
                callback?.Invoke();
            }
        }
        
        /// <summary>
        /// 等待若干帧. 当状态机存在的帧数大于等于给定数值时, 状态机结束运行.
        /// </summary>
        public sealed class WaitForFrames : StateMachine
        {
            int time;
            Action callback;
            
            public WaitForFrames(int t, Action callback) => (this.time, this.callback) = (t, callback);
            public WaitForFrames(int t) => time = t;

            public override IEnumerable<Transfer> Step()
            {
                for(int i = 0; i < time; i++) yield return Pass();
                callback?.Invoke();
            }
        }
        
        /// <summary>
        /// 循环计时触发器. 计时经过 0 时触发一次并重置计时.
        /// </summary>
        public sealed class Timer : StateMachine
        {
            float period;
            Func<float> getPeriod;
            Action callback;
            public Timer(float period, Action callback)
                => (this.period, this.callback) = (period, callback);
                
            public Timer(Func<float> getPeriod, Action callback)
                => (this.getPeriod, this.callback) = (getPeriod, callback);
            
            public float GetNextPeriod() => getPeriod == null ? period : getPeriod();
            
            public override IEnumerable<Transfer> Step()
            {
                float curTime = 0f;
                while(true)
                {
                    if(curTime.CountDownTime() == 0)
                    {
                        callback?.Invoke();
                        curTime = GetNextPeriod();
                    }
                    yield return Pass();
                }
            }
        }
        
        /// <summary>
        /// 事件触发器.
        /// </summary>
        public sealed class WaitFor : StateMachine
        {
            Func<bool> condition;
            Action callback;
            public WaitFor(Func<bool> condition, Action callback) => (this.condition, this.callback) = (condition, callback);
            public WaitFor(Func<bool> condition) => this.condition = condition;
            public override IEnumerable<Transfer> Step()
            {
                while(!condition()) yield return Pass();
                callback?.Invoke();
            }
        }
    }
}
