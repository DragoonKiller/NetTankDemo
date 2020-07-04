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
    public GameObject[] tankTemplate;
    
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
        HostOnly();
        var faction = playerCount % 2 + 1;
        var pos = RevivePointControl.inst.Take(faction).Value;
        var rot = Quaternion.identity;
        return CreatePlayer(endPoint, faction, playerCount + 1, pos, rot);
    }
    
    /// <summary>
    /// 按照给定数据新建一个玩家.
    /// </summary>
    public PVPPlayerEntry CreatePlayer(NetEndPoint netEndPoint, int faction, int id, Vector3 pos, Quaternion rot)
    {
        var c = CreateTank(pos, rot, faction);
        return CreatePlayerEntry(netEndPoint, faction, id, c);
    }
    
    /// <summary>
    /// 报告一个玩家坦克已经爆了.
    /// </summary>
    public PVPPlayerEntry PlayerDeath(int id)
    {
        var player = players[id];
     
        // 设置 UI.
        Signal.Emit(new Signals.KillCount() { faction = player.faction });
        
        // 强制设定血量为 0.
        PVPData.players[id].unit.currentStrength = 0;
        
        // 可以进行死亡响应了.
        player.unit.GetComponent<UnitDestroyControl>().enabled = true;
        
        player.unit = null;
        
        // 玩家原来的坦克不会被删除.
        return player;
    }
    
    /// <summary>
    /// 复活一个玩家.
    /// </summary>
    public PVPPlayerEntry PlayerRevive(int id, Vector3 pos)
    {
        var player = players[id];
        player.unit = CreateTank(pos, Quaternion.identity, player.faction);
        
        // 如果是自己控制的坦克, 绑定控制到上面.
        if(players.me == player) PVPData.inst.SetPlaying(player);
        
        return player;
    }
    
    /// <summary>
    /// 删除玩家.
    /// </summary>
    public void RemovePlayer(int id)
    {
        var player = players[id];
        GameObject.Destroy(player.unit.gameObject);
        playerData.RemoveAll(x => x.id == id);
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
    
    
    PVPPlayerEntry CreatePlayerEntry(NetEndPoint endpoint, int faction, int id, UnitControl control)
    {
        var x = new PVPPlayerEntry() {
            endpoint = endpoint,
            id = id,
            faction = faction,
            unit = control
        };
        playerData.Add(x);
        return x;
    }
    
    /// <summary>
    /// 创建一个坦克.
    /// </summary>
    UnitControl CreateTank(Vector3 pos, Quaternion rot, int faction)
    {
        var g = GameObject.Instantiate(tankTemplate[faction - 1], pos, rot);
        var c = g.GetComponent<UnitControl>();
        c.factionId = faction;
        return c;
    }
    
    /// <summary>
    /// 在函数最前面调用, 保证仅被主机调用.
    /// </summary>
    void HostOnly() => Debug.Assert(PVPEnv.inst.isHost);

    public struct PlayerIndexer : IReadOnlyList<PVPPlayerEntry>
    {
        public PVPData data;
        
        public PlayerIndexer(PVPData x) => data = x;
        
        public PVPPlayerEntry this[int index] => data.playerData.FirstOrDefault(x => x.id == index);
        
        public int Count => data.playerCount;
        
        public PVPPlayerEntry Find(UnitControl unit) => data.playerData.FirstOrDefault(x => x.unit == unit);

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
