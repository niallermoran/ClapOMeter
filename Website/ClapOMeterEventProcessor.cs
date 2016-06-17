using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ServiceBus.Messaging;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using Newtonsoft.Json;
using Website.Models;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices;

namespace Website
{
    /// <summary>
    /// This event processor is used for handling device to cloud messages sent to the IoTHub
    /// </summary>
    public class ClapOMeterEventProcessor : IEventProcessor
    {
        private int maxRecordsToStore = 30;
        public static SoundDataModel SoundDataMax;
        public static SoundDataModel SoundDataMin;
        public static SoundDataModel SoundDataLatest;
        public static double SoundDataAVG;
        private static double soundDataTotal;
        private static int soundDataCount = 0;
        private static DateTime? startTime;
        public static TimeSpan SoundCollectionPeriod;

        /// <summary>
        /// Store the most recent data in memory
        /// </summary>
        public static List<SoundDataModel> SoundData;

        // Keep a buffer of all messages for as long as the client UX needs them
        static TimeSpan bufferTimeInterval = new TimeSpan(0, 1, 0);

        public string LastMessageOffset { get; private set; }

        public Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            return Task.FromResult<object>(null);
        }

        public Task OpenAsync(PartitionContext context)
        {
            SoundData = new List<SoundDataModel>();
            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Resets all averages
        /// </summary>
        public static void ResetAverages()
        {
            SoundData.Clear();
            SoundDataMax = new SoundDataModel();
            SoundDataMin = new SoundDataModel();
            SoundDataLatest = new SoundDataModel();
            SoundDataAVG = 0;
            soundDataTotal = 0;
            soundDataCount = 0;
            startTime = null;
        }

        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            try
            {
                foreach (EventData eventData in messages)
                {
                    // We don't care about messages that are older than bufferTimeInterval
                    if ((eventData.EnqueuedTimeUtc + bufferTimeInterval) >= DateTime.UtcNow )
                    {
                        // reset the offset so we don't keep reading old messages
                        this.LastMessageOffset = eventData.Offset;

                        // get the message
                        string data = Encoding.UTF8.GetString(eventData.GetBytes());

                        // deserialise the json data
                        var model = JsonConvert.DeserializeObject<SoundDataModel>(data);

                        // set the latest value
                        SoundDataLatest = model;

                        // add the value to our list
                        if (SoundData == null)
                            SoundData = new List<SoundDataModel>();
                        SoundData.Add(model);

                        // remove trailing values if over the max
                        if (SoundData.Count == maxRecordsToStore)
                            SoundData.RemoveAt(0);

                        // set the start time as the time of the first event received
                        if (!startTime.HasValue)
                            startTime = model.Time;

                        if (SoundDataMax == null) SoundDataMax = model;
                        if (SoundDataMin == null) SoundDataMin = model;

                        // calculate the aggregates
                        if ( model.Sound > SoundDataMax.Sound)
                            SoundDataMax = model;

                        if (model.Sound < SoundDataMin.Sound || SoundDataMin.Sound == 0)
                            SoundDataMin = model;

                        soundDataCount++;
                        soundDataTotal += model.Sound;
                        SoundDataAVG = soundDataTotal / soundDataCount;
                        SoundCollectionPeriod = DateTime.Now.ToUniversalTime().Subtract(startTime.Value.ToUniversalTime());
                    }
                }

                await context.CheckpointAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error processing events: " + ex.Message);
            }
        }
    }

    /// <summary>
    /// This event processor is used for handling cloud to device messages sent to the IoTHub
    /// Messages that need to be sent to a device are sent to the IoTHub for the device
    /// The IoTHub then queues these messages and sends them to the device in  a reliable fashion
    /// </summary>
    public class ClapOMeterDeviceQueueEventProcessor : IEventProcessor
    {
        public string LastMessageOffset { get; private set; }

        public Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            return Task.FromResult<object>(null);
        }

        public Task OpenAsync(PartitionContext context)
        {
            return Task.FromResult<object>(null);
        }

        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            try
            {
                foreach (EventData eventData in messages)
                {
                    this.LastMessageOffset = eventData.Offset;

                    string data = Encoding.UTF8.GetString(eventData.GetBytes());

                    var model = JsonConvert.DeserializeObject<SoundDataModel>(data);

                    Newtonsoft.Json.Linq.JObject json = JObject.Parse(data);
                    string action = json.GetValue("action").ToString();

                    // send this data to the appropriate evice
                    await SendMessageIoTHubForToDevice(model.DeviceId, action);

                    Trace.WriteLine("Processed cloud to device event for Device: " + model.DeviceId + ", Action: " + action);

                }

                await context.CheckpointAsync();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Error processing events: " + ex.Message);
            }
        }

        /// <summary>
        /// Sends the data to a device registered in the IoT Hub
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private async Task SendMessageIoTHubForToDevice(string deviceId, string data)
        {
            var ioTHubConnectionString = Microsoft.WindowsAzure.CloudConfigurationManager.GetSetting("EventHubConnectionString");
            ServiceClient serviceClient = ServiceClient.CreateFromConnectionString(ioTHubConnectionString);
            var commandMessage = new Microsoft.Azure.Devices.Message(Encoding.ASCII.GetBytes(data));
            await serviceClient.SendAsync(deviceId, commandMessage);
        }
    }
}
