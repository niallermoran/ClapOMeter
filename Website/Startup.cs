using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using Owin;
using System.Threading.Tasks;

using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System.Threading;
using System.Diagnostics;
using Microsoft.WindowsAzure;

[assembly: OwinStartup(typeof(Website.Startup))]

namespace Website
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            try
            {
                SetupEventHubProcessor().Wait();
            }
            catch( Exception ex)
            {
                Trace.WriteLine("An error occured in startup within SetupEventHubProcessor: " + ex.Message + ": " + ex.StackTrace);
                throw new System.Web.HttpException("An error occurred setting up the event processor host for your IoT Hub, please check your connection settings are correct: " + ex.Message);
            }
        }

        private async Task SetupEventHubProcessor()
        {
            var name = CloudConfigurationManager.GetSetting("IoTHubName");
            var consumergroup = CloudConfigurationManager.GetSetting("IoTHubConsumerGroup");
            var connectionstring = CloudConfigurationManager.GetSetting("IoTHubConnectionString");
            var storagecnstring = CloudConfigurationManager.GetSetting("StorageConnectionString");

            EventProcessorHost eventProcessorHost = new EventProcessorHost(Guid.NewGuid().ToString(), name, consumergroup, connectionstring, storagecnstring);
            
            var options = new EventProcessorOptions();
            options.ExceptionReceived += (sender, e) => {
                Trace.WriteLine(e.Exception);
            };
            await eventProcessorHost.RegisterEventProcessorAsync<ClapOMeterEventProcessor>(options);
        }
    }
}
