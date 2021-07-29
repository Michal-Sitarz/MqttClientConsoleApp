using System;
using System.Collections.Generic;
using System.Net.Mqtt;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MqttConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            const string _mqttBroker = "192.168.1.98";
            List<string> topics = new List<string> { "greenhouse/conditions", "mosTst1" };

            Console.WriteLine("Starting application...");

            var mqtt = StartMqttClientAsync(_mqttBroker).Result;

            if (mqtt.connected)
            {
                Console.WriteLine("MQTT broker connection: OK");

                // subscribe to all topics
                var subscribed = SubscribeToTopics(topics, mqtt.client).Result;
                if (subscribed)
                {
                    // recieve messages from topics
                    mqtt.client
                        .MessageStream
                        .Subscribe(msg => MessagesMonitoring(msg.Topic, Encoding.UTF8.GetString(msg.Payload)));
                    //   ^ observable collection, triggered whenever new message is recieved from any topic
                }

                // uncomment to use the publishing messages in a constant loop (i.e. for testing)
                //while (true)
                //{
                //    Publish(mqtt.client);
                //}
            }
            else
            {
                Console.WriteLine("MQTT broker connection: failed attempt.");
            }

            Console.WriteLine("Application finished.");
        }

        static void MessagesMonitoring(string topic, string message)
        {
            // logic what to do with each type (topic) of a message
            // i.e. if this topic, then this...

            // for now, for all new messages -> display in console
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("Message: ");
            Console.ResetColor();
            Console.Write(message);
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write($"[@ Topic: {topic}]");
            Console.ResetColor();
        }

        static void Publish(IMqttClient mqttClient)
        {
            // publish after each second
            Thread.Sleep(1000);

            // testing values
            var now = DateTime.Now;
            var message = $"MQTT rocks! {now}";
            var topic = "ConsoleTestTopic1";

            // publish message
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

        static async Task<(IMqttClient client, bool connected)> StartMqttClientAsync(string brokerIpAddress)
        {
            // connect
            var client = await MqttClient.CreateAsync(brokerIpAddress, new MqttConfiguration());
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

        static async Task<bool> SubscribeToTopics(List<string> topics, IMqttClient client)
        {
            try
            {
                foreach (var t in topics)
                {
                    await client.SubscribeAsync(t, MqttQualityOfService.ExactlyOnce);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
