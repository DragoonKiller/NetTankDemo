using System;
using System.Linq;
using Utils;
using Systems;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 加入主机网络协议.
/// 客户端将该协议发送给主机.
/// 主机在本机创建坦克, 生成 ID.
/// 主机将id和其他信息向所有客户端发送.
/// 客户端生成所有玩家坦克.
/// 客户端会检测坦克是否已经生成了.
/// </summary>
[Serializable]
public class JoinProtocol : Protocol
{
    [Serializable]
    public struct PlayerEntry
    {
        public NetEndPoint endpoint;
        public int id;
        public int faction;
        public Vector3 revivePoint;
        public Vector3 pos;
        public Quaternion rot;
    }
    
    public List<PlayerEntry> players = new List<PlayerEntry>();
    
    public override void AfterReceiveByClient(NetClient client, NetEndPoint from)
    {
        // $"Client Receive!".Log();
        foreach(var p in players)
        {
            var f = PVPData.players.FirstOrDefault(r => r.id == p.id);
            
            // 已经有这个玩家了, 检查, 跳过.
            if(f != null)
            {
                Debug.Assert(f.endpoint == p.endpoint);
                Debug.Assert(f.id == p.id);
                Debug.Assert(f.faction == p.faction);
                Debug.Assert(f.revivePoint == p.revivePoint);
                continue;
            }
            
            // 创建玩家坦克.
            var x = PVPData.inst.CreatePlayer(p.endpoint, p.faction, p.id, p.pos, p.rot, p.revivePoint);
            
            // 创建的是自己的坦克, 绑控制到上面.
            if(p.endpoint == client.localEndpoint)
            {
                PVPData.inst.SetPlaying(x);
            }
        }
    }

    public override void AfterReceiveByHost(NetClient host, NetEndPoint from)
    {
        // $"Host Receive!".Log();
        PVPData.inst.CreatePlayer(from);
        foreach(var p in PVPData.players) players.Add(new PlayerEntry() { 
            endpoint = p.endpoint,
            id = p.id,
            faction = p.faction,
            revivePoint = p.revivePoint,
            pos = p.unit.transform.position,
            rot = p.unit.transform.rotation
        });
        host.Send(this);
    }

    public override void BeforeSendByClient(NetClient client, ref ClientSendConfig cfg)
    {
        // $"Client Send!".Log();
    }

    public override void BeforeSendByHost(NetClient host, List<NetEndPoint> endpointList, ref HostSendConfig cfg)
    {
        // $"Host Send!".Log();
    }
}
