using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website.Models
{
    public class SoundDataModel
    {
        public string DeviceId { get; set; }
        public DateTime Time { get; set; }
        public double Sound { get; set; }

        /// <summary>
        /// Show time in Irish time. Time coming from IoT Hub is always Universal
        /// </summary>
        public DateTime TimeLocal
        {
            get
            {
                return TimeZoneInfo.ConvertTimeFromUtc(Time, TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"));

            }
        }
        public string TimeLabel
        {
            get
            {
                return TimeLocal.ToString("HH:mm:ss");
            }
        }


        public string TimeLabelShort
        {
            get
            {
                return TimeLocal.ToString("HH:mm");
            }
        }


        public string DateTimeLabel
        {
            get
            {
                return TimeLocal.ToString("dd MMM yyyy HH:mm:ss");
            }
        }

    }
}