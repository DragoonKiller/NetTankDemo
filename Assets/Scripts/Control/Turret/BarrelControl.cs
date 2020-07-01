using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

/// <summary>
/// 根据给定的数据控制炮管的运动.
/// 炮管一般只作俯仰运动.
/// 必须挂在炮管上.
/// </summary>
public class BarrelControl : MonoBehaviour
{
    public Vector3 targetPos
    {
        get => _targetPos;
        set => _targetPos = value;
    }
    
    /// <summary>
    /// 指定目标方向.
    /// </summary>
    public Vector3 targetDir
    {
        get => targetPos - transform.position;
        set => targetPos = transform.position + value;
    }
    
    [Tooltip("最大仰角.")]
    public float maxPitch;
    
    [Tooltip("最小仰角.")]
    public float minPitch;
    
    [Tooltip("转速.")]
    public float angularSpeed;
    
    [Header("状态参数")]
    
    [Tooltip("目标位置.")]
    [ReadOnly] [SerializeField] Vector3 _targetPos;
    
    void Start()
    {
        Debug.Assert(transform.localRotation == Quaternion.identity);
    }
    
    void Update()
    {
        UpdatePitch();
    }
    
    /// <summary>
    /// 将炮管位置更新到最接近目标的角度.
    /// </summary>
    void UpdatePitch()
    {
        try
        {
            // 目标到炮管旋转平面的投影的位置.
            var targetLocal = transform.InverseTransformPoint(targetPos);
            // 目标角.
            var deltaPitch = Mathf.Atan2(targetLocal.y, targetLocal.z).ToDeg();
            // 限制转速.
            var limitSpeed = angularSpeed * Time.deltaTime;
            deltaPitch = deltaPitch.Clamp(-limitSpeed, limitSpeed);
            // 旋转.
            transform.localEulerAngles = transform.localEulerAngles.X(
                (transform.localEulerAngles.x - deltaPitch).NormAngleDeg().Clamp(minPitch, maxPitch)
            );
        }
        catch(Exception e)
        {
            // 有时候会出现值为 NAN 的情况.
            // 通常在翻车的时候....
            Debug.LogWarning(e.Message);
        }
    }
}
