
using MQTTnet;
using MQTTnet.Server;
using Server.Helpers;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

var settings = SettingsHelper.Load();

var builder = new MqttServerOptionsBuilder();
if (settings.DefaultEndpoint)
{
    builder = builder.WithDefaultEndpoint()
               .WithDefaultEndpointPort(settings.DefaultEndpointPort)
               .WithDefaultEndpointBoundIPAddress(settings.DefaultEndpointBoundIPAddress);
}
if (settings.EncryptedEndpoint)
{

    builder = builder.WithEncryptedEndpoint()
               .WithEncryptedEndpointPort(settings.EncryptedEndpointPort)
               .WithEncryptedEndpointBoundIPAddress(settings.EncryptedEndpointBoundIPAddress)
               .WithEncryptionSslProtocol(settings.EncryptionSslProtocol);

    if(!string.IsNullOrEmpty(settings.EncryptionCertificateKeyPem) && !string.IsNullOrEmpty(settings.EncryptionCertificateCertPem))
    {
        var sc = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cert", "server-cert.pem");
        var sk = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cert", "server-key.pem");
        var serverCert = X509Certificate2.CreateFromPemFile(sc, sk);
        builder = builder.WithEncryptionCertificate(serverCert.Export(X509ContentType.Pfx));
    }
}

builder = builder
               .WithTcpKeepAliveTime(settings.TcpKeepAliveTime)
               .WithTcpKeepAliveRetryCount(settings.TcpKeepAliveRetryCount)
               .WithTcpKeepAliveInterval(settings.TcpKeepAliveInterval)
               .WithPersistentSessions(settings.EnableSessions) //持续会话 支持QoS 2实现掉线缓冲（并没有持久化）
               .WithMaxPendingMessagesPerClient(settings.MaxPendingMessagesPerClient) //每终端主题最大缓冲
               .WithConnectionBacklog(settings.MaxConnections) //单机最大连接数
               ;

var options = builder.Build();


var factory = new MqttFactory();
using var server = factory.CreateMqttServer(options, new MyLogger());

server.ValidatingConnectionAsync += (async arg =>
{
    var sessions = server.ServerSessionItems; 
    if (arg.UserName == "IMA_OAUTH_ACCESS_TOKEN")
    {
        var token = arg.Password;
        await Task.Delay(1000);
    }
});
var cluster = new ClusterHelper();
server.ClientAcknowledgedPublishPacketAsync += (async arg =>
{
    if (cluster != null && cluster.Enable)
        await cluster.PublicAsync(arg.PublishPacket.Topic, arg.PublishPacket.CorrelationData, arg.PublishPacket.Retain);
});

await server.StartAsync();

//集群转发功能
var otherNodes = settings.ClusterNodes.Where(m => m != settings.ClusterNode).ToArray();
if (otherNodes.Length > 0)
{
    cluster.Init(settings);
}

Console.ReadLine();
Console.ReadLine();
Console.ReadLine();
