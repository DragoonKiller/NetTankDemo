using System;
using System.Net;

namespace Systems
{
    /// <summary>
    /// 代表一个网络端点.
    /// 是主机和客户端的唯一身份凭据.
    /// </summary>
    public struct NetEndPoint : IEquatable<NetEndPoint>, IComparable<NetEndPoint>
    {
        public readonly string ip;
        public readonly int port;
        public NetEndPoint(string ip, int port) => (this.ip, this.port) = (ip, port);
        public NetEndPoint(NetEndPoint x) => (this.ip, this.port) = (x.ip, x.port);
        public NetEndPoint(IPEndPoint endpoint) => (this.ip, this.port) = (endpoint.Address.MapToIPv6().ToString(), endpoint.Port);
        public IPEndPoint iPEndPoint => new IPEndPoint(IPAddress.Parse(ip), port);
        
        public int CompareTo(NetEndPoint v)
        {
            if(ip == v.ip) return port - v.port;
            return ip.CompareTo(v.ip);
        }

        public bool Equals(NetEndPoint v) => ip.CompareTo(v.ip) == 0 && port == v.port;
        public override string ToString() => ip + ":" + port;
    }
}
