using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CollectorHost
{
    public class DataPoint
    {
        public string Metric { get; set; }
        public long Timestamp { get; set; }
        public float Value { get; set; }
        public string Tags { get; set; }
    }
}
