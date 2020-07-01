using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Utils;

namespace Systems
{
    
    public sealed class TcpHost : NetClient, IDisposable
    {
        readonly Socket socket;
        
        /// <summary>
        /// 数据传输缓冲.
        /// </summary>
        public ConcurrentDictionary<NetEndPoint, NetBuffer> connections { get; private set; } = new ConcurrentDictionary<NetEndPoint, NetBuffer>();
        
        public override NetEndPoint localEndpoint => new NetEndPoint((IPEndPoint)socket.LocalEndPoint);
        public override NetEndPoint remoteEndpoint => new NetEndPoint((IPEndPoint)socket.RemoteEndPoint);
        
        public bool established { get; private set; }
        
        public override bool isSetup => established;
        
        /// <summary>
        /// 已经建立的连接数.
        /// </summary>
        public override int connectionCount => connections.Count;
        
        Thread workingThread;
        
        public bool disposed { get; private set; }
        
        public TcpHost() : this(Dns.GetHostEntry(Dns.GetHostName()).AddressList[0]) { }
        public TcpHost(IPAddress addr)
        {
            socket = new Socket(addr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(addr, 0));
            socket.Listen(40);
            established = true;
            
            // 根据 https://stackoverflow.com/questions/1337073/net-c-sharp-socket-concurrency-issues
            // Socket 对象是完全线程安全的. 可以异步操作.
            workingThread = ThreadExt.Start(() => 
            {
                try
                {
                    while(true)
                    {
                        Socket conn = socket.Accept();
                        var targetEndpoint = new NetEndPoint((IPEndPoint)conn.RemoteEndPoint);
                        
                        $"Connection {localEndpoint} => {targetEndpoint} established.".Log();
                        
                        var conndata = new NetBuffer();
                        while(!connections.TryAdd(targetEndpoint, conndata)) Thread.Sleep(TimeSpan.FromMilliseconds(1));
                        
                        // 数据接收线程.
                        conndata.SetupReceive(conn, 5, () => {
                            $"Connection {localEndpoint} => {targetEndpoint} Canceled.".Log();
                            while(!connections.TryRemove(targetEndpoint, out var _)) Thread.Sleep(TimeSpan.FromMilliseconds(1));
                            conn.Close();
                        });
                        
                        // 数据发送线程.
                        conndata.SetupSend(conn, 5, () => {
                            // 放空.
                            // 关闭连接的任务由数据接收线程关闭回调来完成.
                        });
                    }
                }
                catch(SocketException e)
                {
                    if(e.ErrorCode == 10054) return; // Socket 被远程关闭.
                    if(e.ErrorCode == 10060) return; // 接收数据超时.
                    throw e;
                }
            });
            
        }
        
        protected override void Dispose(bool deterministic)
        {
            if(!disposed)
            {
                workingThread.Abort();
                foreach(var i in connections.Values) i.Dispose();
                socket.Close();
                disposed = true;
            }
        }

        /// <summary>
        /// 向所有已经连接的客户端发送协议数据.
        /// </summary>
        public override void Send<T>(T data)
        {
            var config = new HostSendConfig();
            var bytes = data.SerializeToBytes();
            var endpointLists = new List<NetEndPoint>();
            foreach(var conn in connections) endpointLists.Add(conn.Key);
            data.BeforeSendByHost(this, endpointLists, ref config);
            foreach(var conn in connections)
            {
                var ip = conn.Key;
                var buffer = conn.Value;
                conn.Value.SubmitBytes(bytes);
            }
        }
        
        /// <summary>
        /// 处理接收到的数据.
        /// 会对每一个客户端的连接分别处理一次.
        /// 行为由接收到的数据对应的协议类指定.
        /// </summary>
        public override void Proceed(int maxCount)
        {
            foreach(var conn in connections)
            {
                var remoteEndpoint = conn.Key;
                var buffer = conn.Value;
                int curCnt = 0;
                while(!buffer.receive.IsEmpty && curCnt < maxCount)
                {
                    if(buffer.receive.TryDequeue(out var dataBlock))
                    {
                        curCnt += 1;
                        var protocol = dataBlock.data.Deserialize();
                        protocol.AfterReceiveByHost(this, remoteEndpoint);
                    }
                }
            }
        }
    }
}
