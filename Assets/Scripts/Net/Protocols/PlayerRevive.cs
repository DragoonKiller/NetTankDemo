using System;
using UnityEngine;
using Utils;
using Systems;
using System.Collections.Generic;

/// <summary>
/// 玩家复活事件协议.
/// </summary>
[Serializable]
public class PlayerReviveProtocol : Protocol
{
    public int id;
    public Vector3 revivePoint;
    
    public override void AfterReceiveByClient(NetClient client, NetEndPoint from)
    {
        PVPData.inst.PlayerRevive(id, revivePoint);
    }
    
    public override void BeforeSendByHost(NetClient host, List<NetEndPoint> endpointList, ref HostSendConfig cfg)
    {
        revivePoint = RevivePointControl.inst.Take(PVPData.players[id].faction).Value;
        PVPData.inst.PlayerRevive(id, revivePoint);
    }
    
    public override void AfterReceiveByHost(NetClient host, NetEndPoint from) => throw new NotImplementedException();

    public override void BeforeSendByClient(NetClient client, ref ClientSendConfig cfg) => throw new NotImplementedException();

}
