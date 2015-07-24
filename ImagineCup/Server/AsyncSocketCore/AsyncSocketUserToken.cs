using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace AsyncSocketServer
{
    public class AsyncSocketUserToken
    {
        protected SocketAsyncEventArgs m_receiveEventArgs;//异步接收事件
        public SocketAsyncEventArgs ReceiveEventArgs { get { return m_receiveEventArgs; } set { m_receiveEventArgs = value; } }
        protected byte[] m_asyncReceiveBuffer;//异步接收缓冲区
        protected SocketAsyncEventArgs m_sendEventArgs;//异步发送事件
        public SocketAsyncEventArgs SendEventArgs { get { return m_sendEventArgs; } set { m_sendEventArgs = value; } }

        protected DynamicBufferManager m_receiveBuffer;//接收缓冲区
        public DynamicBufferManager ReceiveBuffer { get { return m_receiveBuffer; } set { m_receiveBuffer = value; } }
        protected AsyncSendBufferManager m_sendBuffer;//发送缓冲区
        public AsyncSendBufferManager SendBuffer { get { return m_sendBuffer; } set { m_sendBuffer = value; } }

        protected AsyncSocketInvokeElement m_asyncSocketInvokeElement; //协议对象
        public AsyncSocketInvokeElement AsyncSocketInvokeElement { get { return m_asyncSocketInvokeElement; } set { m_asyncSocketInvokeElement = value; } }

        protected Socket m_connectSocket;
        public int id;//标识用户身份

        //AsyncSocketServer.ProcessAccept()对其赋值
        public Socket ConnectSocket
        {
            get
            {
                return m_connectSocket;
            }
            set
            {
                m_connectSocket = value;
                if (m_connectSocket == null) //清理缓存
                {
                    if (m_asyncSocketInvokeElement != null)
                        m_asyncSocketInvokeElement.Close();
                    m_receiveBuffer.Clear(m_receiveBuffer.DataCount);
                    m_sendBuffer.ClearPacket();
                }
                m_asyncSocketInvokeElement = null;                
                m_receiveEventArgs.AcceptSocket = m_connectSocket;
                m_sendEventArgs.AcceptSocket = m_connectSocket;
            }
        }

        protected DateTime m_ConnectDateTime;//连接到服务器的时间
        public DateTime ConnectDateTime { get { return m_ConnectDateTime; } set { m_ConnectDateTime = value; } }
        protected DateTime m_ActiveDateTime;//上一次活跃的时间
        public DateTime ActiveDateTime { get { return m_ActiveDateTime; } set { m_ActiveDateTime = value; } }

        public AsyncSocketUserToken(int asyncReceiveBufferSize)
        {
            m_connectSocket = null;//清空connectSocket
            m_asyncSocketInvokeElement = null;//清空数据传输协议
            m_receiveEventArgs = new SocketAsyncEventArgs();//新建接收SocketAsyncEventArgs事件
            m_receiveEventArgs.UserToken = this;//指定UserToken
            m_asyncReceiveBuffer = new byte[asyncReceiveBufferSize];//设置接收缓冲区大小
            m_receiveEventArgs.SetBuffer(m_asyncReceiveBuffer, 0, m_asyncReceiveBuffer.Length);//设置异步套接字方法的数据缓冲区

            m_sendEventArgs = new SocketAsyncEventArgs();//新建发送SocketAsyncEventArgs事件
            m_sendEventArgs.UserToken = this;//指定UserToken
            m_receiveBuffer = new DynamicBufferManager(ProtocolConst.InitBufferSize);//动态buffer管理对象（接收）
            m_sendBuffer = new AsyncSendBufferManager(ProtocolConst.InitBufferSize);//动态buffer管理对象（发送）

        }
    }
}
