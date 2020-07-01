using System;
using Utils;
using Systems;
using System.Collections.Generic;

/// <summary>
/// 加入主机网络协议.
/// 客户端将该协议发送给主机.
/// 主机将发送回客户端.
/// </summary>
public class JoinProtocol : Protocol
{
    public override void AfterReceiveByClient(NetClient client, NetEndPoint from)
    {
        $"JoinProtocol Client Receive!".Log();
    }

    public override void AfterReceiveByHost(NetClient host, NetEndPoint from)
    {
        $"JoinProtocol Host Receive!".Log();
    }

    public override void BeforeSendByClient(NetClient client, ref ClientSendConfig cfg)
    {
        $"JoinProtocol Client Send!".Log();
    }

    public override void BeforeSendByHost(NetClient host, List<NetEndPoint> endpointList, ref HostSendConfig cfg)
    {
        $"JoinProtocol Host Send!".Log();
    }
}
