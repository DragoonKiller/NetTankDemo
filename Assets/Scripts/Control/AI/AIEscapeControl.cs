using System;
using UnityEngine;
using System.Collections.Generic;
using Utils;

/// <summary>
/// 用于记录逃跑和躲藏时用的路点, 以及和逃跑点相关的一切逻辑.
/// </summary>
[ExecuteAlways]
public class AIEscapeControl : MonoBehaviour
{
    Transform[] waypoints
    {
        get
        {
            var x = new Transform[this.transform.childCount];
            for(int i = 0; i < this.transform.childCount; i++) x[i] = this.transform.GetChild(i);
            return x;
        }
    }
    
    /// <summary>
    /// 获取最远的逃脱点.
    /// 直接获取离敌人最远的点, 不管是否可见.
    /// </summary>
    public Vector3? EscapeWaypoint(Vector3 enemyPos)
    {
        Transform closet = null;
        foreach(var i in waypoints)
            if(closet == null || Closer(closet.position, i.position, enemyPos)) closet = i;
        return closet?.position;
    }
    
    bool Closer(Vector3 a, Vector3 b, Vector3 t) => a.To(t).magnitude < b.To(t).magnitude;
    
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.8f, 0.6f, 0.4f, 1.0f);
        foreach(var i in waypoints)
        {
            Gizmos.DrawWireSphere(i.position, 2);
        }
    }
}
