using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace AsyncSocketServer
{
    public class AsyncSocketServer
    {
        private Socket listenSocket;

        private int m_numConnections; //服务器接收客户端的个数，供初始化信号量m_maxNumberAcceptedClients调用
        private int m_receiveBufferSize; //接收缓冲区大小
        private Semaphore m_maxNumberAcceptedClients; //信号量：最大接收的客户端数量

        private int m_socketTimeOutMS; //Socket最大超时时间，单位为MS
        public int SocketTimeOutMS { get { return m_socketTimeOutMS; } set { m_socketTimeOutMS = value; } }

        private AsyncSocketUserTokenPool m_asyncSocketUserTokenPool;//AsyncSocketUserToken缓冲池
        private AsyncSocketUserTokenList m_asyncSocketUserTokenList;//syncSocketUserTokenList：已连接的客户端
        public AsyncSocketUserTokenList AsyncSocketUserTokenList { get { return m_asyncSocketUserTokenList; } }
        
        //----------------------------未看-------------------------------------//
        private LogOutputSocketProtocolMgr m_logOutputSocketProtocolMgr;
        public LogOutputSocketProtocolMgr LogOutputSocketProtocolMgr { get { return m_logOutputSocketProtocolMgr; } }

        private UploadSocketProtocolMgr m_uploadSocketProtocolMgr;
        public UploadSocketProtocolMgr UploadSocketProtocolMgr { get { return m_uploadSocketProtocolMgr; } }



        //----------------------------未看-------------------------------------//

        private DaemonThread m_daemonThread;//检测超时客户端线程

        public AsyncSocketServer(int numConnections)
        {
            
            m_numConnections = numConnections;//设置服务器最大并发访问数
            m_receiveBufferSize = ProtocolConst.ReceiveBufferSize;//设置接收缓存区大小

            m_asyncSocketUserTokenPool = new AsyncSocketUserTokenPool(numConnections);//设置AsyncSocketUserTokenPool缓冲池的大小（栈实现）
            m_asyncSocketUserTokenList = new AsyncSocketUserTokenList();//初始化AsyncSocketUserToken列表
            m_maxNumberAcceptedClients = new Semaphore(numConnections, numConnections);//初始化信号量（有点不太懂）

            //-----------------------未看------------------------------//
            m_logOutputSocketProtocolMgr = new LogOutputSocketProtocolMgr();//日志输出对象
            m_uploadSocketProtocolMgr = new UploadSocketProtocolMgr();//上传
            //-----------------------未看------------------------------//
        }

        /// <summary>
        /// 初始化好多好多的userToken(最基本的读写对象)
        /// </summary>
        public void Init()
        {
            AsyncSocketUserToken userToken;
            for (int i = 0; i < m_numConnections; i++) //按照连接数建立读写对象
            {
                userToken = new AsyncSocketUserToken(m_receiveBufferSize);//初始化userToken
                userToken.id = i;//为每一个连接标号
                userToken.ReceiveEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                userToken.SendEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                m_asyncSocketUserTokenPool.Push(userToken);//将userToken添加到UserTokenPool中
            }
        }

        /// <summary>
        /// 开启服务器，准备接收客户端连接
        /// </summary>
        /// <param name="localEndPoint"></param>
        public void Start(IPEndPoint localEndPoint)
        {
            //绑定操作
            #region
            listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(localEndPoint);
            listenSocket.Listen(m_numConnections);
            Program.Logger.InfoFormat("Start listen socket {0} success", localEndPoint.ToString());
            #endregion
            //for (int i = 0; i < 64; i++) //不能循环投递多次AcceptAsync，会造成只接收8000连接后不接收连接了
            StartAccept(null);//开始接收客户端
            m_daemonThread = new DaemonThread(this);//检测客户端状态，如果连接超时即关闭
        }

        /// <summary>
        /// 接收客户端
        /// </summary>
        /// <param name="acceptEventArgs"></param>
        public void StartAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            if (acceptEventArgs == null)
            {
                acceptEventArgs = new SocketAsyncEventArgs();
                acceptEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);
            }
            else
            {
                acceptEventArgs.AcceptSocket = null; //释放上次绑定的Socket，等待下一个Socket连接
            }

            m_maxNumberAcceptedClients.WaitOne(); //获取信号量
            bool willRaiseEvent = listenSocket.AcceptAsync(acceptEventArgs);//异步接收客户端 如果acceptEventArgs为空，则执行AcceptEventArg_Completed;
                                                                            //               如果acceptEventArgs不为空，则执行acceptEventArgs绑定的事件(AcceptEventArg_Completed)                                                        )

            //如果I/O同步完成，不会触发AcceptEventArg_Completed，手动触发ProcessAccept
            if (!willRaiseEvent)
            {
                ProcessAccept(acceptEventArgs);
            }
        }

        /// <summary>
        /// 接StartAccept()
        ///  </summary>
        /// <param name="sender"></param>
        /// <param name="acceptEventArgs"></param>
        void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs acceptEventArgs)
        {
            try
            {
                ProcessAccept(acceptEventArgs);
            }
            catch (Exception E)
            {
                Program.Logger.ErrorFormat("Accept client {0} error, message: {1}", acceptEventArgs.AcceptSocket, E.Message);
                Program.Logger.Error(E.StackTrace);  
            }            
        }

        /// <summary>
        /// 开始接收客户端
        /// </summary>
        /// <param name="acceptEventArgs"></param>
        private void ProcessAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            Program.Logger.InfoFormat("Client connection accepted. Local Address: {0}, Remote Address: {1}",
                acceptEventArgs.AcceptSocket.LocalEndPoint, acceptEventArgs.AcceptSocket.RemoteEndPoint);

            AsyncSocketUserToken userToken = m_asyncSocketUserTokenPool.Pop();//获取到一个AsyncSocketUserToken
                                                                              //包含异步接收、发送事件、接收/发送缓冲区
            m_asyncSocketUserTokenList.Add(userToken); //添加到正在连接列表
            userToken.ConnectSocket = acceptEventArgs.AcceptSocket;//将接收到的客户端绑定到userToken中的m_receiveEventArgs、m_sendEventArgs
            userToken.ConnectDateTime = DateTime.Now;//客户端连接服务器的时间

            try
            {
                bool willRaiseEvent = userToken.ConnectSocket.ReceiveAsync(userToken.ReceiveEventArgs); //投递接收请求
                                                                                                        //在Init()函数中对ReceiveEventArgs、SendEventArgs.completed+=IO_Completed;
                if (!willRaiseEvent)
                {
                    lock (userToken)
                    {
                        ProcessReceive(userToken.ReceiveEventArgs);//接收数据
                    }
                }                    
            }
            catch (Exception E)
            {
                Program.Logger.ErrorFormat("Accept client {0} error, message: {1}", userToken.ConnectSocket, E.Message);
                Program.Logger.Error(E.StackTrace);                
            }            

            StartAccept(acceptEventArgs); //把当前异步事件释放，等待下次连接
                                          //这里的调用acceptEventArgs绑定的都为AcceptEventArg_Completed
        }

        /// <summary>
        /// SendEventArgs、ReceiveEventArgs有操作并且操作完成后触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="asyncEventArgs"></param>
        void IO_Completed(object sender, SocketAsyncEventArgs asyncEventArgs)
        {
            AsyncSocketUserToken userToken = asyncEventArgs.UserToken as AsyncSocketUserToken;

            userToken.ActiveDateTime = DateTime.Now;//客户端最后活跃时间
            try
            {                
                lock (userToken)
                {
                    if (asyncEventArgs.LastOperation == SocketAsyncOperation.Receive)
                        ProcessReceive(asyncEventArgs);
                    else if (asyncEventArgs.LastOperation == SocketAsyncOperation.Send)
                        ProcessSend(asyncEventArgs);
                    else
                        throw new ArgumentException("The last operation completed on the socket was not a receive or send");
                }   
            }
            catch (Exception E)
            {
                Program.Logger.ErrorFormat("IO_Completed {0} error, message: {1}", userToken.ConnectSocket, E.Message);
                Program.Logger.Error(E.StackTrace);
            }                     
        }

        /// <summary>
        /// 数据接收函数（具体接收方法）
        /// </summary>
        /// <param name="receiveEventArgs"></param>
        private void ProcessReceive(SocketAsyncEventArgs receiveEventArgs)
        {
            AsyncSocketUserToken userToken = receiveEventArgs.UserToken as AsyncSocketUserToken;
            if (userToken.ConnectSocket == null)
                return;
            userToken.ActiveDateTime = DateTime.Now;//客户端最后活跃时间
            if (userToken.ReceiveEventArgs.BytesTransferred > 0 && userToken.ReceiveEventArgs.SocketError == SocketError.Success)
            {
                int offset = userToken.ReceiveEventArgs.Offset;// Buffer属性中的数据的起始字节偏移量
                int count = userToken.ReceiveEventArgs.BytesTransferred;//获取在套接字操作中传输的字节数
                if ((userToken.AsyncSocketInvokeElement == null) & (userToken.ConnectSocket != null)) //存在Socket对象，并且没有绑定协议对象，则进行协议对象绑定
                {
                    BuildingSocketInvokeElement(userToken);//根据第一个字节绑定相关协议
                    offset = offset + 1;
                    count = count - 1;
                }
                //绑定协议失败，即收到非法数据包
                if (userToken.AsyncSocketInvokeElement == null) //如果没有解析对象，提示非法连接并关闭连接
                {
                    Program.Logger.WarnFormat("Illegal client connection. Local Address: {0}, Remote Address: {1}", userToken.ConnectSocket.LocalEndPoint, 
                        userToken.ConnectSocket.RemoteEndPoint);
                    CloseClientSocket(userToken);
                }
                //成功绑定协议，开始接收数据
                else
                {
                    if (count > 0) //处理接收数据
                    {
                        if (!userToken.AsyncSocketInvokeElement.ProcessReceive(userToken.ReceiveEventArgs.Buffer, offset, count))
                        { //如果处理数据返回失败，则断开连接
                            CloseClientSocket(userToken);
                        }
                        else //否则投递下次介绍数据请求
                        {
                            bool willRaiseEvent = userToken.ConnectSocket.ReceiveAsync(userToken.ReceiveEventArgs); //投递接收请求
                            if (!willRaiseEvent)
                                ProcessReceive(userToken.ReceiveEventArgs);
                        }
                    }
                    //ProtocolFlag已经发送到，但是数据包还没有发送到服务器
                    else
                    {
                        bool willRaiseEvent = userToken.ConnectSocket.ReceiveAsync(userToken.ReceiveEventArgs); //投递接收请求
                        if (!willRaiseEvent)
                            ProcessReceive(userToken.ReceiveEventArgs);
                    }
                }
            }
            else
            {
                CloseClientSocket(userToken);
            }
        }

        /// <summary>
        /// 绑数据协议
        /// </summary>
        /// <param name="userToken"></param>
        private void BuildingSocketInvokeElement(AsyncSocketUserToken userToken)
        {
            byte flag = userToken.ReceiveEventArgs.Buffer[userToken.ReceiveEventArgs.Offset];
            if (flag == (byte)ProtocolFlag.Upload)
                userToken.AsyncSocketInvokeElement = new UploadSocketProtocol(this, userToken);
            else if (flag == (byte)ProtocolFlag.Control)
                userToken.AsyncSocketInvokeElement = new ControlSocketProtocol(this, userToken);
            else if (flag == (byte)ProtocolFlag.LogOutput)
                userToken.AsyncSocketInvokeElement = new LogOutputSocketProtocol(this, userToken);
            if (userToken.AsyncSocketInvokeElement != null)
            {
                Program.Logger.InfoFormat("Building socket invoke element {0}.Local Address: {1}, Remote Address: {2}",
                    userToken.AsyncSocketInvokeElement, userToken.ConnectSocket.LocalEndPoint, userToken.ConnectSocket.RemoteEndPoint);
            } 
        }

        /// <summary>
        /// 数据发送（具体发送方法）
        /// </summary>
        /// <param name="sendEventArgs"></param>
        /// <returns></returns>
        private bool ProcessSend(SocketAsyncEventArgs sendEventArgs)
        {
            AsyncSocketUserToken userToken = sendEventArgs.UserToken as AsyncSocketUserToken;
            if (userToken.AsyncSocketInvokeElement == null)
                return false;
            userToken.ActiveDateTime = DateTime.Now;//连接活跃时间
            if (sendEventArgs.SocketError == SocketError.Success)
                return userToken.AsyncSocketInvokeElement.SendCompleted(); //调用子类回调函数
            else
            {
                CloseClientSocket(userToken);
                return false;
            }
        }

        //异步发送事件，在AsyncSocketInvokeElement.SendCompleted()中进行了调用
        public bool SendAsyncEvent(Socket connectSocket, SocketAsyncEventArgs sendEventArgs, byte[] buffer, int offset, int count)
        {
            if (connectSocket == null)
                return false;
            sendEventArgs.SetBuffer(buffer, offset, count);
            bool willRaiseEvent = connectSocket.SendAsync(sendEventArgs);
            if (!willRaiseEvent)
            {
                return ProcessSend(sendEventArgs);
            }
            else
                return true;
        }

        /// <summary>
        /// 关闭客户端连接
        /// </summary>
        /// <param name="userToken"></param>
        public void CloseClientSocket(AsyncSocketUserToken userToken)
        {
            if (userToken.ConnectSocket == null)
                return;
            string socketInfo = string.Format("Local Address: {0} Remote Address: {1}", userToken.ConnectSocket.LocalEndPoint,
                userToken.ConnectSocket.RemoteEndPoint);
            Program.Logger.InfoFormat("Client connection disconnected. {0}", socketInfo);
            try
            {
                userToken.ConnectSocket.Shutdown(SocketShutdown.Both);//禁用某 Socket 上的发送和接收
            }
            catch (Exception E) 
            {
                Program.Logger.ErrorFormat("CloseClientSocket Disconnect client {0} error, message: {1}", socketInfo, E.Message);
            }

            lock (UploadSocketProtocol.bsTree)
            {
                UploadSocketProtocol.bsTree.DropCar(userToken.id); //从道路树中删除该道路
            }
            userToken.ConnectSocket.Close();//关闭 Socket 连接并释放所有关联的资源
            userToken.ConnectSocket = null; //释放引用，并清理缓存，包括释放协议对象等资源

            m_maxNumberAcceptedClients.Release();//信号量，增加一个可用连接
            m_asyncSocketUserTokenPool.Push(userToken);//将这个不用的userToken放入asyncSocketUserTokenPool池中
            m_asyncSocketUserTokenList.Remove(userToken);//从连接到服务器的客户端列表中删除该客户端连接
            
        }
    }
}
