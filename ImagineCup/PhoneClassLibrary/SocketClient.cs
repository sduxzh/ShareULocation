using System;
using System.Data.Linq;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ClientClassLibrary.SyncSocketProtocol;

namespace ClientClassLibrary
{
    public class SocketClient
    {
        private ClientUploadSocket _clientSocket;//连接服务器的socket
        private const string ServerIp = "42.159.6.187";//服务器IP地址
        private const int ServerPort = 8856;//通信端口号

        public SocketClient()
        {
            _clientSocket=new ClientUploadSocket();
            
        }
        /// <summary>
        /// 服务器连接操作
        /// </summary>
        public void Connect_Server()
        {
            _clientSocket.Connect(ServerIp, ServerPort);

        }
        
        /// <summary>
        /// 检测是否连接到服务器
        /// </summary>
        /// <returns></returns>
        public bool ConnectState()
        {
           return _clientSocket.ConnectState();
        }

        /// <summary>
        /// 向服务器发送注册信息
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="mail"></param>
        public bool SendRegister(string username, string password, string mail)
        {
            return _clientSocket.DoRegister(username,password,mail);
        }
        /// <summary>
        /// 向服务器发送位置坐标
        /// </summary>
        public void SendLocation(string roadid,string roadname,string lat,string lon) 
        {
            _clientSocket.DoSendLocation(roadid,roadname,lat,lon);
        }

        /// <summary>
        /// 查询道路状态
        /// </summary>
        /// <param name="roadname"></param>
        /// <returns></returns>
        public bool AcquireRoadState(string roadname)
        {
            return _clientSocket.DoAcquireRoadState(roadname);
        }

        /// <summary>
        /// 向服务器发送状态信息
        /// </summary>
        public bool DoActive()
        {
           return _clientSocket.DoActive();
        }

        /// <summary>
        /// 登陆服务器
        /// </summary>
        /// <returns></returns>
        public bool DoLogin(string username,string password)
        {
           return _clientSocket.DoLogin(username, password);
        }

        /// <summary>
        /// 判断客户端是否需要重连
        /// 如果断开，自动重新连接
        /// </summary>
        /// <returns></returns>
        public bool ReConnect()
        {
            return _clientSocket.ReConnect();
        }

        /// <summary>
        /// 判断客户端是否需要重连
        /// 如果断开，自动重新连接并登陆
        /// </summary>
        /// <returns></returns>
        public bool ReConnectAndLogin()
        {
            return _clientSocket.ReConnectAndLogin();
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect()
        {
            _clientSocket.Disconnect();
        }
        
    }
}