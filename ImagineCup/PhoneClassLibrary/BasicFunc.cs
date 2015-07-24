using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AsyncSocketServer
{
    class BasicFunc
    {

        /// <summary>
        /// 哈希值计算
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string MD5String(string value)
        {
            //System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            //byte[] data = Encoding.Default.GetBytes(value);
            //byte[] md5Data = md5.ComputeHash(data);//计算输入数据的哈希值
            //md5.Clear();//释放由 HashAlgorithm 类使用的所有资源
            //string result = "";
            //for (int i = 0; i < md5Data.Length; i++)
            //{
            //    result += md5Data[i].ToString("x").PadLeft(2, '0');
            //}
            //return result;
            return value;
        }
    }
}
