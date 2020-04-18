using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LiveStreamingServer
{
    public class Program
    {
        private static Options options;
        private static Process ffmpeg;

        public static void Main(string[] args)
        {
            options = GetOptions(args);

            if (options.AutoStart)
            {
                Action restart = null;
                restart = () => { Task.Run(StartFfmpeg).ContinueWith(t => restart()); };
                restart();
            }

            HttpListener listener = new HttpListener();
            listener.Prefixes.Add($"http://127.0.0.1/");
            listener.Prefixes.Add($"http://{GetLocalIPAddress().ToString()}/");
            if (!string.IsNullOrEmpty(options.Hostname))
            {
                listener.Prefixes.Add($"http://{options.Hostname}/");
            }

            listener.Start();

            while (true)
            {
                try
                {
                    HttpListenerContext context = listener.GetContext();
                    RunAsync(context);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        private static Options GetOptions(string[] args)
        {
            Options options = new Options();
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "-h":
                    case "--help":
                        Console.WriteLine(Help.Message);
                        Environment.Exit(0);
                        break;
                    case "-d":
                    case "--host":
                        options.Hostname = args[++i];
                        break;
                    case "-f":
                    case "--ffmpeg":
                        options.FfmpegLocation = args[++i];
                        break;
                    case "-p":
                    case "--ffmpegport":
                        options.FfmpegPort = int.Parse(args[++i]);
                        break;
                    case "-o":
                    case "--output":
                        options.StreamDirectory = args[++i];
                        break;
                    case "-r":
                    case "--fps":
                        options.FPS = int.Parse(args[++i]);
                        break;
                    case "-k":
                    case "--keyframe":
                        options.KeyFrame = int.Parse(args[++i]);
                        break;
                    case "-l":
                    case "--listsize":
                        options.ListSize = int.Parse(args[++i]);
                        break;
                    case "-w":
                    case "--hlswrap":
                        options.HLSWrap = int.Parse(args[++i]);
                        break;
                    case "-a":
                    case "--autostart":
                        options.AutoStart = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"'{args}' is not a supported argument. Run with --help for more information on options.");
                }
            }

            if (string.IsNullOrEmpty(options.FfmpegLocation))
            {
                throw new ArgumentException("--ffmpeg");
            }
            else if (string.IsNullOrEmpty(options.StreamDirectory))
            {
                throw new ArgumentException("--output");
            }

            return options;
        }

        private static void RunAsync(HttpListenerContext context)
        {
            string url = context.Request.RawUrl.Substring(1);

            switch(url)
            {
                case "":
                case "index.html":
                    ServePage(context.Response);
                    break;
                case "start":
                    Start(context.Request, context.Response);
                    break;
                default:
                    ServeStream(context.Request, context.Response);
                    break;
            }
        }

        private static void Start(HttpListenerRequest request, HttpListenerResponse response)
        {
            if (options.AutoStart)
            {
                Bad(response, "Server will autostart");
                return;
            }

            string local = GetLocalIPAddress().ToString();
            string remote = request.RemoteEndPoint.Address.ToString();

            // Make sure the request comes from within the network, or on the loopback address
            if (remote != "127.0.0.1" && remote.Substring(0, remote.LastIndexOf(".")) != local.Substring(0, local.LastIndexOf(".")))
            {
                Bad(response, "Request must be made from the local network");
                return;
            }

            if (ffmpeg != null)
            {
                Bad(response, "Server already running");
                return;
            }

            Task.Run(StartFfmpeg);

            OK(response, "started ffmpeg");
        }

        private static void StartFfmpeg()
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = options.FfmpegLocation;
            psi.Arguments = $"-v verbose -listen 1 -i rtmp://{GetLocalIPAddress().ToString()}:{options.FfmpegPort}/live/app -framerate {options.FPS} -hls_time 1 -hls_list_size {options.ListSize} -hls_wrap {options.HLSWrap} -preset veryfast -tune zerolatency -x264-params keyint={options.KeyFrame} {options.StreamDirectory}\\stream.m3u8";
            ffmpeg = new Process();
            ffmpeg.StartInfo = psi;
            ffmpeg.Start();
            Console.WriteLine("Started rtmp server");
            ffmpeg.WaitForExit();
            Console.WriteLine("Stopped rtmp server");
            ffmpeg = null;
        }

        public static IPAddress GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }

            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        private static void ServePage(HttpListenerResponse response)
        {
            response.ContentType = "text/html";

            byte[] output;
            // ffmpeg doesn't clean up the streaming files, but we can check if the start time is greater than the last time to write.
            if (ffmpeg == null || ffmpeg.StartTime.ToUniversalTime() > new FileInfo($"{options.StreamDirectory}\\stream.m3u8").LastWriteTimeUtc)
            {
                output = Encoding.UTF8.GetBytes("<html><body>Stream is not live.</body></html>");
            }
            else
            {
                output = Encoding.UTF8.GetBytes(File.ReadAllText("index.html"));
            }

            response.ContentLength64 = output.Length;
            response.ContentEncoding = Encoding.UTF8;
            response.OutputStream.Write(output, 0, output.Length);
            response.StatusCode = (int)HttpStatusCode.OK;
            response.StatusDescription = "OK";
            response.OutputStream.Close();
        }

        private static void ServeStream(HttpListenerRequest request, HttpListenerResponse response)
        {
            string video = request.RawUrl.Substring(1);
            video = Path.GetFileName(video);

            // attempt to sanitize input here. Try to prevent path traversal
            if (Path.GetDirectoryName(Path.Combine(options.StreamDirectory, video)) != Path.GetDirectoryName($"{options.StreamDirectory}\\"))
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.OutputStream.Close();
                return;
            }

            if (!File.Exists($"{options.StreamDirectory}\\{video}"))
            {
                response.StatusCode = (int)HttpStatusCode.NotFound;
                response.OutputStream.Close();
                return;
            }

            using (Stream stream = new FileStream($"{options.StreamDirectory}\\{video}", FileMode.Open, FileAccess.Read))
            {
                if (Path.GetExtension(video) == ".m3u8")
                {
                    response.ContentType = "application/x-mpegURL";
                }
                else if(Path.GetExtension(video) == ".ts")
                {
                    response.ContentType = "video/MP2T";
                }
                else
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.OutputStream.Close();
                    return;
                }

                response.StatusCode = (int)HttpStatusCode.OK;

                // Send out data in 1KiB chunks
                byte[] packet = new byte[1024 * 1024];
                try
                {
                    int count;
                    while ((count = stream.Read(packet, 0, 1024 * 1024)) > 0)
                    {
                        response.OutputStream.Write(packet, 0, count);
                        response.OutputStream.Flush();
                    }

                    response.OutputStream.Close();
                }
                catch
                {
                    // Uh-oh
                }
            }
        }

        private static void Bad(HttpListenerResponse response, string message)
        {
            Respond(response, (int)HttpStatusCode.BadRequest, "BAD", message);
        }

        private static void OK(HttpListenerResponse response, string message)
        {
            Respond(response, (int)HttpStatusCode.OK, "OK", message);
        }

        private static void Respond(HttpListenerResponse response, int statusCode, string statusDescription, string message)
        {
            byte[] output = Encoding.UTF8.GetBytes(message);
            response.ContentLength64 = output.Length;
            response.ContentEncoding = Encoding.UTF8;
            response.OutputStream.Write(output, 0, output.Length);
            response.StatusCode = statusCode;
            response.StatusDescription = statusDescription;
            response.OutputStream.Close();
        }
    }

    class Options
    {
        public string Hostname { get; set; }
        public string FfmpegLocation { get; set; }
        public string StreamDirectory { get; set; }
        public int FfmpegPort { get; set; } = 8889;
        public int FPS { get; set; } = 30;
        public int HLSWrap { get; set; } = 3;
        public int ListSize { get; set; } = 2;
        public int KeyFrame { get; set; } = 30;
        public bool AutoStart { get; set; } = false;
    }
}
