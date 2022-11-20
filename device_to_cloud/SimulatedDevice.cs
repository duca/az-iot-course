// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// This application uses the Azure IoT Hub device SDK for .NET
// For samples see: https://github.com/Azure/azure-iot-sdk-csharp/tree/master/iothub/device/samples

using System;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

namespace simulatedDevice
{
    class SimulatedDevice
    {
        private static DeviceClient s_deviceClient;

        // The device connection string to authenticate the device with your IoT hub.
        private const string s_connectionString = "HostName=elopes-iot.azure-devices.net;DeviceId=iotdev1;SharedAccessKey=OEAHX43oQfyqFO/mGbQ794XNB2/HF84lwWAPmYwFqwQ=";
        private const string s_serviceConnString = "HostName=elopes-iot.azure-devices.net;SharedAccessKeyName=service;SharedAccessKey=u9m0gB50HbLTZYwUQqp+pjeOmOEeZLYllJl/TuxUpGs=";

        private static async void ReceiveC2dAsync()
        {
            Console.WriteLine("\nReceiving cloud to device messages from service");
            while (true)
            {
                Microsoft.Azure.Devices.Client.Message receivedMessage = await s_deviceClient.ReceiveAsync();
                if (receivedMessage == null) continue;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Received message: {0}",
                Encoding.ASCII.GetString(receivedMessage.GetBytes()));
                Console.ResetColor();

                await s_deviceClient.CompleteAsync(receivedMessage);
            }
        }

        // Async method to send simulated tellemetry
        private static async void SendDeviceToCloudMessagesAsync()
        {
            // Initial telemetry values
            while (true)
            {
                var messageString = "{'status': 'success', 'message':'success retrieving data'}";
                var message = new Microsoft.Azure.Devices.Client.Message(Encoding.ASCII.GetBytes(messageString));
                message.ContentType = "application/json";
                message.ContentEncoding = "utf-8";

                // // Add a custom application property to the message.
                // // An IoT hub can filter on these properties without access to the message body.
                // message.Properties.Add("temperatureAlert", (currentTemperature > 30) ? "true" : "false");

                // Send the tlemetry message
                await s_deviceClient.SendEventAsync(message).ConfigureAwait(false);
                Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);

                await Task.Delay(1000).ConfigureAwait(false);
            }
        }

        private async static Task SendCloudToDeviceMessageAsync()
        {
            ServiceClient serviceClient = ServiceClient.CreateFromConnectionString(s_serviceConnString);
            string targetDevice = "iotdev1";
            var commandMessage = new Microsoft.Azure.Devices.Message(Encoding.ASCII.GetBytes("this is my c2d message."));
            await serviceClient.SendAsync(targetDevice, commandMessage);
        }

        static async void HandleDesiredPropertiesChange()
        {
            await s_deviceClient.SetDesiredPropertyUpdateCallbackAsync(async (desired, ctx) =>
            {
                Newtonsoft.Json.Linq.JValue fpsJson = desired["FPS"];
                var fps = fpsJson.Value;

                Console.WriteLine("Received desired FPS: {0}", fps);

            }, null);
        }

        private static void Main()
        {
            Console.WriteLine("IoT Hub Quickstarts - Simulated device. Ctrl-C to exit.\n");

            // Connect to the IoT hub using the MQTT protocol
            s_deviceClient = DeviceClient.CreateFromConnectionString(s_connectionString, Microsoft.Azure.Devices.Client.TransportType.Mqtt);
            //SendDeviceToCloudMessagesAsync();
            //ReceiveC2dAsync();
            HandleDesiredPropertiesChange();
            Console.ReadLine();
        }
    }
}
