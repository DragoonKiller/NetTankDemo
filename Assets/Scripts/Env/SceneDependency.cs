using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.Linq;
using System;
using System.IO;
using Utils;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
[ExecuteInEditMode]
public class SceneDependency : MonoBehaviour
{
    [Tooltip("依赖的场景.")]
    public string[] requiredSceneNames;
    
    bool[] loaded;
    
    void Update()
    {
        if(requiredSceneNames == null) return;
        if(loaded == null || loaded.Length != requiredSceneNames.Length) loaded = new bool[requiredSceneNames.Length];
        
        for(int i = 0; i < loaded.Length; i++) loaded[i] = false;
        
        for(int i = 0; i < EditorSceneManager.loadedSceneCount; i++)
        {
            var scene = EditorSceneManager.GetSceneAt(i);
            int index = requiredSceneNames.FindIndex(x => x == scene.name);
            if(index == -1) continue;
            loaded[index] = true;
        }
        
        for(int i = 0; i < loaded.Length; i++) if(!loaded[i])
        {
            if(Application.isEditor && !Application.isPlaying)
            {
                var path = Path.Combine("Assets", "Scenes", requiredSceneNames[i] + ".unity");
                EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            }
        }
    }
}

#else

public class SceneDependency : MonoBehaviour
{
    
}

#endif 
