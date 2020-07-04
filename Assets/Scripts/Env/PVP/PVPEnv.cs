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
    
    [Tooltip("玩家名称.")]
    public string playerName;
    
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
        setupStm = StateMachine.Register(new SetupStateMachine() { env = this });
        netDataProceedStm = StateMachine.Register(new NetDataProceedStateMachine() { env = this });
    }
    
    void Update()
    {    
        // 全局状态机执行流程.
        StateMachine.Run();
        
        SetCursorLocked();
        CutoffConnection();
    }
    
    
    void OnDestroy()
    {
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
    
    /// <summary>
    /// 给予客户端一个主动断开连接的方式.
    /// </summary>
    void CutoffConnection()
    {
        if(Input.GetKeyDown(KeyCode.Delete))
        {
            client.Dispose();
            $"Force client offline!".Log();
        }
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
            var client = new TcpHost();
            env.client = client;
            while(!client.isSetup) yield return Pass();
            env.endpointDisplay.text = $"主机 {client.localEndpoint}".WithColor(Color.yellow);
            var x = PVPData.inst.CreatePlayer(client.localEndpoint);
            x.unit.gameObject.name = env.playerName;
            PVPData.inst.SetPlaying(x);
            while(true)
            {
                yield return Pass();
                SyncRemovePlayerEntry(client);
            }
        }
        
        IEnumerable<Transfer> SetupClient()
        {    
            env.client = new TcpClient(new NetEndPoint(env.targetIP, env.targetPort));
            while(!env.client.isSetup) yield return Pass();
            env.endpointDisplay.text = $"连接到 {env.client.remoteEndpoint}";
            env.client.Send(new JoinProtocol());
        }
        
        /// <summary>
        /// 定期检查并清除掉线的玩家.
        /// </summary>
        void SyncRemovePlayerEntry(TcpHost client)
        {
            var players = PVPData.players;
            List<int> removeList = null;
            foreach(var p in players)
            {
                bool valid = p.id == PVPData.inst.id;
                foreach(var c in client.connections.Keys) valid |= c == p.endpoint;
                if(valid) continue;
                if(removeList == null) removeList = new List<int>();
                removeList.Add(p.id);
                $"Player Exit {p.id}".Log();
            }
            if(removeList != null) foreach(var rm in removeList) PVPData.inst.RemovePlayer(rm);
        }
    
    }
    
    
}
