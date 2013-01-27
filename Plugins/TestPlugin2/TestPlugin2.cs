using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using CollectorHost.Plugin;
using CollectorHost.Host;
using System.Timers;

namespace TestPlugin
{
    [Export( typeof( IPlugin ) )]  
    public class TestPlugin2:IPlugin
    {
        private Timer generator;
        public IHost Host { get; set; }
        public string Name { get; set; }
        private bool initialized = false;
        private Random r = new Random(DateTime.Now.Second);
        private float previousValue = 100;

        public void Initialize()
        {
            // Load our name from the host, this cannot be done in the constructor
            string name = Host.GetName();
            Name = "TestPlugin2";
            if (generator == null)
            {
                generator = new Timer(5000);
                generator.Elapsed += new ElapsedEventHandler(Generate);
            }
            initialized = true;
        }

        private void Generate(object sender, ElapsedEventArgs e)
        {
            if (Host != null)
            {
                float delta = r.Next(-20,20);
                Host.Enqueue("tcollector.testplugin.5s", ToUTCSeconds(DateTimeOffset.Now), previousValue + delta, "host=" + Host.GetName());
                //previousValue += delta;
            }
        }

        public void Start()
        {
            if (initialized)
            {
                generator.Start();
            }
        }
        
        public void Stop()
        {
            if (initialized)
            {
                generator.Stop();
            }
        }

        public long ToUTCSeconds(DateTimeOffset timestamp)
        {
            TimeSpan span = (timestamp.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
            long utcSeconds = (long)Math.Round(span.TotalSeconds);
            return utcSeconds;
        }

        private void ConsoleWrite(ConsoleColor consoleColor, string text)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = consoleColor;
            Console.Write(text);
            Console.ForegroundColor = originalColor;
        }
        private void ConsoleWriteLine(ConsoleColor consoleColor, string text)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = consoleColor;
            Console.WriteLine(text);
            Console.ForegroundColor = originalColor;
        }
    }
}



