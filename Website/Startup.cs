using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using Owin;

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

            SetupEventHubProcessor();
        }

        private async void SetupEventHubProcessor()
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
