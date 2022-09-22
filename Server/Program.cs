using MQTTnet;
using MQTTnet.Server;
using System;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            _ = Run_Minimal_Server();
        }

        public static async Task Run_Minimal_Server()
        {
            /*
             * This sample starts a simple MQTT server which will accept any TCP connection.
             */

            var mqttFactory = new MqttFactory();

            // The port for the default endpoint is 1883.
            // The default endpoint is NOT encrypted!
            // Use the builder classes where possible.
            IMqttServerOptions mqttServerOptions = new MqttServerOptionsBuilder().WithDefaultEndpoint().Build();

            // The port can be changed using the following API (not used in this example).
            // new MqttServerOptionsBuilder()
            //     .WithDefaultEndpoint()
            //     .WithDefaultEndpointPort(1234)
            //     .Build();

            using (var mqttServer = mqttFactory.CreateMqttServer())
            {
                await mqttServer.StartAsync(mqttServerOptions);

                Console.WriteLine("Press Enter to exit.");
                Console.ReadLine();

                // Stop and dispose the MQTT server if it is no longer needed!
                await mqttServer.StopAsync();
            }
        }
    }
   
}
