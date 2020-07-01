using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Utils;

/// <summary>
/// 控制一侧履带的行为.
/// </summary>
public class TrackControl : MonoBehaviour
{
    [Tooltip("驱动轮.")]
    public WheelCollider[] driverWheels;
    
    [Tooltip("速度计算轮.")]
    public WheelCollider[] computingWheels;
    
    [Tooltip("驱动轮权重.")]
    public float[] driverWheelWeights;
    
    [Tooltip("物理轮. 每个履带控制点安置一个. 用于自动计算轮子的位置. 不做任何加力操作.")]
    public WheelCollider[] fakeWheels;
    
    [Tooltip("表现轮. 每个模型中的轮子安置一个表现轮.")]
    public WheelControl[] visualWheels;
    
    [Tooltip("履带骨骼控制点.")]
    public Transform[] tracks;
    
    [Tooltip("最大前向功率.")]
    public float frontPower;
    
    [Tooltip("最大后向功率.")]
    public float backPower;
    
    [Tooltip("最大刹车扭矩. 会被均匀分配到物理轮上.")]
    public float maxBreakTorque;
    
    [Tooltip("履带材质滚动速度.")]
    public float rollingSpeed;
    
    [Tooltip("履带材质.")]
    public Renderer rd;
    
    [Header("状态参数")]
    
    [Tooltip("操作杆.")]
    [Range(-1, 1)] public float output;
    
    Vector3[] visualWheelBasePos;
    Vector3[] physWheelBasePos;
    Vector3[] trackBasePos;
    
    float curLinearSpeed 
    { 
        get
        {
            var a = computingWheels.Where(x => x.isGrounded);
            if(a.Count() != 0) return a.Average(x => x.rpm / 60 * Mathf.PI * 2 * x.radius);
            return 0;
        }
    }
    void Start()
    {
        // 记录初始位置, 用偏移操作位置.
        visualWheelBasePos = visualWheels.Select(x => x.transform.localPosition).ToArray();
        physWheelBasePos = fakeWheels.Select(x => x.transform.localPosition).ToArray();
        trackBasePos = tracks.Select(x => x.transform.localPosition).ToArray();
        
        // 把材质复制一份. 不能更改全局材质.
        rd.material = new Material(rd.material);
    }
    
    void Update()
    {
        if(output == 0) Break();
        else if(output > 0) GoFront(output);
        else GoBack(-output);
        
        SyncWheelsRPM();
        SyncWheelPos();
        SyncTrack();
        
        SyncMat();
    }
    
    /// <summary>
    /// 刹车.
    /// </summary>
    void Break()
    {
        for(int i = 0; i < driverWheels.Length; i++)
        {
            driverWheels[i].brakeTorque = maxBreakTorque * driverWheelWeights[i];
            driverWheels[i].motorTorque = 0;
        }
    }
    
    /// <summary>
    /// 向前进.
    /// </summary>
    void GoFront(float outputRate)
    {
        for(int i = 0; i < driverWheels.Length; i++)
        {
            driverWheels[i].brakeTorque = 0;
            driverWheels[i].motorTorque = frontPower * driverWheelWeights[i];
        }
    }
    
    /// <summary>
    /// 倒车.
    /// </summary>
    void GoBack(float outputRate)
    {
        for(int i = 0; i < driverWheels.Length; i++)
        {
            driverWheels[i].brakeTorque = 0;
            driverWheels[i].motorTorque = -backPower * driverWheelWeights[i];
        }
    }
    
    /// <summary>
    /// 同步表现轮转速.
    /// </summary>
    void SyncWheelsRPM()
    {
        var groundedWheels = fakeWheels.Where(x => x.isGrounded);
        foreach(var w in visualWheels) w.linearSpeed = curLinearSpeed;
    }
    
    /// <summary>
    /// 从物理轮同步表现轮位置.
    /// </summary>
    void SyncWheelPos()
    {
        var cnt = visualWheels.Length.Min(fakeWheels.Length);
        for(int i = 0; i < cnt; i++)
        {
            fakeWheels[i].GetWorldPose(out var wPos, out _);
            var delta = wPos - fakeWheels[i].transform.position;
            visualWheels[i].transform.localPosition = delta + visualWheelBasePos[i];
        }
    }
    
    /// <summary>
    /// 从表现轮同步履带位置.
    /// </summary>
    void SyncTrack()
    {
        var cnt = fakeWheels.Length.Min(tracks.Length);
        for(int i = 0; i < cnt; i++)
        {
            var offset = visualWheels[i].transform.localPosition - visualWheelBasePos[i];
            tracks[i].localPosition = trackBasePos[i] + offset;
        }
    }
    
    /// <summary>
    /// 根据线速度更新履带贴图.
    /// </summary>
    void SyncMat()
    {
        rd.material.mainTextureOffset += new Vector2(curLinearSpeed * Time.deltaTime * rollingSpeed, 0);
    }
}
