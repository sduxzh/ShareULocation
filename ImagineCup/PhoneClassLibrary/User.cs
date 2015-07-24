using System.Runtime.Serialization;

namespace ClientClassLibrary
{
    [DataContract]
    public class User
    {
        private string _Username;
        private string _Password;
        private string _Mail;

        [DataMember]
        public string Username {
            get { return _Username;}
            set { _Username = value; }
        }

        [DataMember]
        public string Password
        {
            get { return _Password; }
            set { _Password = value; }
        }

        [DataMember]
        public string Mail
        {
            get { return _Mail;}
            set { _Mail = value; }
        }
       
    }
}