using System;
using UnityEngine;
using Utils;
using Systems;
using System.Collections.Generic;

/// <summary>
/// 玩家死亡事件协议.
/// </summary>
[Serializable]
public class PlayerDeathProtocol : Protocol
{
    public int id;
    
    public override void AfterReceiveByClient(NetClient client, NetEndPoint from)
    {
        PVPData.inst.PlayerDeath(id);
    }

    public override void BeforeSendByHost(NetClient host, List<NetEndPoint> endpointList, ref HostSendConfig cfg)
    {
        PVPData.inst.PlayerDeath(id);
    }
    
    public override void AfterReceiveByHost(NetClient host, NetEndPoint from) => throw new NotImplementedException();

    public override void BeforeSendByClient(NetClient client, ref ClientSendConfig cfg) => throw new NotImplementedException();

}
