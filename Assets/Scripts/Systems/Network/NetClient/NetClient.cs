using System;

namespace Systems
{

    public abstract class NetClient : IDisposable
    {
        public abstract NetEndPoint localEndpoint { get; }
        public abstract NetEndPoint remoteEndpoint { get; }
        
        public abstract int connectionCount { get; }
        
        public abstract bool isSetup { get; }
        
        /// <summary>
        /// 发送一个协议数据包.
        /// </summary>
        public abstract void Send<T>(T data) where T : Protocol;
        
        /// <summary>
        /// 接受最多这么多个协议数据包.
        /// </summary>
        public abstract void Proceed(int maxCount);
        
        protected abstract void Dispose(bool deterministic);
        
        ~NetClient() => Dispose(false);
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
