using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Website.Models;

namespace Website.Controllers
{
    public class SoundAggregatesDataController : ApiController
    {
        public object Get()
        {
            var json = new
            {
                Maximum = ClapOMeterEventProcessor.SoundDataMax,
                Minimum = ClapOMeterEventProcessor.SoundDataMin,
                Average = Math.Round( ClapOMeterEventProcessor.SoundDataAVG),
                Current = ClapOMeterEventProcessor.SoundDataLatest,
                TimeFrame = string.Format("{0} days, {1} hours, {2} minutes, {3} seconds",
                ClapOMeterEventProcessor.SoundCollectionPeriod.Days.ToString(),
                ClapOMeterEventProcessor.SoundCollectionPeriod.Hours.ToString(),
                ClapOMeterEventProcessor.SoundCollectionPeriod.Minutes.ToString(),
                ClapOMeterEventProcessor.SoundCollectionPeriod.Seconds.ToString())
            };
            return json;
        }

        [Route("ResetAggregates")]
        public IHttpActionResult ResetAggregates()
        {
            ClapOMeterEventProcessor.ResetAverages();
            return Ok();
        }
    }

    public class SoundDataController : ApiController
    {
        public List<SoundDataModel> Get()
        {
            return ClapOMeterEventProcessor.SoundData;
        }
    }

    public class SoundLatestDataController : ApiController
    {
        public SoundDataModel Get()
        {
            return ClapOMeterEventProcessor.SoundDataLatest;
        }
    }
}
