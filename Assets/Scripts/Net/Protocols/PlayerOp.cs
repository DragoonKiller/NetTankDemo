using System;
using System.Collections.Generic;
using Systems;
using UnityEngine;
using Utils;

/// <summary>
/// 代表一个玩家操作的数据包.
/// </summary>
[Serializable]
public class PlayerOPProtocol : Protocol
{
    public int id;
    public float forwarding;
    public float turning;
    public Vector3 aimingPoint;
    public Vector3 position;
    public Vector3 velocity;
    public Quaternion rotation;
    public Vector3 angularVelocity;

    public override void BeforeSendByClient(NetClient client, ref ClientSendConfig cfg)
    {
        RetriveData(PVPData.players.me);
    }

    public override void AfterReceiveByHost(NetClient host, NetEndPoint from)
    {
        SetData();
    }
    
    public override void AfterReceiveByClient(NetClient client, NetEndPoint from)
    {
        SetData();
    }

    public override void BeforeSendByHost(NetClient host, List<NetEndPoint> endpointList, ref HostSendConfig cfg)
    {
        RetriveData(PVPData.players[id]);
    }
    
    void RetriveData(PVPPlayerEntry player)
    {
        var data = PVPData.players.me;
        if(data == null) return;            // 自己并没有控制某个单位.
        var unit = data.unit;
        if(unit == null) return;            // 处于复活阶段.
        var tank = unit.GetComponent<TankControl>();
        var turret = unit.GetComponentInChildren<TurretControl>();
        var body = tank.GetComponent<Rigidbody>();
        id = data.id;
        forwarding = tank.forwarding;
        turning = tank.turning;
        aimingPoint = turret.targetPos;
        position = tank.transform.position;
        rotation = tank.transform.rotation;
        velocity = body.velocity;
        angularVelocity = body.angularVelocity;
    }
    
    
    void SetData()
    {
        var data = PVPData.players[id];
        if(data == null) return;            // 已经拿到了同步信号, 但是这个对象还没创建, 或者已经取消控制.
        if(id == PVPData.inst.id) return;   // 不通过网络控制自己.
        var unit = data.unit;
        if(unit == null) return;            // 处于复活阶段.
        var tank = unit.GetComponent<TankControl>();
        var turret = unit.GetComponentInChildren<TurretControl>();
        var body = tank.GetComponent<Rigidbody>();
        tank.forwarding = forwarding;
        tank.turning = turning;
        turret.targetPos = aimingPoint;
        tank.transform.position = position;
        tank.transform.rotation = rotation;
        body.velocity = velocity;
        body.angularVelocity = angularVelocity;
    }
    
}
