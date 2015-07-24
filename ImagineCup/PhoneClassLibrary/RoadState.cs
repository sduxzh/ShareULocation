using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientClassLibrary
{
    public class RoadState
    {
        private string _roadname;
        private bool _roadstate;


        public string RoadName
        {
            get { return _roadname; }
            set { _roadname = value; }
        }


        public bool State
        {
            get { return _roadstate;}
            set { _roadstate = value; }
        }
      
            
        }
}
