using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using AsyncSocketServer.IntelligentPrediction;
using ServerClassLibrary;

namespace AsyncSocketServer
{
    public class UploadSocketProtocol : BaseSocketProtocol
    {
        public static BsTree bsTree=new BsTree();//算法树 
        private SqlConnection connection;//数据库连接符
        public UploadSocketProtocol(AsyncSocketServer asyncSocketServer, AsyncSocketUserToken asyncSocketUserToken)
            : base(asyncSocketServer, asyncSocketUserToken)
        {
            m_socketFlag = "Upload";
            connection = DataBaseOperation.CreateConnection();//获取数据库连接符
            lock (m_asyncSocketServer.UploadSocketProtocolMgr)
            {
                m_asyncSocketServer.UploadSocketProtocolMgr.Add(this);
            }
        }

        public override void Close()
        {
            base.Close();
            lock (m_asyncSocketServer.UploadSocketProtocolMgr)
            {
                m_asyncSocketServer.UploadSocketProtocolMgr.Remove(this);
            }
        }

        public override bool ProcessCommand(byte[] buffer, int offset, int count) //处理分完包的数据，子类从这个方法继承
        {
            UploadSocketCommand command = StrToCommand(m_incomingDataParser.Command);
            m_outgoingDataAssembler.Clear();
            m_outgoingDataAssembler.AddResponse();
            m_outgoingDataAssembler.AddCommand(m_incomingDataParser.Command);
            if (!CheckLogined(command)) //检测登录
            {
                m_outgoingDataAssembler.AddFailure(ProtocolCode.UserHasLogined, "");
                return DoSendResult();
            }

            if (command == UploadSocketCommand.Login)
                return DoLogin();
            else if (command == UploadSocketCommand.Active)
                return DoActive();
            else if (command == UploadSocketCommand.SendLocation)
                return DoSendLocation();
            else if (command == UploadSocketCommand.Register)
                return DoRegister();
            else if (command == UploadSocketCommand.AcquireRoadState)
                return DoAcquireRoadState();
            else if (command == UploadSocketCommand.Tab)
                return Tab();
            else
            {
                Program.Logger.Error("Unknow command: " + m_incomingDataParser.Command);
                return false;
            }
        }

        public UploadSocketCommand StrToCommand(string command)
        {
            if (command.Equals(ProtocolKey.Active, StringComparison.CurrentCultureIgnoreCase))
                return UploadSocketCommand.Active;
            else if (command.Equals(ProtocolKey.Login, StringComparison.CurrentCultureIgnoreCase))
                return UploadSocketCommand.Login;
            else if (command.Equals(ProtocolKey.Eof, StringComparison.CurrentCultureIgnoreCase))
                return UploadSocketCommand.Eof;
            else if(command.Equals(ProtocolKey.SendLocation, StringComparison.CurrentCultureIgnoreCase))
                return UploadSocketCommand.SendLocation;
            else if(command.Equals(ProtocolKey.Register, StringComparison.CurrentCultureIgnoreCase))
                return UploadSocketCommand.Register;
            else if(command.Equals(ProtocolKey.AcquireRoadState, StringComparison.CurrentCultureIgnoreCase))
                return UploadSocketCommand.AcquireRoadState;
            else if(command.Equals(ProtocolKey.Tab, StringComparison.CurrentCultureIgnoreCase))
                return UploadSocketCommand.Tab;
            else
                return UploadSocketCommand.None;
        }

        /// <summary>
        /// 检测客户端是否在线
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public bool CheckLogined(UploadSocketCommand command)
        {
            if ((command == UploadSocketCommand.Login) || (command == UploadSocketCommand.Active) || (command == UploadSocketCommand.Register) ||(command == UploadSocketCommand.SendLocation) || (command == UploadSocketCommand.AcquireRoadState) || (command == UploadSocketCommand.Tab))
                return true;
            else
                return m_logined;
        }

