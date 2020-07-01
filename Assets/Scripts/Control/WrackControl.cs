using System;
using UnityEngine;
using Utils;

/// <summary>
/// 控制残骸不停冒烟.
/// </summary>
public class WrackControl : MonoBehaviour
{
    [Tooltip("生成的内容.")]
    public GameObject[] generates;
    
    [Tooltip("生成周期.")]
    public float span;
    
    /// <summary>
    /// 生成事件计时.
    /// </summary>
    float cooldown;
    
    void Update()
    {
        if(cooldown.CountDownTimeLoop(span))
        {
            foreach(var i in generates) GameObject.Instantiate(i, this.transform);
        }
    }
}
