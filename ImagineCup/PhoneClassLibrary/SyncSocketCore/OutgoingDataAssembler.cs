using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//协议组装器
namespace AsyncSocketServer
{
    public class OutgoingDataAssembler
    {
        private List<string> m_protocolText;

        public OutgoingDataAssembler()
        {
            m_protocolText = new List<string>();
        }

        /// <summary>
        /// 清空m_protocolText
        /// </summary>
        public void Clear()
        {
            m_protocolText.Clear();
        }

        /// <summary>
        /// 组装（获取）协议字符串
        /// </summary>
        /// <returns></returns>
        public string GetProtocolText()
        {
            string tmpStr = "";
            if (m_protocolText.Count > 0)
            {
                tmpStr = m_protocolText[0];
                for (int i = 1; i < m_protocolText.Count; i++)
                {
                    tmpStr = tmpStr + ProtocolKey.ReturnWrap + m_protocolText[i];
                }
            }
            return tmpStr;
        }
        /// <summary>
        /// 添加请求字段 "[Request]"
        /// </summary>
        public void AddRequest()
        {
            m_protocolText.Add(ProtocolKey.LeftBrackets + ProtocolKey.Request + ProtocolKey.RightBrackets);
        }
        /// <summary>
        /// 添加回应字段 "[Response]"
        /// </summary>
        public void AddResponse()
        {
            m_protocolText.Add(ProtocolKey.LeftBrackets + ProtocolKey.Response + ProtocolKey.RightBrackets);
        }

        /// <summary>
        /// 添加命令字段（Command=commandKey）
        /// </summary>
        /// <param name="commandKey"></param>
        public void AddCommand(string commandKey)
        {
            m_protocolText.Add(ProtocolKey.Command + ProtocolKey.EqualSign + commandKey);
        }

        /// <summary>
        /// 不太清楚作用
        /// 貌似是一种状态标识，很有可能是心跳包的时候调用
        /// Code=0x00000000
        /// </summary>
        public void AddSuccess()
        {
            m_protocolText.Add(ProtocolKey.Code + ProtocolKey.EqualSign + ProtocolCode.Success.ToString());
        }

        /// <summary>
        /// 不太清楚作用
        /// Code=errorCode
        /// Message=message
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="message"></param>
        public void AddFailure(int errorCode, string message)
        {
            m_protocolText.Add(ProtocolKey.Code + ProtocolKey.EqualSign + errorCode.ToString());
            m_protocolText.Add(ProtocolKey.Message + ProtocolKey.EqualSign + message);
        }

        /// <summary>
        /// 向protocolText（List）中添加自定义内容
        /// </summary>
        /// <param name="protocolKey"></param>
        /// <param name="value"></param>
        public void AddValue(string protocolKey, string value)
        {
            m_protocolText.Add(protocolKey + ProtocolKey.EqualSign + value);
        }

        public void AddValue(string protocolKey, short value)
        {
            m_protocolText.Add(protocolKey + ProtocolKey.EqualSign + value.ToString());
        }

        public void AddValue(string protocolKey, int value)
        {
            m_protocolText.Add(protocolKey + ProtocolKey.EqualSign + value.ToString());
        }

        public void AddValue(string protocolKey, long value)
        {
            m_protocolText.Add(protocolKey + ProtocolKey.EqualSign + value.ToString());
        }

        public void AddValue(string protocolKey, Single value)
        {
            m_protocolText.Add(protocolKey + ProtocolKey.EqualSign + value.ToString());
        }

        public void AddValue(string protocolKey, double value)
        {
            m_protocolText.Add(protocolKey + ProtocolKey.EqualSign + value.ToString());
        }
    }
}
