using System;
using UnityEngine;
using Utils;
using Systems;
using System.Collections.Generic;

/// <summary>
/// Hp 同步协议.
/// </summary>
[Serializable]
public class PlayerHpProtocol : Protocol
{
    public int id;
    public float hp;
    
    public override void AfterReceiveByClient(NetClient client, NetEndPoint from)
    {
        var player = PVPData.players[id];
        if(player == null || player.unit == null) return;
        player.unit.currentStrength = hp;
    }

    public override void BeforeSendByHost(NetClient host, List<NetEndPoint> endpointList, ref HostSendConfig cfg)
    {
        var player = PVPData.players[id];
        if(player == null || player.unit == null) return;
        hp = player.unit.currentStrength;
    }
    
    public override void AfterReceiveByHost(NetClient host, NetEndPoint from) => throw new NotImplementedException();

    public override void BeforeSendByClient(NetClient client, ref ClientSendConfig cfg) => throw new NotImplementedException();
    
}
