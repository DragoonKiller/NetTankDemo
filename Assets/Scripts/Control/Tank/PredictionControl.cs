using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Utils;
using UnityEngine.UI;

/// <summary>
/// 预测炮弹落点.
/// </summary>
public class PredictionControl : MonoBehaviour
{
    [Tooltip("需要预测的发射点. 这些发射点的落点取平均就是输出落点.")]
    public LaunchControl[] launchers;
    
    /// <summary>
    /// 预测落点.
    /// </summary>
    public Vector3? hitPoint { get; private set; }
    
    /// <summary>
    /// 预测落点是否击中了一个目标.
    /// </summary>
    public UnitControl hitUnit { get; private set; }
    
    LayerMask cannonballHit;
    
    void Start()
    {
        cannonballHit = LayerMask.GetMask("Unit", "Terrain");
    }
    
    void Update()
    {
        var hits = launchers
        .Select(GetHitPoint)
        .Where(x => x.collider != null);
        hitPoint = hits.Aggregate(Vector3.zero, (a, x) => x.point + a) / hits.Count();
        hitUnit = hits
            .Select(x => x.collider?.attachedRigidbody?.GetComponent<UnitControl>())
            .Where(x => x != null)
            .FirstOrDefault();
    }
    
    /// <summary>
    /// 获取炮弹落点.
    /// </summary>
    RaycastHit GetHitPoint(LaunchControl launcher)
    {
        // 计算以时刻 t 为参数的抛物线方程.
        var launchPoint = launcher.transform.position;
        var launchVelocity = launcher.transform.forward * launcher.cannonBallTemplate.speed;
        var gravity = Physics.gravity;
        // 以 t = 0.1s 的步长测试交点.
        // 最多测试次数.
        int maxTestCount = 20;
        float t = 0;
        for(var _ = 0; _ < maxTestCount; _++)
        {
            var nxt = t + 0.1f;
            var curPos = launchPoint + launchVelocity * t + 0.5f * gravity * t * t;
            var nxtPos = launchPoint + launchVelocity * nxt + 0.5f * gravity * nxt * nxt;
            if(Physics.Raycast(new Ray(curPos, curPos.To(nxtPos)), out var hit, curPos.To(nxtPos).magnitude, cannonballHit))
            {
                return hit;
            }
            t = nxt;
        }
        
        return new RaycastHit();
    }
}
