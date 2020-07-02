using System;
using System.Linq;
using System.Runtime;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Reflection;
using System.Net;

namespace Systems
{
    /// <summary>
    /// 
    /// </summary>
    public static class ProtocolUtils
    {
        static Dictionary<string, Type> types = new Dictionary<string, Type>();
        
        static ProtocolUtils()
        {
            // 获取所有实际协议类, 记录它们的类型名称.
            // 不支持动态加载 Assembly 的协议类.
            foreach(var type in 
                Assembly.GetAssembly(typeof(ProtocolUtils)).GetTypes()
                .Where(x => !x.IsGenericType && !x.IsAbstract && x.IsSubclassOf(typeof(Protocol))))
            {
                types.Add(type.Name, type);
            }
        }
        
        const string typeTag = "||";
        
        public static string SerializeToString(this Protocol x) => x.GetType().Name + typeTag + JsonUtility.ToJson(x);
        
        public static byte[] SerializeToBytes(this Protocol x) => Encoding.UTF8.GetBytes(x.SerializeToString());
        
        public static Protocol Deserialize(this byte[] data)
        {
            var str = Encoding.UTF8.GetString(data);
            return Deserialize(str);
        }
        
        public static Protocol Deserialize(this string str)
        {
            var typeTagIndex = str.IndexOf(typeTag, 0);
            if(typeTagIndex == -1) throw new Exception("Protocol syntax not correct.");
            var typeStr = str.Substring(0, typeTagIndex);
            var dataStr = str.Substring(typeTagIndex + typeTag.Length);
            Protocol res = (Protocol)Activator.CreateInstance(ProtocolUtils.types[typeStr]);
            JsonUtility.FromJsonOverwrite(dataStr, res);
            return res;
        }
    }
    
    /// <summary>
    /// 基础协议类.
    /// 数据传输时, 协议类的对象会被序列化.
    /// </summary>
    [Serializable]
    public abstract class Protocol
    {
        public abstract void BeforeSendByClient(NetClient client, ref ClientSendConfig cfg);
        public abstract void BeforeSendByHost(NetClient host, List<NetEndPoint> endpointList, ref HostSendConfig cfg);
        public abstract void AfterReceiveByClient(NetClient client, NetEndPoint from);
        public abstract void AfterReceiveByHost(NetClient host, NetEndPoint from);
    }
    
    /// <summary>
    /// 发送数据时的配置信息.
    /// </summary>
    public struct HostSendConfig
    {
        
    }
    
    /// <summary>
    /// 发送数据时的配置信息.
    /// </summary>
    public struct ClientSendConfig
    {
        
    }
}
