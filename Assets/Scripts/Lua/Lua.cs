using System;
using System.IO;
using UnityEngine;
using XLua;
using Utils;

/// <summary>
/// Lua 脚本的全局对象.
/// </summary>
public class Lua : MonoBehaviour
{
    public static LuaEnv inst { get; private set; } = new LuaEnv();
    
    [Header("GC")]
    
    [Tooltip("在一次GC完毕后, Lua虚拟机需要申请当前内存的百分之多少, 才会重新开始GC.")]
    public int gcPause = 200;
    
    [Tooltip("GC节点扫描速度相对于内存申请节点速度的百分比.")]
    public int gcStepMultiply = 200;
    
    const string luaUtilsFilePath = "Utils/Utils.lua";
    
    static string luaUtilsPath => Path.Combine(Application.streamingAssetsPath.FullPath(), "LuaScripts", luaUtilsFilePath).FullPath();
    
    int recordGcPause;
    int recordGcStepMultiply;
    
    static Lua()
    {
        if(!File.Exists(luaUtilsPath)) Debug.LogWarning("LuaUtils文件不存在.");
        else inst.DoString(File.ReadAllText(luaUtilsPath), "Lua Init");
    }
    
    void Update()
    {
        if(gcStepMultiply <= 100) Debug.LogWarning("GCStepMultiply必须大于100, 否则可能导致垃圾回收过程永远无法执行完毕.");
        if(recordGcPause.UpdateTo(gcPause)) Lua.inst.GcPause = recordGcPause;
        if(recordGcStepMultiply.UpdateTo(gcStepMultiply)) Lua.inst.GcStepmul = recordGcStepMultiply;
        inst.Tick();
    }
    
}
