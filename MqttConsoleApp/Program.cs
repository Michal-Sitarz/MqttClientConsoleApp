using System;
using System.Net.Mqtt;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MqttConsoleApp
{
    class Program
    {
        const string mqttBrokerIpAddress = "192.168.1.98";

        static void Main(string[] args)
        {
            
            Console.WriteLine("Starting application...");

            var mqtt = StartMqttAsync().Result;

            if (mqtt.connected)
            {
                Console.WriteLine("MQTT broker connection: OK");

                while (true)
                {
                    Publish(mqtt.client);
                }
            }
            else
            {
                Console.WriteLine("MQTT broker connection: failed");
            }

            Console.WriteLine("Application finished.");
        }

        static void Publish(IMqttClient mqttClient)
        {
            Thread.Sleep(1000);

            var now = DateTime.Now;
            var message = $"MQTT rocks {now}";

            var topic = "ConsoleTestTopic1";

            var result = PublishMessageAsync(message, topic, mqttClient).Result;
            if (result)
            {
                Console.WriteLine("Message sent to the MQTT broker succesfully.");
            }
            else
            {
                Console.WriteLine("Message to the MQTT broker not sent!!! Sorry.");
            }
        }

        static async Task<(IMqttClient client, bool connected)> StartMqttAsync()
        {
            // connect
            var client = await MqttClient.CreateAsync(mqttBrokerIpAddress, new MqttConfiguration());
            Console.WriteLine("Connecting to the MQTT broker...");

            try
            {
                var mqttSession = await client.ConnectAsync(new MqttClientCredentials(clientId: "consoleAppOne"));
                Console.WriteLine("MQTT: connected! :)");
            }
            catch (Exception)
            {
                Console.WriteLine("MQTT: not connected. Sorry :(");
                return (client, false);
            }
            
            return (client, true);
        }

        static async Task<bool> PublishMessageAsync(string message, string topic, IMqttClient client)
        {
            var mqttMessage = new MqttApplicationMessage(topic, Encoding.UTF8.GetBytes(message));

            try
            {
                await client.PublishAsync(mqttMessage, MqttQualityOfService.AtLeastOnce);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
