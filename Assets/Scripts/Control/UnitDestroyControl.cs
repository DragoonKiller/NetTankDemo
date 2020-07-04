using System;
using UnityEngine;
using UnityEngine.Events;
using Utils;


/// <summary>
/// 控制单位摧毁后的行为.
/// </summary>
[RequireComponent(typeof(UnitControl))]
public class UnitDestroyControl : MonoBehaviour
{
    [Tooltip("摧毁单位时创建的内容. 会跟随单位行进.")]
    public GameObject[] generates;
    
    [Tooltip("摧毁单位时创建的内容. 会定在摧毁地点.")]
    public GameObject[] generatesWorld;
    
    [Tooltip("摧毁单位时删除的对象.")]
    public GameObject[] removeObjects;
    
    [Tooltip("摧毁单位时删除的组件.")]
    public Component[] removeComponents;
    
    [Tooltip("摧毁单位时关闭的组件.")]
    public Behaviour[] disableComponents;
    
    [Tooltip("摧毁单位事件回调")]
    public UnityEvent callback;
    
    /// <summary>
    /// 单位摧毁事件回调.
    /// </summary>
    public Action<UnitControl> unitCallback;
    
    
    /// <summary>
    /// 检测的单位.
    /// </summary>
    UnitControl target => this.GetComponent<UnitControl>();
    
    void Update()
    {
        if(target.currentStrength == 0)
        {
            UnitDestroy();
            Destroy(this);
        }
    }
    
    /// <summary>
    /// 强制触发摧毁事件.
    /// </summary>
    public void UnitDestroy()
    {
        callback.Invoke();
        unitCallback?.Invoke(target);
        if(generates != null) foreach(var i in generates) if(i) GameObject.Instantiate(i, target.transform, false);
        if(generatesWorld != null) foreach(var i in generatesWorld) if(i) GameObject.Instantiate(i, this.transform.position, this.transform.rotation);
        if(removeObjects != null) foreach(var i in removeObjects) if(i) GameObject.Destroy(i);
        if(disableComponents != null) foreach(var i in disableComponents) if(i) i.enabled = false;
        if(removeComponents != null) foreach(var i in removeComponents) if(i) Destroy(i);
    }
}
