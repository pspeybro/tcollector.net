using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CollectorHost;

namespace TCollector
{
    class Program
    {
        static void Main(string[] args)
        {
            TCollectorHost collector = new TCollectorHost();
            collector.Initialize();
            if (collector.Connect("demo", 4242))
                collector.StartCollecting();

            while (true)
            {
                System.Threading.Thread.Sleep(100);
            }
        }
    }
}
