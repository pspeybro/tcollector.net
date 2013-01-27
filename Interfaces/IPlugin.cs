using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CollectorHost.Host;

namespace CollectorHost.Plugin
{
    public interface IPlugin
    {
        string Name { get; set; }
        IHost Host { get; set; }
        void Initialize();
        void Start();
        void Stop();
    }
}
