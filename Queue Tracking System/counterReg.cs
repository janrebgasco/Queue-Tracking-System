using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queue_Tracking_System
{
    class counterReg
    {
        //Create a class for register... using this .
        public int availableToken { get; set; }
        public int customerCount { get; set; }
        public int servingToken { get; set; }
        public int totalServedToken { get; set; }
        public bool syncQueueList { get; set; }
        public string esImg { get; set; }
        
    }
}
