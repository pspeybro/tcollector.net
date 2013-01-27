using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using CollectorHost.Host;
using CollectorHost.Plugin;
using System.Collections.Concurrent;
using System.Reflection;
using System.Timers;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace CollectorHost
{
    public class TCollectorHost: IHost
    {
        [ImportMany(typeof(IPlugin))]
        private IEnumerable<IPlugin> plugins;        
        private ConcurrentQueue<DataPoint> queue;
        private bool initialized = false;
        private AggregateCatalog catalog;
        private CompositionContainer container;
        private Timer consumer;
        public bool Connected {get; set;}
        Timer watchdog;
        Socket socket;

        public void Initialize()
        {
            Console.WriteLine("Initializing Collectors...");
            // Make sure the plugins know where the host is...
            //instance.Host = this;

            catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
            catalog.Catalogs.Add(new DirectoryCatalog("."));
            container = new CompositionContainer(catalog);
            //container.SatisfyImportsOnce(this);
            container.ComposeParts(this);

            foreach (IPlugin plugin in plugins)
            {
                plugin.Host = this;
                plugin.Initialize();
                Console.Write(">> Plugin Initialized: ");
                ConsoleWriteLine(ConsoleColor.Blue, plugin.Name);
            }

            //try to empty the queue every 3 seconds
            consumer = new Timer(3000);
            consumer.Elapsed += new ElapsedEventHandler(SendData);
            consumer.Start();
            initialized = true;
        }

        /// <summary>
        /// Try to open connection and start watchdog to keep an eye on the connection and try to reconnect if necessary
        /// </summary>
        /// <param name="server">Server url or IP address</param>
        /// <param name="port">Port to connect to. Default OpenTSDB port is 4242.</param>
        /// <returns></returns>
        public bool Connect(string server, int port)
        {
            Console.Write("Connecting ... ");
            IPAddress ip;
            if (!IPAddress.TryParse(server, out ip))
            {
                try
                {
                    IPAddress[] addresslist = Array.FindAll(
                        Dns.GetHostEntry(server).AddressList,
                            a => a.AddressFamily == AddressFamily.InterNetwork);

                    if (addresslist.Length > 0)
                    {
                        ip = addresslist[0];
                    }
                }
                catch (Exception ex)
                {
                    ConsoleWriteLine(ConsoleColor.Red, "Failed");
                    Console.WriteLine(string.Format(" => Server not found: {0}, {1}", server, ex.Message));

                    Debug.WriteLine(string.Format("Server not found: {0}" + Environment.NewLine + ex.ToString(), server));
                    //MessageBox.Show(string.Format("Server not found: {0}" + Environment.NewLine + ex.ToString(), server));
                    return false;
                }
            }

            try
            {
                IPEndPoint endPoint = new IPEndPoint(ip, port); // tsdb deamon port
                
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(endPoint);
                Connected = true;
                ConsoleWriteLine(ConsoleColor.Green, "Success");                
            }
            catch (Exception ex)
            {
                ConsoleWriteLine(ConsoleColor.Red, "Failed");
                Console.WriteLine(string.Format(" => Exception: {0}", ex.Message));
                Debug.WriteLine(ex);
                //MessageBox.Show(ex.Message, "Connection problem");
            }
            return true;
        }

        void SendData(object sender, ElapsedEventArgs e)
        {
            //prevent multiple firings while emptying queue
            consumer.Stop();

            if (queue != null)
            {
                DataPoint dp;
                while (queue.TryDequeue(out dp))
                {
                    //send dp to OpenTSDB
                    if (Connected)
                    {
                        string message = string.Format("put {0}", dp.ToString());
                        socket.Send(new ASCIIEncoding().GetBytes(message + Environment.NewLine));
                    }

                    dp.ToString();
                }
            }
            consumer.Start();
        }

        public void StartCollecting()
        {
            Console.WriteLine("Starting data collection...");
            if (initialized)
            {
                foreach (IPlugin plugin in plugins)
                {
                    plugin.Start();
                }
            }
        }

        public string GetName()
        {
            return Environment.MachineName;
        }
               

        public bool Enqueue(string metric, long timestamp, float data, string tags)
        {
            if (queue == null)
            {
                queue = new ConcurrentQueue<DataPoint>();                
            }

            queue.Enqueue(new DataPoint
                {
                    Metric = metric,
                    Timestamp = timestamp,
                    Value = data,
                    Tags = tags
                });
            Console.WriteLine("DataPoint: " + metric + " " + data+"{"+tags+"}");
            return true;
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



