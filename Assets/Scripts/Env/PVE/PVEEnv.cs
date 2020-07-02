using UnityEngine;
using System;
using Utils;
using Systems;

/// <summary>
/// PVE 专用的环境.
/// </summary>
public class PVEEnv : Env
{
    public static PVEEnv inst { get; private set; }
    
    PVEEnv() => inst = this;
    
    void Start()
    {
        Signal<Signals.Launch>.Listen(LaunchControl.LaunchCallback);
        Signal<Signals.Hit>.Listen(UnitControl.HitCallback);
    }
    
    void Update()
    {
        // 全局状态机执行流程.
        StateMachine.Run();
        
        SetCursorLocked();
    }
    
    /// <summary>
    /// 强制设置光标.
    /// </summary>
    void SetCursorLocked()
    {
        #if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.V)) ExCursor.cursorLocked = !ExCursor.cursorLocked;
        #endif
    }
    
    void OnDestroy()
    {
        Signal<Signals.Launch>.Remove(LaunchControl.LaunchCallback);
        Signal<Signals.Hit>.Remove(UnitControl.HitCallback);
    }
}
