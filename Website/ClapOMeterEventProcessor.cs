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
        public static List<SoundDataModel> SoundData = new List<SoundDataModel>();
        public static SoundDataModel SoundDataMax;
        public static SoundDataModel SoundDataMin;
        public static double SoundDataAVG;
        private static double soundDataTotal;
        private static int soundDataCount = 0;

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
                    // reset the offset so we don't keep reading old messages
                    this.LastMessageOffset = eventData.Offset;

                    // get the message
                    string data = Encoding.UTF8.GetString(eventData.GetBytes());

                    // deserialise the json data
                    var model = JsonConvert.DeserializeObject<SoundDataModel>(data);

                    // add the new item
                    SoundData.Add(model);

                    // calculate the aggregates
                    if ( SoundDataMax == null || model.Sound == SoundDataMax.Sound )
                        SoundDataMax = model;

                    if (SoundDataMin == null || model.Sound < SoundDataMax.Sound)
                        SoundDataMin = model;

                    soundDataCount++;
                    soundDataTotal += model.Sound;
                    SoundDataAVG = soundDataTotal / soundDataCount;

                    // trim down the list to no more than 100 items for realtime graphs
                    if ( SoundData.Count > 100 )
                    {
                        int delta = SoundData.Count - 100;
                        SoundData.RemoveRange( SoundData.Count - delta, delta);
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
