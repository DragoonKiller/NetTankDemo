using UnityEngine;
using System;
using Utils;
using Systems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Net;
using System.Linq;

/// <summary>
/// PVP 专用的环境.
/// 建立或加入房间.
/// </summary>
public class PVPEnv : Env
{
    public static PVPEnv inst { get; private set; }
    PVPEnv() => inst = this;
    
    [Tooltip("想要连接到的主机的IP. 如果想要创建主机, 将该字段放空.")]
    public string targetIP;
    
    [Tooltip("想要连接到的主机的端口. 如果想要创建主机, 将该字段放空.")]
    public int targetPort;
    
    [Tooltip("游戏是否已经开始.")]
    public bool gameBegin;
    
    [Tooltip("战斗面板.")]
    public BattleHUDDisplay battleHUD;
    
    [Tooltip("玩家坦克模板. 用于创建玩家坦克.")]
    public GameObject tankTemplate;
    
    [Tooltip("显示ip和端口号的文本框.")]
    public Text endpointDisplay;
    
    [Tooltip("复活点控制器.")]
    public RevivePointControl revivePoints;
    
    /// <summary>
    /// 玩家坦克. 下标对应玩家编号.
    /// </summary>
    public List<GameObject> player;
    
    /// <summary>
    /// 需要同步的对象.
    /// </summary>
    public HashSet<NetIdentity> netObjects;
    
    /// <summary>
    /// 客户端连接. 如果为主机, 该字段存储 Host 类. 如果为客户端, 该字段存储 Client 类. 
    /// </summary>
    NetClient client;
    
    StateMachine setupStm;
    StateMachine netDataProceedStm;
    
    void Start()
    {
        if(!Application.isEditor) Debug.LogError("Open Console!");
        Signal<Signals.Launch>.Listen(LaunchControl.LaunchCallback);
        Signal<Signals.Hit>.Listen(UnitControl.HitCallback);
        setupStm = StateMachine.Register(new SetupStateMachine() { env = this });
        netDataProceedStm = StateMachine.Register(new NetDataProceedStateMachine() { env = this });
    }
    
    void Update()
    {    
        // 全局状态机执行流程.
        StateMachine.Run();
        
        SetCursorLocked();
    }
    
    
    void OnDestroy()
    {
        Signal<Signals.Launch>.Remove(LaunchControl.LaunchCallback);
        Signal<Signals.Hit>.Remove(UnitControl.HitCallback);
        StateMachine.Remove(setupStm.tag);
        StateMachine.Remove(netDataProceedStm.tag);
    }
    
    /// <summary>
    /// 强制设置光标.
    /// </summary>
    void SetCursorLocked()
    {
        #if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.V)) ExCursor.cursorLocked = !ExCursor.cursorLocked;
        #endif
    }
    
    
    void CreateTankLocal()
    {
        // 创建自己的坦克.
        var tank = GameObject.Instantiate(tankTemplate, revivePoints.Take().Value, Quaternion.identity);
        var id = tank.GetComponent<NetIdentity>();
        id.netId = 1;
        
        var unit = tank.GetComponent<UnitControl>();
        unit.factionId = 1;
        
        // 绑定自己的相机.
        var camFollow = Camera.main.gameObject.GetComponent<CameraFollow>();
        string.Join(",", tank.transform.AllChildren().Select(x => x.name)).Log();
        camFollow.target = tank.transform.AllChildren().Where(x => x.gameObject.name == "FocusPoint").ToArray()[0].gameObject;
        camFollow.enabled = true;
        
        var camZoom = Camera.main.gameObject.GetComponent<CameraZoom>();
        camZoom.enabled = true;
        
        // 绑定自己的控制模块.
        var control = tank.GetComponent<PlayerControl>();
        control.enabled = true;
        
        // 绑定自己的UI.
        battleHUD.player = control;
        battleHUD.prediction = tank.GetComponent<PredictionControl>();
        battleHUD.gameObject.SetActive(true);
        
        // 把坦克添加到列表中.
        player.Add(tank);
        
    }

    class NetDataProceedStateMachine : StateMachine
    {
        public PVPEnv env;
        
        public override IEnumerable<Transfer> Step()
        {
            while(env.client == null) yield return Pass();
            while(true)
            {
                yield return Pass();
                // 每帧处理 500 个数据包.
                env.client.Proceed(500);
            }
        }
    }

    class SetupStateMachine : StateMachine
    {
        public PVPEnv env;

        public override IEnumerable<Transfer> Step()
        {
            if(env.targetIP.IsNullOrEmtpy()) return SetupHost();
            else return SetupClient();
        }
        
        IEnumerable<Transfer> SetupHost()
        {    
            env.client = new TcpHost();
            while(!env.client.isSetup) yield return Pass();
            env.endpointDisplay.text = $"主机 {env.client.localEndpoint}";
        }
        
        IEnumerable<Transfer> SetupClient()
        {    
            env.client = new TcpClient(new NetEndPoint(env.targetIP, env.targetPort));
            while(!env.client.isSetup) yield return Pass();
            env.endpointDisplay.text = $"连接到 {env.client.remoteEndpoint}";
            env.client.Send(new JoinProtocol());
        }
    }
    
    
}
