using System;
using UnityEngine;
using System.Collections.Generic;
using Utils;

/// <summary>
/// 用于记录躲藏时用的路点, 以及和躲藏点相关的一切逻辑.
/// </summary>
[ExecuteAlways]
public class AIHideControl : MonoBehaviour
{
    [Tooltip("实际判定点相对于 Transform.position 的偏移.")]
    public Vector3 offset;
    
    readonly List<Transform> _waypoints = new List<Transform>();
    List<Transform> waypoints
    {
        get
        {
            _waypoints.Clear();
            for(int i = 0; i < this.transform.childCount; i++)
            {
                var x = this.transform.GetChild(i);
                for(int j = 0; j < x.childCount; j++)
                {
                    var y = x.GetChild(j);
                    _waypoints.Add(y);
                }
            }
            return _waypoints;
        }
    }
    
    /// <summary>
    /// 获取最近的隐藏点.
    /// 敌人不可见的路点就是隐藏点.
    /// </summary>
    public Vector3? ClosetWaypoint(AISearchEnemyControl searcher, UnitControl enemy, UnitControl self)
    {
        Transform closet = null;
        foreach(var i in waypoints)
        {
            // 从这个路点能看到敌人坦克, 不能选.
            if(self.CanSee(enemy, i.position, false)) continue;
            // 选离自己最近的.
            if(closet == null || Closer(i.position, closet.position, self.aimCenter)) closet = i;
        }
        
        return closet?.position;
    }
    
    bool Closer(Vector3 a, Vector3 b, Vector3 t) => a.To(t).magnitude < b.To(t).magnitude;
    
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1.0f, 0.6f, 0.6f, 1.0f);
        foreach(var i in waypoints)
        {
            Gizmos.DrawWireSphere(i.position, 3);
        }
    }
}
