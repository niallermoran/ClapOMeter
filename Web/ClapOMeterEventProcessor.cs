using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ServiceBus.Messaging;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Web
{
    public class ClapOMeterEventProcessor : IEventProcessor
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
                    // reset the offset so we don't keep reading old messages
                    this.LastMessageOffset = eventData.Offset;

                    // get the message
                    string data = Encoding.UTF8.GetString(eventData.GetBytes());

                  //  var model = JsonConvert.DeserializeObject<TemperatureReading>(data);

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
