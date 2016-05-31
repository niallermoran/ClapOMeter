using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Website.Models;

namespace Website.Controllers
{
    public class SoundDataController : ApiController
    {
        public IEnumerable<SoundDataModel> Get()
        {
            return ClapOMeterEventProcessor.SoundData.OrderByDescending( x=>x.Time );
        }
    }

    public class SoundAggregatesDataController : ApiController
    {
        public object Get()
        {
            var json = new {
                Maximum = ClapOMeterEventProcessor.SoundDataMax,
                Minimum = ClapOMeterEventProcessor.SoundDataMin,
                Average = ClapOMeterEventProcessor.SoundDataAVG
            };
            return json;
        }
    }
}