        /// <summary>
        /// 接收客户端发来的地理位置信息
        /// </summary>
        /// <returns></returns>
        public bool DoSendLocation()
        {
            string roadid = "";
            string roadname = "";
            string lat = "";
            string lon = "";
            double speed = 0;

            if (m_incomingDataParser.GetValue(ProtocolKey.Lat, ref lat) &
                m_incomingDataParser.GetValue(ProtocolKey.Lon.ToLower(), ref lon) &
                m_incomingDataParser.GetValue(ProtocolKey.RoadId.ToLower(), ref roadid) &
                m_incomingDataParser.GetValue(ProtocolKey.RoadName.ToLower(), ref roadname) &
                m_incomingDataParser.GetValue(ProtocolKey.Speed.ToLower(), ref speed)) ;
            {
                //lock(bsTree)
                {
                    int num= bsTree.InsertIn(roadid, roadname, AsyncSocketUserToken.id,speed);//添加车辆信息
                    Console.WriteLine("客户端id为：{0} 道路id为：{1} 道路名称为：{2} 经纬度为：{3},{4},速度为：{5}", AsyncSocketUserToken.id, roadid, roadname, lat, lon,speed);
                    Console.WriteLine("道路最大负载量：{0} 道路目前车辆数:{1}" ,num,bsTree.numberOnRoad(roadname,roadid)); 

                }
               
            }
            return true;
        }

        /// <summary>
        /// 接收用户注册信息
        /// </summary>
        /// <returns></returns>
        public bool DoRegister()
        {
            string username = null;
            string password = null;
            string mail = null;

            if (m_incomingDataParser.GetValue(ProtocolKey.UserName, ref username) &
                m_incomingDataParser.GetValue(ProtocolKey.Password.ToLower(), ref password) &
                m_incomingDataParser.GetValue(ProtocolKey.Mail.ToLower(), ref mail))
            {
                Console.WriteLine("用户名：{0} 密码：{1} 邮箱：{2}",username,password,mail);
                string queryCmdText = "select * from UserInformation where UserName='" + username + "'";
                string reviseCmdText = "insert into UserInformation (UserName,Password,Mail) values('" + username + "','" + password + "','" + mail + "')";
                connection.Open();
                var repeat = DataBaseOperation.GetSqlDataReader(queryCmdText, connection).HasRows;//查询该用户是否存在
                connection.Close();
                if (repeat)
                {
                    
                    m_outgoingDataAssembler.AddFailure(ProtocolCode.RepeatInsert, "The user name repetition");//添加注册失败信息
                }
                //将用户注册信息插入数据库
                else
                {
                    connection.Open();
                    DataBaseOperation.ReviseDataToDataBase(reviseCmdText, connection);
                    m_outgoingDataAssembler.AddSuccess();
                    Console.WriteLine("用户信息成功写入数据库");
                }
                connection.Close();
                
            }
            return DoSendResult();//发送注册信息（成功/失败）

        }

        /// <summary>
        /// 查询道路信息
        /// </summary>
        /// <returns></returns>
        public bool DoAcquireRoadState()
        {
            string roadname="";
            int suggestion = 0;
            if (m_incomingDataParser.GetValue(ProtocolKey.RoadName, ref roadname))
            {
                //调用算法
                if (roadname != "")
                {
                    lock (bsTree)
                    {
                        suggestion = bsTree.Predict(roadname);
                    }
                    
                }
                
                if (suggestion==0)
                {
                    m_outgoingDataAssembler.AddSuccess();
                   
                }
                else
                {
                    m_outgoingDataAssembler.AddFailure(ProtocolCode.ManyCar, "The num of car is too much");
                }
                
            }
            return DoSendResult();//发送注册信息（成功/失败）
        }

        /// <summary>
        /// 标记路段拥堵
        /// </summary>
        /// <returns></returns>
        public bool Tab()
        {
            string roadname = "";
            if (m_incomingDataParser.GetValue(ProtocolKey.RoadName, ref roadname))
            {
                Console.WriteLine("用户标记路段为：{0}",roadname);
                bsTree.userBlock(roadname);
                m_outgoingDataAssembler.AddSuccess();
                

            }
            return DoSendResult();
        }
    }
    

    public class UploadSocketProtocolMgr : Object
    {
        private List<UploadSocketProtocol> m_list;

        public UploadSocketProtocolMgr()
        {
            m_list = new List<UploadSocketProtocol>();
        }

        public int Count()
        {
            return m_list.Count;
        }

        public UploadSocketProtocol ElementAt(int index)
        {
            return m_list.ElementAt(index);
        }

        public void Add(UploadSocketProtocol value)
        {
            m_list.Add(value);
        }

        public void Remove(UploadSocketProtocol value)
        {
            m_list.Remove(value);
        }
    }
}
