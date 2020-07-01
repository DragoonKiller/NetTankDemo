using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

/// <summary>
/// 控制轮子.
/// 通常只用于表现, 而不用于物理模拟.
/// </summary>
public class WheelControl : MonoBehaviour
{
    [Tooltip("轮子半径.")]
    public float radius;
    
    [Header("状态参数")]
    
    [Tooltip("线速度.")]
    public float linearSpeed;
    
    void Update()
    {
        this.transform.Rotate((linearSpeed / radius).ToDeg() * Time.deltaTime, 0, 0);
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(this.transform.position, radius * this.transform.lossyScale.x);
    }
}
