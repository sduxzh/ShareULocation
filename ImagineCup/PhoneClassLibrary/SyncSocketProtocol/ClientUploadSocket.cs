using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Controls;
using AsyncSocketServer;
using ClientClassLibrary.SyncSocketProtocolCore;

namespace ClientClassLibrary.SyncSocketProtocol
{
    class ClientUploadSocket : ClientBaseSocket
    {
        public ClientUploadSocket()
            : base()
        {
            m_protocolFlag = ProtocolFlag.Upload;//设置协议标识符
        }

        /// <summary>
        /// 向服务器发送注册信息
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="mail"></param>
        public bool DoRegister(string username, string password, string mail)
        {
            try
            {
                m_outgoingDataAssembler.Clear();
                m_outgoingDataAssembler.AddRequest();
                m_outgoingDataAssembler.AddCommand(ProtocolKey.Register);
                m_outgoingDataAssembler.AddValue(ProtocolKey.UserName,username );
                m_outgoingDataAssembler.AddValue(ProtocolKey.Password, password);
                m_outgoingDataAssembler.AddValue(ProtocolKey.Mail,mail);
                SendCommand();
                Thread.Sleep(1000);
                bool bSuccess = RecvCommand();
                if (bSuccess)
                {
                    bSuccess = CheckErrorCode();
                    if (bSuccess)
                    {
                        Debug.WriteLine("注册成功");
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
        }

        /// <summary>
        /// 向服务器端发送地理位置
        /// </summary>
        /// <returns></returns>
        public void DoSendLocation(string roadid,string roadname,string lat, string lon)
        {
            try
            {
                m_outgoingDataAssembler.Clear();
                m_outgoingDataAssembler.AddRequest();
                m_outgoingDataAssembler.AddCommand(ProtocolKey.SendLocation);
                m_outgoingDataAssembler.AddValue(ProtocolKey.RoadId, roadid);
                m_outgoingDataAssembler.AddValue(ProtocolKey.RoadName, roadname);
                m_outgoingDataAssembler.AddValue(ProtocolKey.Lat, lat);
                m_outgoingDataAssembler.AddValue(ProtocolKey.Lon, lon);
                SendCommand();
            }
            catch (Exception E)
            {
                //记录日志
                m_errorString = E.Message;
            }
        }

        /// <summary>
        /// 查询道路状态
        /// </summary>
        /// <param name="roadname"></param>
        /// <returns></returns>
        public bool DoAcquireRoadState(string roadname)
        {
            try
            {
                m_outgoingDataAssembler.Clear();
                m_outgoingDataAssembler.AddRequest();
                m_outgoingDataAssembler.AddCommand(ProtocolKey.AcquireRoadState);
                m_outgoingDataAssembler.AddValue(ProtocolKey.RoadName, roadname);
                SendCommand();
                Thread.Sleep(50);
                bool bSuccess = RecvCommand();
                if (bSuccess)
                {
                    bSuccess = CheckErrorCode();
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
        }
        
        

        /// <summary>
        /// 不太懂，看看有没有具体的调用
        /// </summary>
        /// <param name="fileSize"></param>
        /// <returns></returns>
        public bool DoEof(Int64 fileSize)
        {
            try
            {
                m_outgoingDataAssembler.Clear();
                m_outgoingDataAssembler.AddRequest();
                m_outgoingDataAssembler.AddCommand(ProtocolKey.Eof);
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
    }
}
