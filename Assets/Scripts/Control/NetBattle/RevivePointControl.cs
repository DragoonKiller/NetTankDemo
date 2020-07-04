using System;
using System.Linq;
using UnityEngine;
using Utils;

/// <summary>
/// 决定复活点.
/// </summary>
public class RevivePointControl : MonoBehaviour
{
    public static RevivePointControl inst;
    
    public RevivePointControl() => inst = this;
    
    public float reviveRadius;
    public float testRadius;
    
    public Transform[] factionRevivePoints;
    
    void OnDrawGizmos()
    {
        if(factionRevivePoints == null) return;
        foreach(var rp in factionRevivePoints)
        {
            Gizmos.color = new Color(1, 0.5f, 0.5f, 1);
            Gizmos.DrawWireSphere(rp.transform.position, reviveRadius);
        }
    }
    
    public Vector3? Take(int faction)
    {
        faction -= 1;
        // 随机打点. 如果附近没有障碍物, 选这个点.
        for(int i = 0; i < 500; i++)
        {
            var r = (0f, reviveRadius).Random();
            var a = (0f, Mathf.PI * 2).Random();
            Vector2 delta = new Vector2(r * a.Cos(), r * a.Sin());
            var pos = factionRevivePoints[faction].transform.position + new Vector3(delta.x, 0, delta.y);
            var overlaps = Physics.OverlapSphere(pos, testRadius);
            if(overlaps.Where(x => x.gameObject.layer == LayerMask.NameToLayer("Unit")).Count() == 0) return pos;
        }
        
        return null;
    }
}
