using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//协议解析器
namespace AsyncSocketServer
{
    public class IncomingDataParser
    {
        private string m_header;
        public string Header { get { return m_header; } }
        private string m_command;
        public string Command { get { return m_command; } }
        private List<string> m_names;//命令名称
        public List<string> Names { get { return m_names; } }
        private List<string> m_values;//命令内容
        public List<string> Values { get { return m_values; } }

        public IncomingDataParser()
        {
            //例如进行文件传输，会有文件目录和文件名
            m_names = new List<string>();//命令的其他参量
            m_values = new List<string>();////命令的其他参量
        }

        /// <summary>
        /// 命令解析函数
        /// 将命令存放到Names、Values列表中
        /// </summary>
        /// <param name="protocolText"></param>
        /// <returns></returns>
        public bool DecodeProtocolText(string protocolText)
        {
            m_header = "";
            m_names.Clear();//清空List
            m_values.Clear();//清空List
            int speIndex = protocolText.IndexOf(ProtocolKey.ReturnWrap);//查找协议关键字（换行符）
            if (speIndex < 0)
            {
                return false;
            }
            else
            {
                string[] tmpNameValues = protocolText.Split(new string[] { ProtocolKey.ReturnWrap }, StringSplitOptions.RemoveEmptyEntries);//根据分隔符将字符串（命令）分开
                if (tmpNameValues.Length < 2) //每次命令至少包括两行
                    return false;
                for (int i = 0; i < tmpNameValues.Length; i++)
                {
                    string[] tmpStr = tmpNameValues[i].Split(new string[] { ProtocolKey.EqualSign }, StringSplitOptions.None);
                    if (tmpStr.Length > 1) //存在等号
                    {
                        if (tmpStr.Length > 2) //超过两个等号，返回失败
                            return false;
                        if (tmpStr[0].Equals(ProtocolKey.Command, StringComparison.CurrentCultureIgnoreCase))
                        {
                            m_command = tmpStr[1];//命令的具体内容
                        }
                        else
                        {
                            m_names.Add(tmpStr[0].ToLower());
                            m_values.Add(tmpStr[1]);
                        }
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// 查找谋协议关键字对应的内容
        /// </summary>
        /// <param name="protocolKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool GetValue(string protocolKey, ref string value)
        {
            int index = m_names.IndexOf(protocolKey.ToLower());//寻找该协议关键字所在位置
            if (index > -1)
            {
                value = m_values[index];//返回该协议关键字对应的内容
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// 在协议列表中查找该协议关键字对应的所有内容
        /// </summary>
        /// <param name="protocolKey"></param>
        /// <returns></returns>
        public List<string> GetValue(string protocolKey)
        {
            List<string> result = new List<string>();
            for (int i = 0; i < m_names.Count; i++)
            {
                if (protocolKey.Equals(m_names[i], StringComparison.CurrentCultureIgnoreCase))
                    result.Add(m_values[i]);
            }
            return result;
        }

        /// <summary>
        /// 将协议字符串表示形式转换为它的等效 16 位有符号整数
        /// </summary>
        /// <param name="protocolKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool GetValue(string protocolKey, ref short value)
        {
            int index = m_names.IndexOf(protocolKey.ToLower());
            if (index > -1)
            {
                return short.TryParse(m_values[index], out value);
            }
            else
                return false;
        }

        /// <summary>
        /// 将协议字符串表示形式转换为它的等效 32 位有符号整数
        /// </summary>
        /// <param name="protocolKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool GetValue(string protocolKey, ref int value)
        {
            int index = m_names.IndexOf(protocolKey.ToLower());
            if (index > -1)
                return int.TryParse(m_values[index], out value);
            else
                return false;
        }

        /// <summary>
        /// 将协议字符串表示形式转换为它的等效 64 位有符号整数
        /// </summary>
        /// <param name="protocolKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool GetValue(string protocolKey, ref long value)
        {
            int index = m_names.IndexOf(protocolKey.ToLower());
            if (index > -1)
                return long.TryParse(m_values[index], out value);
            else
                return false;
        }

        /// <summary>
        /// 将协议字符串表示形式转换为它的等效的单精度浮点数字
        /// </summary>
        /// <param name="protocolKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool GetValue(string protocolKey, ref Single value)
        {
            int index = m_names.IndexOf(protocolKey.ToLower());
            if (index > -1)
                return Single.TryParse(m_values[index], out value);
            else
                return false;
        }

        /// <summary>
        /// 将协议字符串表示形式转换为它的等效的双精度浮点数字
        /// </summary>
        /// <param name="protocolKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool GetValue(string protocolKey, ref Double value)
        {
            int index = m_names.IndexOf(protocolKey.ToLower());
            if (index > -1)
                return Double.TryParse(m_values[index], out value);
            else
                return false;
        }
    }
}