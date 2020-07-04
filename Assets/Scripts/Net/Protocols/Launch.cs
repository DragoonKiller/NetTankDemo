using System;
using System.Linq;
using UnityEngine;
using Systems;
using Utils;
using System.Collections.Generic;

[Serializable]
public class LaunchProtocol : Protocol
{
    [NonSerialized] public Signals.Launch e;
    
    public int id;
    public Vector2 biasVec;
    public float biasDir;
    public int launchPointId;
    
    public override void BeforeSendByHost(NetClient host, List<NetEndPoint> endpointList, ref HostSendConfig cfg)
    {
        RetriveData();
        LaunchControl.LaunchCallback(e);
    }
    
    public override void AfterReceiveByClient(NetClient client, NetEndPoint from)
    {
        RestoreData();
        LaunchControl.LaunchCallback(e);
    }

    public override void BeforeSendByClient(NetClient client, ref ClientSendConfig cfg)
    {
        RetriveData();
        // LaunchControl.LaunchCallback(e);
    }
    
    public override void AfterReceiveByHost(NetClient host, NetEndPoint from)
    {
        RestoreData();
        // Signal.Emit(e);
        host.Send(this);
    }
    
    
    void RetriveData()
    {
        // 获取玩家.
        var player = PVPData.players.Find(e.launcher.unit);
        id = player.id;
        // 记录发射点索引.
        var launchPoints = e.launcher.unit.GetComponent<LaunchGroupControl>().launches;
        for(int i = 0; i < launchPoints.Length; i++) if(launchPoints[i] == e.launcher) launchPointId = i;
        // 保存其它参数.
        biasVec = e.biasVec;
        biasDir = e.biasDir;
    }
    
    void RestoreData()
    {
        // 获取玩家.
        var player = PVPData.players[id];
        // 获取发射点.
        var launchPoint = player.unit.GetComponent<LaunchGroupControl>().launches[launchPointId];
        // 构建事件信息.
        e = new Signals.Launch() {
            launcher = launchPoint,
            biasDir = biasDir,
            biasVec = biasVec
        };
    }

}
