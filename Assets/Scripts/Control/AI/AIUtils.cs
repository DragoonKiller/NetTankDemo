using System;
using UnityEngine;
using Utils;

public static class AI
{
    static LayerMask mask = LayerMask.GetMask("Terrain", "Unit");
    
    /// <summary>
    /// 是否可以从 curPos 看见指定目标点. 忽略自己的遮挡.
    /// </summary>
    public static bool CanSee(this UnitControl self, UnitControl target, Vector3 curPos, bool includingUnits = true)
    {
        var targetPos = target.aimCenter;
        
        var hits = Physics.RaycastAll(curPos, curPos.To(targetPos), 1e6f, mask);
        
        // 从近到远排序.
        Array.Sort(hits, (a, b) => (a.distance - b.distance).Sgn());
        
        // 射线必须直接命中目标; 或先命一些无关紧要的东西(比如自己, 或其它单位), 再命中目标.
        for(int i = 0; i < hits.Length; i++)
        {
            // 命中了奇怪的东西.
            if(LayerMask.LayerToName(hits[i].collider.gameObject.layer) != "Unit") return false;
            
            // 带有 Unit tag 的碰撞盒必须连接到一个 Rigidbody.
            var body = hits[i].collider.attachedRigidbody;
            Debug.Assert(body != null);
            
            // 带有 Unit tag 的 Rigidbody 必须拥有 UnitControl 组件.
            UnitControl hitUnit = null;
            Debug.Assert(body.TryGetComponent(out hitUnit));
            
            // 命中了目标.
            if(hitUnit == target) return true;
            
            // 命中了自己.
            if(hitUnit == self) continue;
            
            // 如果不包括其它单位, 那么当它们不存在.
            if(!includingUnits) continue;
            else return false;
        }
        
        return false;
    }
    
    
    /// <summary>
    /// 是否可以看见指定目标点.
    /// </summary>
    public static bool CanSee(this UnitControl self, UnitControl enemy, bool includingUnits = true)
    {
        return self.CanSee(enemy, self.aimCenter, includingUnits);
    }
}
