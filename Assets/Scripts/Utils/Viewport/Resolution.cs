using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

public static class Resolution
{
    public static Vector2 GetMainGameViewSize()
    {
        Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
        MethodInfo GetSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView", BindingFlags.NonPublic | BindingFlags.Static);
        var Res = GetSizeOfMainGameView.Invoke(null,null);
        return (Vector2)Res;
    }
}
