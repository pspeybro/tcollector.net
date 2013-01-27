using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CollectorHost.Host
{
    public interface IHost
    {
        string GetName();
        //Queue<KeyValuePair<long, float>> GetQueue();
        bool Enqueue(string metric, long timestamp, float data, string tags);
    }
}
