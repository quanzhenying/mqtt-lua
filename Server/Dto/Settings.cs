using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;
using static System.Net.Mime.MediaTypeNames;

namespace Server.Dto
{
    public class Settings
    {
        public bool DefaultEndpoint { get; set; }
        public int DefaultEndpointPort { get; set; }
        public IPAddress DefaultEndpointBoundIPAddress { get; set; } = IPAddress.Any;

        public bool EncryptedEndpoint { get; set; }
        public int EncryptedEndpointPort { get; set; }
        public IPAddress EncryptedEndpointBoundIPAddress { get; set; } = IPAddress.Any;
        public string EncryptionCertificateKeyPem { get; set; } = string.Empty;
        public string EncryptionCertificateCertPem { get; set; } = string.Empty;
        public SslProtocols EncryptionSslProtocol { get; set; } = SslProtocols.Tls12;

        public int TcpKeepAliveTime { get; set; } = 30;
        public int TcpKeepAliveRetryCount { get; set; } = 3;
        public int TcpKeepAliveInterval { get; set; } = 10;

        public bool EnableSessions { get; set; } = true;
        public int MaxPendingMessagesPerClient { get; set; } = 1000000;
        public int MaxConnections { get; set; } = 100000;

        public string Event_Started { get; set; } = string.Empty;
        public string Event_Stoped { get; set; } = string.Empty;

        public string Event_ValidatingConnection { get; set; } = string.Empty;
        public string Event_ClientSubscribedTopic { get; set; } = string.Empty;
        public string Event_ClientUnSubscribedTopic { get; set; } = string.Empty;

        public string Event_ClientAcknowledgedPublishPacket { get; set; } = string.Empty;
        public string Event_ClientConnected { get; set; } = string.Empty;
        public string Event_ClientDisconnected { get; set; } = string.Empty;

        public string ClusterNode { get; set; } = string.Empty;
        public string[] ClusterNodes { get; set; } = [];
        public string ClusterUserName { get; set;} = "cluster_default";
        public string ClusterPassword { get; set; } = "cluster_default";
    }
}

