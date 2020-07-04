using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

/// <summary>
/// 根据给定的数据控制炮台和对应炮管的运动.
/// 炮塔一般只做水平转动.
/// 必须挂在炮塔上.
/// </summary>
[RequireComponent(typeof(UnitControl))]
public class TurretControl : MonoBehaviour
{
    [Tooltip("炮台对象")]
    public GameObject turret;
    
    [Tooltip("该炮台对应的炮管.")]
    public BarrelControl[] barrels;
    
    [Tooltip("转速.")]
    public float angularSpeed;
    
    [Header("状态参数")]
    
    [Tooltip("目标位置.")]
    [ReadOnly] [SerializeField] Vector3 _targetPos;
    
    /// <summary>
    /// 指定目标位置.
    /// </summary>
    public Vector3 targetPos
    {
        get => _targetPos;
        set
        {
            _targetPos = value;
            foreach(var t in barrels) t.targetPos = value;
        }
    }
    
    /// <summary>
    /// 指定目标方向.
    /// </summary>
    public Vector3 targetDir
    {
        get => targetPos - turret.transform.position;
        set => targetPos = turret.transform.position + value;
    }
    
    /// <summary>
    /// 当前预估朝向.
    /// </summary>
    public Vector3 curDir
    {
        get
        {
            Vector3 sum = Vector3.zero;
            foreach(var b in barrels) sum += b.transform.forward;
            sum /= barrels.Length;
            return sum.normalized;
        }
    }
    
    void Start()
    {
        Debug.Assert(turret.transform.localRotation == Quaternion.identity);
    }
    
    void Update()
    {
        UpdateYaw();
    }
    
    /// <summary>
    /// 将炮管位置更新到最接近目标的角度.
    /// </summary>
    void UpdateYaw()
    {
        // 目标到炮管旋转平面的投影的位置.
        var targetLocal = turret.transform.InverseTransformPoint(targetPos);
        // 目标角.
        var deltaYaw = Mathf.Atan2(targetLocal.x, targetLocal.z).ToDeg();
        // 限制转速.
        var limitSpeed = angularSpeed * Time.deltaTime;
        deltaYaw = deltaYaw.Clamp(-limitSpeed, limitSpeed);
        // 旋转.
        turret.transform.Rotate(0, deltaYaw, 0);
    }
    
}
