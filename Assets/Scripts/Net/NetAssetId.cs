using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 使用整数来对应资产.
/// 由于 Unity 引擎限制, 不能直接使用 InstanceID 获取对应 GameObject.
/// 不过 JsonUtility.FromJsonOverwrite() 能够正确读取这些资产.
/// </summary>
[CreateAssetMenu(fileName = "Net Identified Asset", menuName = "Network/Identified Asset", order = 1)]
public class NetAssetId : ScriptableObject
{
    public static NetAssetId inst;
    
    Dictionary<int, UnityEngine.Object> id2Object;
    
    Dictionary<UnityEngine.Object, int> object2Id;
    
    public bool inited = false;
    
    public List<UnityEngine.Object> entry = new List<UnityEngine.Object>();
    
    NetAssetId() => inst = this;
    
    public UnityEngine.Object this[int id]
    {
        get
        {
            if(id2Object == null) InitId2Object();
            return id2Object[id];
        }
    }
    
    public int this[UnityEngine.Object obj]
    {
        get
        {
            if(object2Id == null) InitObject2Id();
            return object2Id[obj];
        }
    }
    
    void InitId2Object()
    {
        inited = true;
        id2Object = new Dictionary<int, UnityEngine.Object>();
        for(int i = 0; i < entry.Count; i++) id2Object.Add(i + 1, entry[i]);
    }
    
    void InitObject2Id()
    {
        inited = true;
        object2Id = new Dictionary<UnityEngine.Object, int>();
        for(int i = 0; i < entry.Count; i++) object2Id.Add(entry[i], i + 1);
    }
}
