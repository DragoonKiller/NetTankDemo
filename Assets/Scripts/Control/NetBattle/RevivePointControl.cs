using System;
using UnityEngine;

/// <summary>
/// 存储复活点列表.
/// </summary>
public class RevivePointControl : MonoBehaviour
{
    public static RevivePointControl inst;
    
    public RevivePointControl() => inst = this;
    
    public Transform[] revivePoints;
    
    public int used;
    
    
    void OnDrawGizmos()
    {
        if(revivePoints == null) return;
        Gizmos.color = new Color(1, 0.5f, 0.5f, 1);
        for(int i = used + 1; i < revivePoints.Length; i++)
        {
            Gizmos.DrawLine(revivePoints[i - 1].position, revivePoints[i].position);
        }
    }
    
    public Vector3? Take()
    {
        if(used == revivePoints.Length) return null;
        var res = revivePoints[used].position;
        used += 1;
        return res;
    }
}
