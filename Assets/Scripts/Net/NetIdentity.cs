using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class NetIdentity : MonoBehaviour
{
    public static Dictionary<int, NetIdentity> id2target = new Dictionary<int, NetIdentity>();
    
    public int netId;
    
    void Start()
    {
        if(netId == 0) throw new Exception("NetID 不能为 0.");
        id2target.Add(netId, this);
    }
    
    void OnDestroy()
    {
        id2target.Remove(netId);
    }
    
}
