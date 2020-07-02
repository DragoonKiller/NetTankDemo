using System;
using System.Linq;
using System.Collections.Generic;
using Systems;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using System.Collections;

/// <summary>
/// 存储一个 PVP 玩家的相关信息.
/// </summary>
[Serializable]
public class PVPPlayerEntry
{
    /// <summary>
    /// 该玩家的网络端口.
    /// </summary>
    [ReadOnly] public NetEndPoint endpoint;
    
    /// <summary>
    /// 该玩家的 ID.
    /// </summary>
    [ReadOnly] public int id;
    
    /// <summary>
    /// 该玩家的从属阵营.
    /// </summary>
    [ReadOnly] public int faction;
    
    /// <summary>
    /// 该玩家的复活点.
    /// </summary>
    [ReadOnly] public Vector3 revivePoint;
    
    /// <summary>
    /// 该玩家控制的单位.
    /// </summary>
    [ReadOnly] public UnitControl unit;
    
    /// <summary>
    /// 是否由玩家控制.
    /// </summary>
    public bool isControlled => id == PVPData.inst.id;
}

/// <summary>
/// 存储当前 PVP 状态数据, 以及本地和 PVP Gameplay 相关的数据.
/// </summary>
public class PVPData : MonoBehaviour
{
    public static PVPData inst;
    
    PVPData() => inst = this;
    
    [Tooltip("玩家坦克模板. 用于创建玩家坦克.")]
    public GameObject tankTemplate;
    
    [Header("状态参数")]
    
    [Tooltip("玩家坦克. 下标对应玩家编号.")]
    [ReadOnly] [SerializeField] List<PVPPlayerEntry> playerData = new List<PVPPlayerEntry>();
    
    public static PlayerIndexer players => new PlayerIndexer(inst);
    
    public int id;
    
    public bool established;
    
    BattleHUDDisplay battleHUD => BattleHUDDisplay.inst;
    
    PlayerControl myControl => PlayerControl.inst;
    
    int nextPlayerId => playerCount + 1;
    
    int nextPlayerFaction => playerCount % 2 + 1;
    
    int playerCount => playerData.Count;
    
    /// <summary>
    /// 按照自动生成的数据新建一个玩家.
    /// </summary>
    public PVPPlayerEntry CreatePlayer(NetEndPoint endPoint)
    {
        var pos = RevivePointControl.inst.Take().Value;
        var rot = Quaternion.identity;
        return CreatePlayer(endPoint, playerCount % 2 + 1, playerCount + 1, pos, rot, pos);
    }
    
    /// <summary>
    /// 按照给定数据新建一个玩家.
    /// </summary>
    public PVPPlayerEntry CreatePlayer(NetEndPoint netEndPoint, int faction, int id, Vector3 curPos, Quaternion curRot, Vector3 revivePoint)
    {
        var g = GameObject.Instantiate(tankTemplate, curPos, curRot);
        var c = g.GetComponent<UnitControl>();
        c.factionId = faction;
        var x = new PVPPlayerEntry() {
            endpoint = netEndPoint,
            id = id,
            faction = faction,
            revivePoint = revivePoint,
            unit = c
        };
        playerData.Add(x);
        return x;
    }
    
    /// <summary>
    /// 设置自己或别人控制这个单位.
    /// </summary>
    public PVPPlayerEntry SetPlaying(PVPPlayerEntry data)
    {
        this.id = data.id;
        
        // 绑定自己的控制模块.
        var control = PlayerControl.inst;
        var launches = new List<LaunchControl>();
        foreach(var x in data.unit.transform.Subtree())
        {
            if(x.TryGetComponent<TurretControl>(out var turret)) control.turret = turret;
            if(x.TryGetComponent<TankControl>(out var tank)) control.tank = tank;
            if(x.TryGetComponent<LaunchControl>(out var launcher)) launches.Add(launcher);
        }
        control.launches = launches.ToArray();
        control.unit = data.unit;
        control.working = true;
        
        // 绑定自己的相机.
        var mainCam = Camera.main;
        var camZoom = mainCam.GetComponent<CameraZoom>();
        camZoom.enabled = true;
        var camFollow = mainCam.GetComponent<CameraFollow>();
        camFollow.target = data.unit.transform.Subtree().Where(x => x.gameObject.name == "FocusPoint").ToArray()[0].gameObject;
        camFollow.working = true;
        
        // 绑定自己的UI.
        battleHUD.gameObject.SetActive(true);
        
        // 开启自己的同步操作.
        NetPlayerSync.inst.enabled = true;
        
        return data;
    }

    public struct PlayerIndexer : IReadOnlyList<PVPPlayerEntry>
    {
        public PVPData data;
        
        public PlayerIndexer(PVPData x) => data = x;
        
        public PVPPlayerEntry this[int index] => data.playerData.FirstOrDefault(x => x.id == index);
        
        public int Count => data.playerCount;

        public IEnumerator<PVPPlayerEntry> GetEnumerator() => data.playerData.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
        
        public PVPPlayerEntry me
        {
            get
            {
                for(int i = 0; i < data.playerData.Count; i++) 
                {
                    if(data.playerData[i].id == PVPData.inst.id) return data.playerData[i];
                }
                return null;
            }
        }
    }
}
