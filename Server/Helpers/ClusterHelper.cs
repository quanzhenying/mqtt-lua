using MQTTnet;
using MQTTnet.Client;
using Server.Dto;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Server.Helpers
{
    public class ClusterHelper
    {
        public List<IMqttClient> clients = [];

        public async void Init(Settings settings)
        {
            var otherNodes = settings.ClusterNodes.Where(m => m != settings.ClusterNode).ToArray();
            foreach (string node in otherNodes)
            {
                var client = new MqttFactory().CreateMqttClient();
                var optionsBuilder = new MqttClientOptionsBuilder()
                    .WithCredentials(settings.ClusterUserName, settings.ClusterPassword);

                var options = optionsBuilder.WithTcpServer(node, settings.DefaultEndpointPort)
                             .WithClientId(Guid.NewGuid().ToString())
                             .WithCleanSession(true)
                             .Build();
                client.ConnectedAsync += async e =>
                {
                    Console.WriteLine($"{DateTime.Now},集群节点连接成功,{node},{settings.DefaultEndpointPort}");
                };
                client.DisconnectedAsync += async e =>
                {
                    Console.WriteLine($"{DateTime.Now},集群节点掉线,{node},{e.Reason}");
                };

                var connResult = await client.ConnectAsync(options);
                clients.Add(client);
            }
            if(otherNodes.Length > 0)
            {
                Enable = true;
            }

            //30秒检查重连机制
            var timer = new System.Timers.Timer(30 * 1000);
            timer.Elapsed += async (a, b) =>
            {
                foreach (var client in clients)
                {
                    if (!client.IsConnected)
                    {
                       await client.ReconnectAsync();
                    }
                }
                if(clients.All(m => m.IsConnected) && buffer.Count > 0)
                {
                    Console.WriteLine($"{DateTime.Now},转发集群重连补发,待补发{buffer.Count}");
                    lock (timer)
                    {
                        while (buffer.TryDequeue(out var msg))
                        {
                            try
                            {
                                foreach (var node in clients)
                                {
                                    node.PublishAsync(msg);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"{DateTime.Now},转发集群节点失败,{ex}");
                                buffer.Enqueue(msg);
                                break;
                            }
                        };
                    }
                }
            };
            timer.Start();
        }

        public bool Enable { get; set; } = false;
        private readonly ConcurrentQueue<MqttApplicationMessage> buffer = new();
        public async Task PublicAsync(string topic, byte[] data, bool retain)
        {
            var msg = new MqttApplicationMessageBuilder()
                                           .WithTopic(topic)
                                           .WithPayload(data)
                                           .WithRetainFlag(retain)
                                           .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce)
                                           .Build();

            if (clients.Any(m => !m.IsConnected))
            {
                Console.WriteLine($"{DateTime.Now},部分集群节点掉线,稍后补传");
                buffer.Enqueue(msg);
                return;
            }

            try
            {
                foreach (var node in clients)
                {
                    await node.PublishAsync(msg);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"{DateTime.Now},转发集群节点失败,{ex}");
                buffer.Enqueue(msg);
            }
        }
    }
}
