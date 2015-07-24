using System;
using System.Text;
using System.Net.Sockets;
using AsyncSocketServer;
using System.Net;
using System.Threading;
using System.Diagnostics;
using System;
using System.Security.Cryptography;


namespace ClientClassLibrary.SyncSocketCore
{
    public class SyncSocketInvokeElement
    {
        protected Socket m_tcpClient;
        static ManualResetEvent _myEvent = new ManualResetEvent(false);//控制线程通信
        private int Timeout = 8000;//线程等待时间
        protected string m_host;
        protected int m_port;
        protected ProtocolFlag m_protocolFlag;
        private bool m_netByteOrder;
        public bool NetByteOrder { get { return m_netByteOrder; } set { m_netByteOrder = value; } } //长度是否使用网络字节顺序
        protected OutgoingDataAssembler m_outgoingDataAssembler; //协议组装器，用来组装往外发送的命令
        protected DynamicBufferManager m_recvBuffer; //接收数据的缓存
        protected IncomingDataParser m_incomingDataParser; //收到数据的解析器，用于解析返回的内容
        protected DynamicBufferManager m_sendBuffer; //发送数据的缓存，统一写到内存中，调用一次发送

        public SyncSocketInvokeElement()
        {
            m_tcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_protocolFlag = ProtocolFlag.None;
            m_outgoingDataAssembler = new OutgoingDataAssembler();
            m_recvBuffer = new DynamicBufferManager(ProtocolConst.ReceiveBufferSize);
            m_incomingDataParser = new IncomingDataParser();
            m_sendBuffer = new DynamicBufferManager(ProtocolConst.ReceiveBufferSize);
            
        }

        public void Connect(string host, int port)
        {
            DnsEndPoint endPoint = new DnsEndPoint(host, port);
            SocketAsyncEventArgs asEventArg = new SocketAsyncEventArgs();
            asEventArg.RemoteEndPoint = endPoint;
            _myEvent.Reset();
            asEventArg.Completed += (object sender, SocketAsyncEventArgs arg) =>
            {
                if (arg.SocketError == SocketError.Success)
                {
                    Debug.WriteLine("Connect with Server");
                    Debug.WriteLine("Start to send SocketFlag");
                    m_host = host;
                    m_port = port;
                    byte[] socketFlag = new byte[1];
                    socketFlag[0] = (byte)m_protocolFlag;
                    SendSocketFlag(socketFlag);
                    Debug.WriteLine("SocketFlag has sent");
                                      
                }
                else
                {
                    Debug.WriteLine("Did not connect with Server... \nThe Reson is {0}",arg.SocketError.ToString());
                }
                _myEvent.Set();
            };
            m_tcpClient.ConnectAsync(asEventArg);
            _myEvent.WaitOne(Timeout);
            
        }

        /// <summary>
        /// 检测是否连接到服务器
        /// </summary>
        /// <returns></returns>
        public  bool ConnectState()
        {
            return m_tcpClient.Connected;
        }

