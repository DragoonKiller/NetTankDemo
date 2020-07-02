using System;
using System.Net;

namespace Systems
{
    /// <summary>
    /// 代表一个网络端点.
    /// 是主机和客户端的唯一身份凭据.
    /// </summary>
    [Serializable]
    public struct NetEndPoint : IEquatable<NetEndPoint>, IComparable<NetEndPoint>
    {
        public string ip;
        public int port;
        public NetEndPoint(string ip, int port) => (this.ip, this.port) = (ip, port);
        public NetEndPoint(NetEndPoint x) => (this.ip, this.port) = (x.ip, x.port);
        public NetEndPoint(IPEndPoint endpoint) => (this.ip, this.port) = (endpoint.Address.MapToIPv6().ToString(), endpoint.Port);
        public IPEndPoint iPEndPoint => new IPEndPoint(IPAddress.Parse(ip), port);
        
        public int CompareTo(NetEndPoint v)
        {
            if(ip == v.ip) return port - v.port;
            return ip.CompareTo(v.ip);
        }

        public static bool operator==(NetEndPoint a, NetEndPoint b) => a.ip == b.ip && a.port == b.port;
        public static bool operator!=(NetEndPoint a, NetEndPoint b) => a.ip != b.ip || a.port != b.port;
        public static bool operator<(NetEndPoint a, NetEndPoint b) => a.ip == b.ip ? a.port < b.port : a.ip.CompareTo(b.ip) < 0;
        public static bool operator>(NetEndPoint a, NetEndPoint b) => a.ip == b.ip ? a.port > b.port : a.ip.CompareTo(b.ip) > 0;
        public static bool operator<=(NetEndPoint a, NetEndPoint b) => a.ip == b.ip ? a.port <= b.port : a.ip.CompareTo(b.ip) < 0;
        public static bool operator>=(NetEndPoint a, NetEndPoint b) => a.ip == b.ip ? a.port >= b.port : a.ip.CompareTo(b.ip) > 0;
        public bool Equals(NetEndPoint v) => ip.CompareTo(v.ip) == 0 && port == v.port;
        public override bool Equals(object v) => v is NetEndPoint q ? q == this : false;
        public override int GetHashCode() => $"{ip}{port}".GetHashCode();
        public override string ToString() => ip + ":" + port;
    }
}
