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

namespace Website
{
    public class ClapOMeterEventProcessor : IEventProcessor
    {
        public static SoundDataModel SoundDataMax;
        public static SoundDataModel SoundDataMin;
        public static SoundDataModel SoundDataLatest;
        public static double SoundDataAVG;
        private static double soundDataTotal;
        private static int soundDataCount = 0;
        private static DateTime? startTime;
        public static TimeSpan SoundCollectionPeriod;

        // Keep a buffer of all messages for as long as the client UX needs them
        static TimeSpan bufferTimeInterval = new TimeSpan(0, 1, 0);

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
                    // We don't care about messages that are older than bufferTimeInterval
                    if ((eventData.EnqueuedTimeUtc + bufferTimeInterval) >= DateTime.UtcNow )
                    {

                        // reset the offset so we don't keep reading old messages
                        this.LastMessageOffset = eventData.Offset;

                        // get the message
                        string data = Encoding.UTF8.GetString(eventData.GetBytes());

                        // deserialise the json data
                        var model = JsonConvert.DeserializeObject<SoundDataModel>(data);

                        // push this to rest api or elsewhere

                        // set the latest value
                        SoundDataLatest = model;

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
                        SoundCollectionPeriod = DateTime.Now.Subtract(startTime.Value);
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
}
