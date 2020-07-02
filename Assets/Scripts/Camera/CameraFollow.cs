using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

[RequireComponent(typeof(Camera))]
[ExecuteAlways]
public class CameraFollow : MonoBehaviour
{
    public static CameraFollow inst;
    
    CameraFollow() => inst = this;
    
    [Tooltip("跟随目标.")]
    public GameObject target;
    
    [Tooltip("最大跟随距离.")]
    public float minDistance;
    
    [Tooltip("最小跟随距离.")]
    public float maxDistance;
    
    [Tooltip("最大俯角(世界坐标).")]
    public float maxPitch;
    
    [Tooltip("最小俯角(世界坐标).")]
    public float minPitch;
    
    [Tooltip("相机移动的灵敏度.")]
    public float sensitivity;
    
    [Tooltip("缩放的灵敏度.")]
    public float scrollSensitivity;
    
    [Tooltip("摄像机竖直偏移距离. 偏移摄像机, 使其离开地表.")]
    public float upOffset;
    
    [Tooltip("固定鼠标指针.")]
    public bool lockCursor;
    
    [Tooltip("标志是否正在工作.")]
    public bool working;
    
    [Header("状态参数")]
    
    [Tooltip("当前跟随距离(世界坐标).")]
    public float curDistance;
    
    [Tooltip("当前偏航角(世界坐标).")]
    public float curYaw;
    
    [Tooltip("当前俯角(世界坐标).")]
    public float curPitch;
    
    [Header("Debug")]
    
    [Tooltip("是否在编辑器内更新摄像机.")]
    public bool updateInEditor;
    
    Camera cam => this.GetComponent<Camera>();
    Vector3 targetPos => target.transform.position;
    
    void Update()
    {
        if(!working) return;
        if(!updateInEditor && !Application.isPlaying) return;
        SetDistance();
        SetYaw();
        SetPitch();
        SyncPos();
    }
    
    /// <summary>
    /// 设置距离.
    /// </summary>
    void SetDistance()
    {
        var offset = Input.mouseScrollDelta.y;
        curDistance -= offset * scrollSensitivity;
        curDistance = curDistance.Clamp(minDistance, maxDistance);
    }
    
    /// <summary>
    /// 根据玩家操作变化偏航角.
    /// </summary>
    void SetYaw()
    {
        var offset = Input.GetAxis("Mouse X");
        curYaw += offset * sensitivity;
    }
    
    /// <summary>
    /// 根据玩家操作变化俯仰角.
    /// </summary>
    void SetPitch()
    {
        var offset = Input.GetAxis("Mouse Y");
        curPitch += offset * sensitivity;
        curPitch = curPitch.Clamp(minPitch, maxPitch);
    }
    
    /// <summary>
    /// 设置摄像机的位置和朝向.
    /// </summary>
    void SyncPos()
    {
        var offset = Quaternion.Euler(0, curYaw, 0) * Quaternion.Euler(curPitch, 0, 0) * Vector3.forward * curDistance;
        var camPos = targetPos + offset;
        if(Physics.Raycast(targetPos, offset, out var hit, offset.magnitude, LayerMask.GetMask("Terrain"))) camPos = hit.point;
        camPos += Vector3.up * upOffset;
        cam.transform.position = camPos;
        cam.transform.LookAt(targetPos);
    }
}
