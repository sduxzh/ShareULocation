using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
//完成
namespace AsyncSocketServer
{
    class DaemonThread : Object
    {
        private Thread m_thread;
        private AsyncSocketServer m_asyncSocketServer;

        public DaemonThread(AsyncSocketServer asyncSocketServer)
        {
            m_asyncSocketServer = asyncSocketServer;
            m_thread = new Thread(DaemonThreadStart);
            m_thread.Start();
        }

        /// <summary>
        /// 检查超时连接并断开
        /// </summary>
        public void DaemonThreadStart()
        {
            while (m_thread.IsAlive)
            {
                AsyncSocketUserToken[] userTokenArray = null;//当前连接到服务器的userToken
                m_asyncSocketServer.AsyncSocketUserTokenList.CopyList(ref userTokenArray);//拷贝一份当前正在连接的客户端列表

                //检查费活动连接，关闭超时连接
                for (int i = 0; i < userTokenArray.Length; i++)
                {
                    if (!m_thread.IsAlive)
                        break;
                    try
                    {
                        if ((DateTime.Now - userTokenArray[i].ActiveDateTime).Milliseconds > m_asyncSocketServer.SocketTimeOutMS) //超时Socket断开
                        {
                            lock (userTokenArray[i])
                            {
                                m_asyncSocketServer.CloseClientSocket(userTokenArray[i]);//断开连接
                            }
                        }
                    }                    
                    catch (Exception E)
                    {
                        Program.Logger.ErrorFormat("Daemon thread check timeout socket error, message: {0}", E.Message);
                        Program.Logger.Error(E.StackTrace);
                    }
                }

                for (int i = 0; i < 60 * 1000 / 10; i++) //每分钟检测一次
                {
                    if (!m_thread.IsAlive)
                        break;
                    Thread.Sleep(10);
                }
            }
        }

        /// <summary>
        /// 阻塞该线程
        /// </summary>
        public void Close()
        {
            m_thread.Abort();//开始终止的过程
            m_thread.Join();//阻塞该线程
        }
    }
}
