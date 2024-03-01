using MQTTnet.Diagnostics;

namespace Server.Helpers
{
    public class MyLogger : IMqttNetLogger
    {
        public bool IsEnabled => true;

        public void Publish(MqttNetLogLevel logLevel, string source, string message, object[] parameters, Exception exception)
        {
            Console.WriteLine(message, parameters);
        }
    }
}