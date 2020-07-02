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
/// 控制 PVP 常规运行时, 以及初始化方面的流程控制..
/// 控制建立或加入房间.
/// </summary>
[RequireComponent(typeof(PVPData))]
public class PVPEnv : Env
{
    public static PVPEnv inst { get; private set; }
    PVPEnv() => inst = this;
    
    [Tooltip("想要连接到的主机的IP. 如果想要创建主机, 将该字段放空.")]
    public string targetIP;
    
    [Tooltip("想要连接到的主机的端口. 如果想要创建主机, 将该字段放空.")]
    public int targetPort;
    
    [Tooltip("显示ip和端口号的文本框.")]
    public Text endpointDisplay;
    
    /// <summary>
    /// 客户端连接. 如果为主机, 该字段存储 Host 类. 如果为客户端, 该字段存储 Client 类. 
    /// </summary>
    public NetClient client { get; private set; }
    
    public bool isHost => client is TcpHost;
    
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
            var x = PVPData.inst.CreatePlayer(env.client.localEndpoint);
            PVPData.inst.SetPlaying(x);
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
