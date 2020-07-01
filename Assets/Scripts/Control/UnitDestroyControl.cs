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
        foreach(var i in generates) GameObject.Instantiate(i, target.transform, false);
        foreach(var i in generatesWorld) GameObject.Instantiate(i, this.transform.position, this.transform.rotation);
        foreach(var i in removeObjects) GameObject.Destroy(i);
        foreach(var i in disableComponents) i.enabled = false;
        foreach(var i in removeComponents) Destroy(i);
    }
}
