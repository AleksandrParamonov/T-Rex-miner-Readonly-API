using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace T_RexReadOnlyAPI
{
    class ReadOnlyAPI
    {
        private readonly object responseLockObject = new object();
        string cachedResponse = string.Empty;
        string ipAddress = string.Empty;
        int ipPort = 0;

        /// <summary>
        /// Tries to connect to 8.8.8.8 to get IPv4 address with internet connection
        /// </summary>
        /// <returns></returns>
        static string GetIpAddress()
        {
            string answer = string.Empty;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                answer = endPoint.Address.ToString();
            }
            return answer;
        }

        public void Start()
        {
            Thread backgroundPolling = new Thread(PollingThread);
            Thread httpHandler = new Thread(HttpListenerThread);
            backgroundPolling.Start();
            httpHandler.Start();
        }

        /// <summary>
        /// Creates readonly monitor on selected ip address and port
        /// </summary>
        /// <param name="addr">Address for HttpListener</param>
        /// <param name="port">Port for HttpListener</param>
        public ReadOnlyAPI(string addr, int port)
        {
            ipAddress = addr;
            ipPort = port;

            cachedResponse = "no result yet";

        }

        /// <summary>
        /// Creates readonly monitor on default ip address and port
        /// </summary>
        /// <param name="port">Port for HttpListener</param>
        public ReadOnlyAPI(int port) : this(GetIpAddress(), port) { }

        /// <summary>
        /// Creates readonly monitor on default ip address and default port(4069)
        /// </summary>   
        public ReadOnlyAPI() : this(GetIpAddress(), 4069) { }

        /// <summary>
        /// Updates cached "summary" response every sec with 50ms timeout
        /// </summary>
        void PollingThread()
        {
            while (true)
            {
                lock (responseLockObject)
                {
                    try
                    {
                        cachedResponse = (new HttpClient() { Timeout = TimeSpan.FromMilliseconds(50) }).GetAsync("http://127.0.0.1:4067/summary").Result.Content.ReadAsStringAsync().Result;
                    }
                    catch //is thrown on timeout
                    {
                        cachedResponse = "T-Rex is offline or \"127.0.0.1:" + 4067 + "\" is wrong T-Rex HTTP address";
                        Console.WriteLine(cachedResponse);
                    }
                }
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Handles incoming requests in endless loop
        /// </summary>
        void HttpListenerThread()
        {
            HttpListener httpListener = new HttpListener();
            httpListener.Prefixes.Add("http://" + ipAddress + ":" + ipPort + "/");
            httpListener.Prefixes.Add("http://" + "localhost" + ":" + ipPort + "/");
            httpListener.Start();

            while (true)
            {
                IAsyncResult result = httpListener.BeginGetContext(new AsyncCallback(ListenerCallback), httpListener);
                result.AsyncWaitHandle.WaitOne();
            }
        }

        void ListenerCallback(IAsyncResult result)
        {
            HttpListener listener = (HttpListener)result.AsyncState;

            HttpListenerContext context = listener.EndGetContext(result);
            HttpListenerRequest request = context.Request;

            HttpListenerResponse response = context.Response;
            response.ContentType = "application/json; charset=utf-8";
            string responseString = string.Empty;
            lock (responseLockObject)
            {
                responseString = string.Copy(cachedResponse);
            }
            byte[] responseByteArray = System.Text.Encoding.UTF8.GetBytes(responseString);

            response.ContentLength64 = responseByteArray.Length;
            System.IO.Stream output = response.OutputStream;
            output.Write(responseByteArray, 0, responseByteArray.Length);

            output.Close();
        }


    }
    class Program
    {

        static void Main(string[] args)
        {
            ReadOnlyAPI api;
            switch (args.Length)
            {
                case 0:
                    api = new ReadOnlyAPI();
                    break;
                case 1:
                    api = new ReadOnlyAPI(int.Parse(args[0]));
                    break;
                case 2:
                    api = new ReadOnlyAPI(args[0], int.Parse(args[1]));
                    break;
                default:
                    Console.WriteLine("Wrong number of arguments");
                    return;
            }

            api.Start();

        }
    }
}
