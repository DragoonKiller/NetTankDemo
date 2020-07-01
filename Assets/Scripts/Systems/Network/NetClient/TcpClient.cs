using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Utils;

namespace Systems
{
    public sealed class TcpClient : NetClient, IDisposable
    {
        readonly Socket socket;
        
        public NetworkStream stream { get; private set; }
        
        /// <summary>
        /// 数据传输缓冲.
        /// </summary>
        public readonly NetBuffer buffer = new NetBuffer();
        
        public override NetEndPoint localEndpoint => new NetEndPoint((IPEndPoint)socket.LocalEndPoint);
        public override NetEndPoint remoteEndpoint => new NetEndPoint((IPEndPoint)socket.RemoteEndPoint);
        
        public override bool isSetup => connected;
        
        /// <summary>
        /// 是否连上了主机.
        /// </summary>
        public bool connected { get; private set; }
        
        /// <summary>
        /// 已经建立的连接数.
        /// </summary>
        public override int connectionCount => connected ? 0 : 1;
        
        Thread workingThread;
        
        public bool disposed { get; private set; }
        
        public TcpClient(NetEndPoint endpoint)
        {
            socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            workingThread = ThreadExt.Start(() => 
            {
                $"Connecting... {endpoint}.".Log();
                socket.Connect(endpoint.iPEndPoint);
                connected = true;
                
                $"Connection {socket.LocalEndPoint} => {socket.RemoteEndPoint} established.".Log();
                
                // 数据接收线程.
                buffer.SetupReceive(socket, 5, () => {
                    $"Connection {socket.LocalEndPoint} => {socket.RemoteEndPoint} Canceled.".Log();
                    socket.Close();
                });
                
                // 数据发送线程.
                buffer.SetupSend(socket, 5, () => {
                    // 放空.
                    // 关闭连接的任务由数据接收线程关闭回调来完成.
                });
            });
        }
        
        
        protected override void Dispose(bool deterministic)
        {
            if(!disposed)
            {
                workingThread?.Abort();
                socket.Close();
                buffer.Dispose();
                disposed = true;
            }
        }

        /// <summary>
        /// 发送协议数据.
        /// </summary>
        public override void Send<T>(T data)
        {
            var config = new ClientSendConfig();
            data.BeforeSendByClient(this, ref config);
            var bytes = data.SerializeToBytes();
            buffer.SubmitBytes(bytes);
        }
        
        /// <summary>
        /// 处理接收到的数据.
        /// 行为由接收到的数据对应的协议类指定.
        /// </summary>
        public override void Proceed(int maxCount)
        {
            int curCnt = 0;
            while(!buffer.receive.IsEmpty && curCnt < maxCount)
            {
                if(buffer.receive.TryDequeue(out var dataBlock))
                {
                    curCnt += 1;
                    var protocol = dataBlock.data.Deserialize();
                    protocol.AfterReceiveByClient(this, remoteEndpoint);
                }
            }
        }
    }
    
}
