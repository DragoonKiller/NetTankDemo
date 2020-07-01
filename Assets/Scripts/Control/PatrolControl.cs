using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

/// <summary>
/// 管理一组巡逻路径点.
/// </summary>
public class PatrolControl : MonoBehaviour, IEnumerable<Vector3>
{
    [Tooltip("巡逻路径点.")]
    public Transform[] patrolPoints;
    
    [Tooltip("到达路径点的半径.")]
    public float radius;
    
    public Vector3 this[int k] => patrolPoints[k].position;
    
    public int NextWaypoint(int i) => (i + 1).ModSys(patrolPoints.Length);
    
    void OnDrawGizmos()
    {
        if(patrolPoints == null) return;
        
        #if UNITY_EDITOR
        if(UnityEditor.Selection.activeObject != this.gameObject) return;
        #endif
        
        Gizmos.color = Color.white;
        foreach(var i in patrolPoints) Gizmos.DrawWireSphere(i.position, radius);
        
        for(int i = 0; i < patrolPoints.Length; i++)
        {
            var cur = i;
            var nxt = NextWaypoint(cur);
            Gizmos.DrawLine(patrolPoints[cur].position, patrolPoints[nxt].position);
        }
    }
    
    /// <summary>
    /// 是否认为到达了巡逻路点.
    /// </summary>
    public bool Reached(Vector3 from, Vector3 waypoint)
    {
        return from.To(waypoint).magnitude <= radius;
    }

    public IEnumerator<Vector3> GetEnumerator()
    {
        int cur = 0;
        while(true)
        {
            yield return patrolPoints[cur].position;
            cur = (cur + 1).ModSys(patrolPoints.Length);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
}
