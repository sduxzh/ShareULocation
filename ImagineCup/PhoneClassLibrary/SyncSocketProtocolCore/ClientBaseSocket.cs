using System;
using AsyncSocketServer;
using ClientClassLibrary.SyncSocketCore;
using System.Net.Sockets;

namespace ClientClassLibrary.SyncSocketProtocolCore
{
    public class ClientBaseSocket : SyncSocketInvokeElement
    {
        protected string m_errorString;
        public string ErrorString { get { return m_errorString; } }
        protected string m_userName;//用户名
        protected string m_password;//用户密码

        public ClientBaseSocket()
            : base()
        {
        }

        /// <summary>
        /// 检查是否有错误信息
        /// </summary>
        /// <returns></returns>
        public bool CheckErrorCode()
        {
            int errorCode = 0;
            m_incomingDataParser.GetValue(ProtocolKey.Code, ref errorCode);
            if (errorCode == ProtocolCode.Success)
                return true;
            else
            {
                m_errorString = ProtocolCode.GetErrorCodeString(errorCode);
                return false;
            }
        }

        /// <summary>
        /// 证明客户端处于活跃状态
        /// </summary>
        /// <returns></returns>
        public bool DoActive()
        {
            try
            {
                m_outgoingDataAssembler.Clear();
                m_outgoingDataAssembler.AddRequest();
                m_outgoingDataAssembler.AddCommand(ProtocolKey.Active);
                SendCommand();
                bool bSuccess = RecvCommand();
                if (bSuccess)
                    return CheckErrorCode();
                else
                    return false;
            }
            catch (Exception E)
            {
                //记录日志
                m_errorString = E.Message; 
                return false;
            }
        }

        /// <summary>
        /// 客户端登陆函数
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool DoLogin(string userName, string password)
        {
            try
            {
                m_outgoingDataAssembler.Clear();
                m_outgoingDataAssembler.AddRequest();
                m_outgoingDataAssembler.AddCommand(ProtocolKey.Login);
                m_outgoingDataAssembler.AddValue(ProtocolKey.UserName, userName);
                m_outgoingDataAssembler.AddValue(ProtocolKey.Password, BasicFunc.MD5String(password));
                SendCommand();
                bool bSuccess = RecvCommand();
                if (bSuccess)
                {
                    bSuccess = CheckErrorCode();
                    if (bSuccess)
                    {
                        m_userName = userName;
                        m_password = password;
                    }
                    return bSuccess;
                }
                else
                    return false;
            }
            catch (Exception E)
            {
                //记录日志
                m_errorString = E.Message;
                return false;
            }
            return true;
        }

        /// <summary>
        /// 判断客户端是否需要重连
        /// 如果断开，自动重新连接
        /// </summary>
        /// <returns></returns>
        public bool ReConnect()
        {
            if (m_tcpClient.Connected && (!DoActive()))
                return true;
            else
            {
                if (!m_tcpClient.Connected)
                {
                    try
                    {
                        Connect(m_host, m_port);
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
                else
                    return true;
            }
        }

        /// <summary>
        /// 判断客户端是否需要重连
        /// 如果断开，自动重新连接并登陆
        /// </summary>
        /// <returns></returns>
        public bool ReConnectAndLogin()
        {
            if (m_tcpClient.Connected && (!DoActive()))
                return true;
            else
            {
                if (!m_tcpClient.Connected)
                {
                    try
                    {
                        Disconnect();//关闭连接，创建新的TcpCliet
                        Connect(m_host, m_port);//连接服务器，把ProtocolFlag发送到了服务器
                        return DoLogin(m_userName, m_password);//登陆服务器
                    }
                    catch (Exception E)
                    {
                        return false;
                    }
                }
                else
                    return true;
            }
        }
    }
}
