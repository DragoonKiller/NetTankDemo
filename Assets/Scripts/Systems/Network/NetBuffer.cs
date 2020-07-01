using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Reflection;
using System.Runtime.InteropServices;
using Utils;
using System.Net.Sockets;
using System.Net;

namespace Systems
{
    /// <summary>
    /// 一个接收数据块.
    /// </summary>
    public struct ReceiveDataBlock
    {
        public readonly byte[] data;
        public ReceiveDataBlock(byte[] data) => this.data = data;
    }
    
    /// <summary>
    /// 一个发送数据块.
    /// </summary>
    public struct SendDataBlock
    {
        public readonly byte[] data;
        public SendDataBlock(byte[] data) => this.data = data;
    }
    
    /// <summary>
    /// 代表一个数据传输缓存.
    /// 主线程向其中存入或取出数据. 工作线程进行数据传输.
    /// </summary>
    public sealed class NetBuffer : IDisposable
    {
        public ConcurrentQueue<ReceiveDataBlock> receive { get; private set; } = new ConcurrentQueue<ReceiveDataBlock>();
        
        public ConcurrentQueue<SendDataBlock> send { get; private set; } = new ConcurrentQueue<SendDataBlock>();
        
        int? curLength;
        readonly ArrayList<byte> buffer = new ArrayList<byte>().Reserve(1024);
        
        public NetEndPoint sendTo { get; private set; } 
        
        public NetEndPoint receiveFrom { get; private set; } 
        
        Thread sendThread;
        Thread receiveThread;
        
        public void SubmitBytes(byte[] data)
        {
            var cdata = new byte[data.Length + 4];
            unsafe { fixed(byte* dst = cdata) fixed(byte* src = data) {
                *((Int32*)dst) = data.Length + 4; 
                Buffer.MemoryCopy(src, dst + 4, data.Length, data.Length);
            }}
            send.Enqueue(new SendDataBlock(cdata));
        }
        
        public void ReceiveByte(byte data)
        {
            buffer.Add(data);
            if(buffer.Count == 4)
            {
                curLength = BitConverter.ToInt32(buffer.array, 0);
            }
            else if(buffer.Count == curLength)
            {
                var obj = new byte[curLength.Value - 4];
                buffer.CopyTo(obj, 0, 4);
                receive.Enqueue(new ReceiveDataBlock(obj));
                curLength = null;
                buffer.Clear();
            }
        }
        
        /// <summary>
        /// 开启发送队列的监听线程.
        /// </summary>
        public void SetupSend(Socket conn, float timeoutSec, Action cancelCallback)
            => sendThread = ThreadExt.Start(() => 
                {
                    conn.SendTimeout = (int)(timeoutSec * 1000);
                    this.sendTo = new NetEndPoint((IPEndPoint)conn.RemoteEndPoint);
                    try
                    {
                        int timer = 0;
                        while(true)
                        {
                            bool succeeded = false;
                            while(this.send.TryDequeue(out var dataBlock))
                            {
                                int cnt = conn.Send(dataBlock.data);
                                succeeded = true;
                            }
                            
                            timer += 1;
                            if(succeeded) timer = 0;
                            if(timer > timeoutSec * 1000) throw new SocketException(10060); // 手动 timeout.
                            Thread.Sleep(1);
                        } 
                    }
                    catch(SocketException e)
                    {
                        if(e.ErrorCode == 10060)
                        {
                            cancelCallback();
                            return;
                        }
                        throw e;
                    }
                });
        
        /// <summary>
        /// 开启接收队列的监听线程.
        /// </summary>     
        public void SetupReceive(Socket conn, float timeoutSec, Action cancelCallback)
            => receiveThread = ThreadExt.Start(() => 
                {
                    $"{conn.LocalEndPoint} Begin Receive from {conn.RemoteEndPoint}!".Log();
                    conn.ReceiveTimeout = (int)(timeoutSec * 1000);
                    this.receiveFrom = new NetEndPoint((IPEndPoint)conn.RemoteEndPoint);
                    var buffer = new byte[32];
                    try
                    {
                        while(true)
                        {
                            int actualReceive = conn.Receive(buffer, buffer.Length, SocketFlags.None);
                            for(int i = 0; i < actualReceive; i++) this.ReceiveByte(buffer[i]);
                        }
                    }
                    catch(SocketException e)
                    {
                        if(e.ErrorCode == 10060)
                        {
                            cancelCallback();
                            return;
                        }
                        throw e;
                    }
                    catch(ObjectDisposedException) // Socket 对象被释放了,  
                    {
                        return;
                    }
                });
        
        ~NetBuffer() => Dispose();
        public void Dispose()
        {
            sendThread?.Abort();
            receiveThread?.Abort();
            GC.SuppressFinalize(this);
        }
    }
    
}
