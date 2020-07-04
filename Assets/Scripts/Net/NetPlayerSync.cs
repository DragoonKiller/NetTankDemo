using System;
using Systems;
using UnityEngine;

/// <summary>
/// 网络玩家同步器.
/// </summary>
public class NetPlayerSync : MonoBehaviour
{
    public static NetPlayerSync inst;
    
    [Tooltip("复活倒计时")]
    public float reviveTime;
    
    NetPlayerSync() => inst = this;
    
    void Start()
    {
        Signal<Signals.Launch>.Listen(LocalLaunchCallback);
        Signal<Signals.Hit>.Listen(LocalHitCallback);
    }
    
    void Update()
    {
        var client = PVPEnv.inst.client;
        if(PVPEnv.inst.isHost)
        {
            // 向玩家同步其它玩家的操作和位置.
            foreach(var i in PVPData.players) client.Send(new PlayerOPProtocol() { id = i.id });
            // 向玩家同步各自的 HP.
            foreach(var i in PVPData.players) client.Send(new PlayerHpProtocol() { id = i.id });
        }
        else
        {
            // 向主机同步玩家操作和位置.
            client.Send(new PlayerOPProtocol());
        }
    }
    
    void OnDestroy()
    {
        Signal<Signals.Launch>.Remove(LocalLaunchCallback);
        Signal<Signals.Hit>.Remove(LocalHitCallback);
    }
    
    
    static void LocalLaunchCallback(Signals.Launch e)
    {
        PVPEnv.inst.client.Send(new LaunchProtocol() { e = e });
    }
    
    
    static void LocalHitCallback(Signals.Hit e)
    {
        // 记录血量.
        var prevStrength = e.hit.currentStrength;
        
        // 在本地计算血量扣除.
        UnitControl.HitCallback(e);
        
        // 如果主机上的目标死亡, 同步死亡状态.
        if(PVPEnv.inst.isHost)
        {
            if(prevStrength != 0 && e.hit.currentStrength == 0)
            {
                var id = PVPData.players.Find(e.hit).id;
                var player = PVPData.players[id];
                
                // 处理死亡流程, 把死亡信息传递给客户端.
                PVPEnv.inst.client.Send(new PlayerDeathProtocol() { id = id });
                
                // 主机准备复活.
                StateMachine.Register(new StateMachine.WaitForSeconds(NetPlayerSync.inst.reviveTime, () => {
                    // 处理复活流程, 把复活信息传递给客户端.
                    PVPEnv.inst.client.Send(new PlayerReviveProtocol() { id = id });
                }));
            }
            
        }
    }
    
}
