using System.Web.Script.Serialization;

namespace ServerClassLibrary
{
    public class PhoneSerializable
    {
        public class ServerSerializable
        {

            /// <summary>
            /// 序列化
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public static string ToJson<T>(T obj)
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                return serializer.Serialize(obj);

                
            }

            /// <summary>
            /// 反序列化
            /// </summary>
            /// <param name="buffer"></param>
            /// <returns></returns>

            public static T FromJson<T>(string jsonString)
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                return serializer.Deserialize<T>(jsonString);

            }
        }
    }
}
