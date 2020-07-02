using System;
using Systems;
using UnityEngine;

/// <summary>
/// 网络玩家同步器.
/// </summary>
public class NetPlayerSync : MonoBehaviour
{
    public static NetPlayerSync inst;
    
    NetPlayerSync() => inst = this;
    
    void Update()
    {
        var client = PVPEnv.inst.client;
        if(PVPEnv.inst.isHost)
        {
            foreach(var i in PVPData.players)
            {
                client.Send(new PlayerOP() { id = i.id });
            }
        }
        else
        {
            client.Send(new PlayerOP());
        }
    }
    
}
