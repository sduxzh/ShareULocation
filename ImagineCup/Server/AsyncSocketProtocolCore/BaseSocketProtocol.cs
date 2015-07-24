using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace AsyncSocketServer
{
    public class BaseSocketProtocol : AsyncSocketInvokeElement
    {
        protected string m_userName;//用户用户名
        public string UserName { get { return m_userName; } }
        protected bool m_logined;
        public bool Logined { get { return m_logined; } }
        protected string m_socketFlag;
        public string SocketFlag { get { return m_socketFlag; } }

        public BaseSocketProtocol(AsyncSocketServer asyncSocketServer, AsyncSocketUserToken asyncSocketUserToken)
            : base(asyncSocketServer, asyncSocketUserToken)
        {
            m_userName = "";
            m_logined = false;
            m_socketFlag = "";
        }

        


        /// <summary>
        /// 判断客户端是否登陆成功
        /// </summary>
        /// <returns></returns>
        public bool DoLogin()
        {
            string userName = "";//存储用户名
            string password = "";//用户密码
            if (m_incomingDataParser.GetValue(ProtocolKey.UserName, ref userName) & m_incomingDataParser.GetValue(ProtocolKey.Password, ref password))
            {
                if (password.Equals(BasicFunc.MD5String("admin"), StringComparison.CurrentCultureIgnoreCase))
                {
                    m_outgoingDataAssembler.AddSuccess();//添加登陆成功的协议字段信息
                    m_userName = userName;//用户名
                    m_logined = true;//用户登录成功
                    Program.Logger.InfoFormat("{0} login success", userName);
                    
                }
                else
                {
                    m_outgoingDataAssembler.AddFailure(ProtocolCode.UserOrPasswordError, "");//添加登陆失败的协议字段信息
                    Program.Logger.ErrorFormat("{0} login failure,password error", userName);
                }
            }
            else
                m_outgoingDataAssembler.AddFailure(ProtocolCode.ParameterError, "");
            return DoSendResult();//将刚刚的登陆信息发送到客户端
        }

        /// <summary>
        /// 检测客户端是否活跃
        /// </summary>
        /// <returns></returns>
        public bool DoActive()
        {
            m_outgoingDataAssembler.AddSuccess();//在m_protocolText列表中添加了"code= 0x00000000"
            return DoSendResult();//发送心跳包
        }
    }
}
