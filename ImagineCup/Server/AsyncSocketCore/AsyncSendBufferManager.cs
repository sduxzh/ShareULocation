using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//完成
//异步发送列表是在动态缓存的基础上加了一个列表管理，记录每个包的位置信息，并提供管理函数
namespace AsyncSocketServer
{
    struct SendBufferPacket
    {
        public int Offset;
        public int Count;
    }

    //由于是异步发送，有可能接收到两个命令，写入了两次返回，发送需要等待上一次回调才发下一次的响应
    public class AsyncSendBufferManager
    {
        private DynamicBufferManager m_dynamicBufferManager;//动态buffer管理器
        public DynamicBufferManager DynamicBufferManager { get { return m_dynamicBufferManager; } }
        private List<SendBufferPacket> m_sendBufferList;//发送包列表
        private SendBufferPacket m_sendBufferPacket;//当前的发送包

        public AsyncSendBufferManager(int bufferSize)
        {
            m_dynamicBufferManager = new DynamicBufferManager(bufferSize);//初始化m_dynamicBufferManager
                                                                          //设置缓冲区buffer的大小
            m_sendBufferList = new List<SendBufferPacket>();//初始化发送包列表
            m_sendBufferPacket.Offset = 0;//当前发送包的偏移量
            m_sendBufferPacket.Count = 0;//当前发送包的长度
        }

        public void StartPacket()
        {
            m_sendBufferPacket.Offset = m_dynamicBufferManager.DataCount;//设置发送包在缓冲区buffer中的偏移量
            m_sendBufferPacket.Count = 0;
        }

        public void EndPacket()
        {
            m_sendBufferPacket.Count = m_dynamicBufferManager.DataCount - m_sendBufferPacket.Offset;//获取发送包的长度
            m_sendBufferList.Add(m_sendBufferPacket);//将当前发送包添加到发送包列表
        }

        /// <summary>
        /// 获取发送包列表中的第一个包
        /// 修改当前包的offset、count值
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public bool GetFirstPacket(ref int offset, ref int count)
        {
            if (m_sendBufferList.Count <= 0)
                return false;
            offset = 0;//m_sendBufferList[0].Offset;清除了第一个包后，后续的包往前移，因此Offset都为0
            count = m_sendBufferList[0].Count;
            return true;
        }

        /// <summary>
        /// 删除列表中的第一个包
        /// </summary>
        /// <returns></returns>
        public bool ClearFirstPacket()
        {
            if (m_sendBufferList.Count <= 0)
                return false;
            int count = m_sendBufferList[0].Count;
            m_dynamicBufferManager.Clear(count);
            m_sendBufferList.RemoveAt(0);
            return true;
        }

        /// <summary>
        /// 删除列表中的所有发送包
        /// </summary>
        public void ClearPacket()
        {
            m_sendBufferList.Clear();
            m_dynamicBufferManager.Clear(m_dynamicBufferManager.DataCount);
        }
    }
}
