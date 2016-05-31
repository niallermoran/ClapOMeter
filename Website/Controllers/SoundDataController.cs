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
            var json = new
            {
                Maximum = ClapOMeterEventProcessor.SoundDataMax,
                Minimum = ClapOMeterEventProcessor.SoundDataMin,
                Average = Math.Round( ClapOMeterEventProcessor.SoundDataAVG),
                TimeFrame = string.Format("{0} days, {1} hours, {2} minutes, {3} seconds",
                ClapOMeterEventProcessor.SoundCollectionPeriod.Days.ToString(),
                ClapOMeterEventProcessor.SoundCollectionPeriod.Hours.ToString(),
                ClapOMeterEventProcessor.SoundCollectionPeriod.Minutes.ToString(),
                ClapOMeterEventProcessor.SoundCollectionPeriod.Seconds.ToString())
            };
            return json;
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
