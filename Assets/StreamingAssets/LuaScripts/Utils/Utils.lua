-- 存储泛用的 Lua 函数.
-- 这个脚本会在 Lua 虚拟机初始化时加载进 Lua 虚拟机.
-- 想要移动该文件的位置, 请同步更改 Scripts/Lua/Lua.cs 中的路径. 

-- print("Lua Utils Load!")

GetGenericMethod = function (class, methodName, ...)
    local method = xlua.get_generic_method(class, methodName)
    return method(...)
end

-- 定义一些便于使用的常量.

Unity = CS.UnityEngine
Editor = CS.UnityEditor
UnityUI = CS.UnityEngine.UI
System = CS.System
Collections = CS.System.Collections.Generic
App = CS.UnityEngine.Application
EditorApp = CS.UnityEditor.EditorApplication
SceneManager = CS.UnityEngine.SceneManagement.SceneManager
Scene = CS.UnityEngine.SceneManagement
