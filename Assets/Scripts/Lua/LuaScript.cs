using System;
using System.IO;
using System.Collections.Generic;
using System.Web;
using UnityEngine;
using XLua;
using Utils;

/// <summary>
/// Lua 脚本执行器.
/// Lua 脚本采用和 MonoBehavour 相似的格式,
/// 全局返回一个对象, 
/// </summary>
#if UNITY_EDITOR
[ExecuteAlways]
#endif
public class LuaScript : MonoBehaviour
{
    /// <summary>
    /// 存储对象的引用.
    /// </summary>
    [Serializable] struct Reference
    {
        // < field never assigned to and will always be null > 警告.
        #pragma warning disable CS0649
        public string name;
        public UnityEngine.Object target;
        #pragma warning disable CS0649
    }
    
    public string luaFilePath;
    
    #if UNITY_EDITOR
    [SerializeField] [ReadOnly] string _actualPath;
    [SerializeField] [ReadOnly] bool pathValid;
    #endif
    
    [Tooltip("从场景或资产中引用若干物体.")]
    [SerializeField] List<Reference> references = new List<Reference>();
    
    string actualPath => Path.Combine(Application.streamingAssetsPath.FullPath(), "LuaScripts", luaFilePath).FullPath();
    
    LuaTable scriptEnv;
    LuaFunction onStart;
    LuaFunction onDestroy;
    LuaFunction onUpdate;
    LuaFunction onDrawGizmos;
    
    void Start()
    {
        #if UNITY_EDITOR
        if(Application.isPlaying)
        #endif
        {
            Debug.Assert(luaFilePath != null && luaFilePath != "");
            
            scriptEnv = Lua.inst.NewTable();
            
            // 让 scriptEnv 可以获取内置函数库和全局函数.
            // 注意, 覆盖了全局名称的变量只在 scriptEnv 内生效.
            var scriptEnvMeta = Lua.inst.NewTable();
            scriptEnvMeta.Set("__index", Lua.inst.Global);
            scriptEnv.SetMetaTable(scriptEnvMeta);
            
            // 让 scriptEnv 可以向全局环境写东西.
            scriptEnv.Set("global", Lua.inst.Global);
            
            // 让脚本可以调用自己.
            scriptEnv.Set("this", this);
            scriptEnv.Set("self", scriptEnv);
            
            // 让脚本可以加载同一目录下的 lua 文件.
            scriptEnv.Set<string, Action<string>>("loadLua", LocalLoader);
            
            // 执行脚本, 读入函数.
            var ret = Lua.inst.DoString(File.ReadAllText(actualPath), $"{this.gameObject.name}:{luaFilePath}", scriptEnv);
            scriptEnv.Get("onStart", out onStart);
            scriptEnv.Get("onDestroy", out onDestroy);
            scriptEnv.Get("onUpdate", out onUpdate);
            scriptEnv.Get("onDrawGizmos", out onDrawGizmos);
            
            // 添加引用对象.
            foreach(var i in references)
            {
                if(i.target is LuaScript ia)
                {
                    // 如果同样是 Lua 脚本, 直接把 scriptEnv 丢进去, 而不是把 LuaScript 对象丢进去.
                    scriptEnv.Set(i.name, ia);
                }
                else
                {
                    // 否则让 XLua 转换对象.
                    scriptEnv.Set(i.name, i.target);
                }
            }
            
            onStart?.Call();
        }
    }
    
    #if UNITY_EDITOR
    void Update()
    {
        // 显示实际的读取路径, 以及路径是否合法.
        if(luaFilePath == null) luaFilePath = "";
        pathValid = File.Exists(actualPath);
        var projectPath = Application.dataPath.FullPath();
        _actualPath = actualPath.StartsWith(projectPath) ? actualPath.Substring(projectPath.Length) : actualPath;
        OnUpdate();
    }
    #else
    void Update() => OnUpdate();
    #endif
    
    void OnUpdate()
    {
        onUpdate?.Call();
    }
    
    void OnDrawGizmos()
    {
        onDrawGizmos?.Call();
    }
    
    void OnDestroy()
    {
        onDestroy?.Call();
        
        scriptEnv?.Dispose();
        onStart?.Dispose();
        onDestroy?.Dispose();
        onUpdate?.Dispose();
        onDrawGizmos?.Dispose();
    }
    
    /// <summary>
    /// 从一个脚本加载另一个脚本.
    /// </summary>
    void LocalLoader(string name)
    {
        if(!name.EndsWith(".lua")) name += ".lua";
        var currentPath = Path.GetDirectoryName(actualPath);
        var filePath = Path.Combine(currentPath, name).FullPath();
        Lua.inst.DoString(File.ReadAllText(filePath), $"{filePath}", scriptEnv);
    }
}
