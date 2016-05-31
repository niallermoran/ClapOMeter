using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using Owin;

using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System.Threading;
using System.Diagnostics;

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
            EventProcessorHost eventProcessorHost = new EventProcessorHost(Guid.NewGuid().ToString(),
                 "clapometeriothub",
                 "clapometer",
                 "Endpoint=sb://iothub-ns-clapometer-41652-7dcd69e3c6.servicebus.windows.net/;SharedAccessKeyName=iothubowner;SharedAccessKey=y0Hdq5gOXk0MhbyuqQnt/+uDbzGa+QlqQDHUsWjLdEo=",
                 "DefaultEndpointsProtocol=https;AccountName=clapometer;AccountKey=xEn4z6Q83yOJXij8PyDgpQVTrsU/d8MBDGtG/hEEMaNJKTneaLZ2dSRU+EHl4AttO0Vi3Vapr5W7RU29XHS0kg==");

            var options = new EventProcessorOptions();
            options.ExceptionReceived += (sender, e) => {
                Trace.WriteLine(e.Exception);
            };
            await eventProcessorHost.RegisterEventProcessorAsync<ClapOMeterEventProcessor>(options);
        }
    }
}
