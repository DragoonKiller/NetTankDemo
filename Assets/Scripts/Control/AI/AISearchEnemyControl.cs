using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using Systems;

public class AISearchEnemyControl : MonoBehaviour
{
    [Tooltip("挂载了AI的单位.")]
    public UnitControl unit;
    
    [Tooltip("索敌范围.")]
    public float searchRange;
    
    [Header("状态参数")]
    
    [Tooltip("目标单位. 由自己搜索决定.")]
    [SerializeField] [ReadOnly] UnitControl _target;
    
    public UnitControl target
    {
        get => _target != null && _target.dead ? null : _target;
        private set => _target = value;
    }
    
    /// <summary>
    /// 存储物理判交结果.
    /// </summary>
    public readonly Collider[] colliderResult = new Collider[128];
    
    /// <summary>
    /// 搜索范围内的目标.
    /// </summary>
    public List<UnitControl> sensedTarget = new List<UnitControl>();
    
    /// <summary>
    /// 直线可见的目标.
    /// </summary>
    public List<UnitControl> visibleTarget = new List<UnitControl>();
    
    /// <summary>
    /// 避免重复添加相同的 UnitControl 进目标列表.
    /// </summary>
    readonly HashSet<UnitControl> takedControls = new HashSet<UnitControl>();
    
    void Start()
    {
        Signal<Signals.Hit>.Listen(PassiveSetTarget);
    }
    
    void Update()
    {
        SyncTargetInRange();
        InitiativeSetTarget();
        CancelTarget();
    }
    
    void OnDestroy()
    {
        Signal<Signals.Hit>.Remove(PassiveSetTarget);
    }
    
    /// <summary>
    /// 主动搜寻.
    /// </summary>
    void InitiativeSetTarget()
    {
        if(visibleTarget.Count != 0) target = visibleTarget[0];
    }
    
    /// <summary>
    /// 被动搜寻.
    /// </summary>
    void PassiveSetTarget(Signals.Hit hit)
    {
        if(hit.hit != unit) return;
        target = hit.source;
    }
    
    /// <summary>
    /// 取消搜索.
    /// </summary>
    void CancelTarget()
    {
        if(target != null && !unit.CanSee(target))
        target = null;
    }
    
    /// <summary>
    /// 搜索射程范围内的目标.
    /// </summary>
    void SyncTargetInRange()
    {
        sensedTarget.Clear();
        visibleTarget.Clear();
        takedControls.Clear();
        
        int castCnt = Physics.OverlapSphereNonAlloc(unit.aimCenter, searchRange, colliderResult, LayerMask.GetMask("Unit"));
        
        // 设置 SensedTarget.
        for(int i = 0; i < castCnt; i++)
        {
            if(colliderResult[i].attachedRigidbody.TryGetComponent<UnitControl>(out var unit))
            {
                if(!unit.dead && unit.factionId != this.unit.factionId && !takedControls.Contains(unit))
                {
                    sensedTarget.Add(unit);
                    takedControls.Add(unit);
                }
            }    
        }
        
        // 按距离排序.
        var curPos = unit.aimCenter;
        sensedTarget.Sort((a, b) => (a.transform.position.To(curPos).sqrMagnitude - b.transform.position.To(curPos).sqrMagnitude).Sgn());
        
        // 设置 visibleTarget.
        foreach(var target in sensedTarget) if(unit.CanSee(target)) visibleTarget.Add(target);
    }
    
    
    void OnDrawGizmos()
    {
        if(unit)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(unit.aimCenter, searchRange);
        }
    }
    
    /// <summary>
    /// 是否在索敌范围内.
    /// </summary>
    public bool InRange(Vector3 pos) => unit.aimCenter.To(pos).magnitude < searchRange;
}
