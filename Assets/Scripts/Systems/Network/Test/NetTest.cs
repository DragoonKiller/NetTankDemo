using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using Utils;

namespace Systems.Test
{
    public class NetTest : MonoBehaviour
    {

        class TestProtocol : Protocol
        {
            static int cnt = 0;
            
            public int id;
            public string customInfo;
            
            
            public override void BeforeSendByClient(NetClient client, ref ClientSendConfig _)
            {
                cnt += 1;
                id = cnt;
            }
            
            public override void BeforeSendByHost(NetClient host, List<NetEndPoint> to, ref HostSendConfig _)
            {
                
            }
            public override void AfterReceiveByClient(NetClient client, NetEndPoint from)
            {
                $"Client receive! {id} {customInfo}".Log();
            }
            
            public override void AfterReceiveByHost(NetClient host, NetEndPoint from)
            {
                $"Host receive! {id} : {customInfo}".Log();
                host.Send(new TestProtocol() { customInfo = "string from host" });
            }
        }

        NetClient host;
        NetClient client;
        
        void Start()
        {
            $"Test On".Log();
            host = new TcpHost();
            client = new TcpClient(host.localEndpoint);
            $"host local {host.localEndpoint}".Log();
            while(!(client as TcpClient).connected) System.Threading.Thread.Sleep(1);
            $"client local {client.localEndpoint}".Log();
            $"client remote {client.remoteEndpoint}".Log();
            client.Send(new TestProtocol());
            client.Send(new TestProtocol(){ customInfo = "second time!" });
        }
        
        void Update()
        {
            host.Proceed(10);
            client.Proceed(10);
        }
        
        void OnDestroy()
        {
            host?.Dispose();
            client?.Dispose();
        }
    }
}
