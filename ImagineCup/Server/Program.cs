using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Reflection;
using log4net;
using log4net.Config;

[assembly: XmlConfigurator(Watch = true)]

namespace AsyncSocketServer
{    
    public class Program
    {
        public static ILog Logger;
        public static AsyncSocketServer AsyncSocketSvr;//服务器Socket
        
        static void Main(string[] args)
        {
            DateTime currentTime = DateTime.Now;//获取系统当前时间
            GlobalContext.Properties["LogDir"] = currentTime.ToString("yyyyMM");
            GlobalContext.Properties["LogFileName"] = "_SocketAsyncServer" + currentTime.ToString("yyyyMMdd");
            Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);//将当前应用程序的配置文件作为 System.Configuration.Configuration 对象打开

            //配置服务器端口
            int port = 0;
            //if (!(int.TryParse(config.AppSettings.Settings["Port"].Value, out port)))
                port = 8856;
            //配置并发客户端数量
            int parallelNum = 0;
            if (!(int.TryParse(config.AppSettings.Settings["ParallelNum"].Value, out parallelNum)))
                parallelNum = 8000;
            //设置超时时间
            int socketTimeOutMS = 0;
            if (!(int.TryParse(config.AppSettings.Settings["SocketTimeOutMS"].Value, out socketTimeOutMS)))
                socketTimeOutMS = 5 * 60 * 1000;

            
            AsyncSocketSvr = new AsyncSocketServer(parallelNum);//new AsyncSocketServer
            AsyncSocketSvr.SocketTimeOutMS = socketTimeOutMS;//设置超时时间
            AsyncSocketSvr.Init();//初始化AsyncSocketServer
            IPEndPoint listenPoint = new IPEndPoint(IPAddress.Any, port);
            AsyncSocketSvr.Start(listenPoint);//开启服务器

            Console.WriteLine("Press any key to terminate the server process....");
            Console.ReadKey();
        }
    }
}