        /// <summary>
        /// 向服务器发送SocketFlag
        /// 此标志必须发送，否则会造成协议绑定失败，断开服务器连接
        /// </summary>
        /// <param name="flagByte"></param>
        private void SendSocketFlag(byte[] flagByte)
        {
            if (m_tcpClient != null && m_tcpClient.Connected)
            {
                bool iscompleted = false;
                SocketAsyncEventArgs asyArg = new SocketAsyncEventArgs();
                asyArg.SetBuffer(flagByte, 0, flagByte.Length);//设置发送缓冲区
                asyArg.Completed += (object sender, SocketAsyncEventArgs arg) =>
                {
                    if (arg.SocketError == SocketError.Success)
                    {
                        iscompleted = true;
                        //待定
                    }
                    else
                    {
                        //重新发送数据

                    }
                    
                };
                m_tcpClient.SendAsync(asyArg);

                while (true)
                {
                    if (iscompleted)
                        break;
                }
            }
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Disconnect()
        {
            m_tcpClient.Close();
            m_tcpClient = new  Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //m_tcpClient.Client.
        }


        /// <summary>
        /// 发送数据至服务器
        /// </summary>
        public void SendCommand()
        {
            if (m_tcpClient != null && m_tcpClient.Connected)
            {
                bool isCompleted = false;
                string commandText = m_outgoingDataAssembler.GetProtocolText();
                byte[] bufferUTF8 = Encoding.UTF8.GetBytes(commandText);
                int totalLength = sizeof (int) + bufferUTF8.Length; //获取总大小
                m_sendBuffer.Clear(); //清空发送缓冲区
                m_sendBuffer.WriteInt(totalLength, false); //写入总大小
                m_sendBuffer.WriteInt(bufferUTF8.Length, false); //写入命令大小
                m_sendBuffer.WriteBuffer(bufferUTF8); //写入命令内容
                SocketAsyncEventArgs asyArg = new SocketAsyncEventArgs();
                asyArg.SetBuffer(m_sendBuffer.Buffer, 0, m_sendBuffer.DataCount);
                asyArg.Completed += (object sender, SocketAsyncEventArgs arg) =>
                {
                    if (arg.SocketError == SocketError.Success)
                    {
                        isCompleted = true;
                        //待定
                    }
                    else
                    {
                        //重新发送数据

                    }
                    //_myEvent.Set();
                };
                //_myEvent.Reset();
                m_tcpClient.SendAsync(asyArg);
                while (true)
                {
                    if(isCompleted)
                        break;
                }
            }
        }


        /// <summary>
        /// 重载，先不管
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public void SendCommand(byte[] buffer, int offset, int count)
        {
            if (m_tcpClient != null && m_tcpClient.Connected)
            {
                string commandText = m_outgoingDataAssembler.GetProtocolText();
                byte[] bufferUTF8 = Encoding.UTF8.GetBytes(commandText);
                int totalLength = sizeof(int) + bufferUTF8.Length + count; //获取总大小
                m_sendBuffer.Clear();
                m_sendBuffer.WriteInt(totalLength, false); //写入总大小
                m_sendBuffer.WriteInt(bufferUTF8.Length, false); //写入命令大小
                m_sendBuffer.WriteBuffer(bufferUTF8); //写入命令内容
                m_sendBuffer.WriteBuffer(buffer, offset, count); //写入二进制数据

                SocketAsyncEventArgs asyArg = new SocketAsyncEventArgs();
                asyArg.SetBuffer(m_sendBuffer.Buffer, 0, m_sendBuffer.DataCount);
                asyArg.Completed += (object sender, SocketAsyncEventArgs arg) =>
                {
                    if (arg.SocketError == SocketError.Success)
                    {
                        Debug.WriteLine("socket flag has sent successfully");
                    }
                    else
                    {
                        //重新发送数据

                    }
                    //_myEvent.Set();
                };
                //_myEvent.Reset();
                m_tcpClient.SendAsync(asyArg);
            }
            
        }

        /// <summary>
        /// 接收服务器端的反馈信息
        /// </summary>
        /// <returns></returns>
        public bool RecvCommand()
        {
            int packetLength=0;
            bool completed = false;
            m_recvBuffer.Clear();//清空接收缓冲区
            SocketAsyncEventArgs asyArg = new SocketAsyncEventArgs();
            asyArg.SetBuffer(m_recvBuffer.Buffer, 0, sizeof(int));
            asyArg.Completed += (object sender, SocketAsyncEventArgs arg) =>
                {
                    if (arg.SocketError == SocketError.Success)
                    {
                        Debug.WriteLine("socket flag has sent successfully");
                        packetLength = BitConverter.ToInt32(m_recvBuffer.Buffer, 0); //获取包长度
                        if (NetByteOrder)
                            packetLength = System.Net.IPAddress.NetworkToHostOrder(packetLength); //把网络字节顺序转为本地字节顺序
                        m_recvBuffer.SetBufferSize(sizeof(int) + packetLength); //保证接收有足够的空间
                        completed = true;//接收完成


                    }
                    else
                    {
                        //重新发送数据

                    }
                    //_myEvent.Set();
                };
            m_tcpClient.ReceiveAsync(asyArg);//阻塞接收模式
            while (true)
            {
                if (completed)
                {
                    Debug.WriteLine("收到命令长度");
                    return RecvData(m_recvBuffer.Buffer, packetLength);

                }
            }  
        }

        private bool RecvData(byte [] buffer,int packetlength)
        {
            bool completed = false;
            string tmpStr = null;
            SocketAsyncEventArgs asyArg = new SocketAsyncEventArgs();
            asyArg.SetBuffer(m_recvBuffer.Buffer, sizeof(int), packetlength);
            asyArg.Completed += (object sender, SocketAsyncEventArgs arg) =>
            {
                if (arg.SocketError == SocketError.Success)
                {
                    Debug.WriteLine("socket flag has sent successfully");
                    int commandLen = BitConverter.ToInt32(m_recvBuffer.Buffer, sizeof(int)); //取出命令长度
                    tmpStr = Encoding.UTF8.GetString(m_recvBuffer.Buffer, sizeof(int) + sizeof(int), commandLen);
                    completed = true;

                }
                else
                {
                    

                }
                //_myEvent.Set();
            };
            m_tcpClient.ReceiveAsync(asyArg);//阻塞接收模式
            while (true)
            {
                if (completed)
                {
                    Debug.WriteLine("命令字符串：{0}",tmpStr);
                    if (!m_incomingDataParser.DecodeProtocolText(tmpStr)) //解析命令
                        return false;
                    else
                        return true;
                }
            }
        }  

    }
}
