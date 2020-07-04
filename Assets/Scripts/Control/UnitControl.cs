using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Utils;
using System;
using Systems;

public partial class Signals
{
    /// <summary>
    /// 存储命中信息.
    /// </summary>
    public struct Hit
    {
        public UnitControl hit;
        public UnitControl source;
        public float amount;
    }
}

/// <summary>
/// 一个单位的控制.
/// 单位包含血量, 所属阵营等属性.
/// 必须把该脚本挂在单位的受击碰撞盒位置, 以方便碰撞体获取该脚本.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class UnitControl : MonoBehaviour
{
    [Tooltip("所属阵营.")]
    public int factionId;
    
    [Tooltip("最大生命值.")]
    public float maxStrength;
    
    [Tooltip("设置无敌. 血量不会减少(但是会增加).")]
    public bool invincible;
    
    [Tooltip("威胁值从1减少到0的耗时.")]
    public float threatReduceTime;
    
    /// <summary>
    /// AI 锁定这一单位的瞄准点相对于单位锚点的偏移.
    /// </summary>
    public Vector3 aimOffset;
    
    [Header("状态参数")]
    
    [Tooltip("当前生命值.")]
    [SerializeField] float _currentStrength;
    
    [Tooltip("威胁值.")]
    public float threat;
    
    /// <summary>
    /// AI 锁定这一单位的瞄准点.
    /// </summary>
    public Vector3 aimCenter => aimOffset + this.transform.position;
    
    /// <summary>
    /// 单位是否死亡.
    /// </summary>
    public bool dead => currentStrength == 0;
    
    public Rigidbody body => this.GetComponent<Rigidbody>();
    
    /// <summary>
    /// 单位当前生命值.
    /// </summary>
    public float currentStrength
    {
        get => _currentStrength;
        set => _currentStrength = value.Clamp(invincible ? _currentStrength : 0, maxStrength);
    }
    
    void Update()
    {
        threat = (threat - Time.deltaTime / threatReduceTime).Max(0);
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(aimCenter, 0.5f);
        Gizmos.DrawWireSphere(aimCenter, 0.6f);
        Gizmos.DrawWireSphere(aimCenter, 0.7f);
        Gizmos.DrawWireSphere(aimCenter, 0.8f);
    }
    
    public bool HasSameFaction(UnitControl x) => x.factionId != factionId;
    
    
    public static void HitCallback(Signals.Hit hit)
    {
        if(hit.source != null && hit.source.factionId == hit.hit.factionId) return;
        hit.hit.currentStrength -= hit.amount;
    }
}
