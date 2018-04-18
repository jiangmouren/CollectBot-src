using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormBot.Models
{
    public class GMSContext
    {
        public string endpoint;
        public string action;
        public string requestContent;
        //public string gmsResponse;
        public ConcurrentQueue<Vector> optionVectors;
        public GMSContext(string endpoint, string action, string requestContent, ConcurrentQueue<Vector> optionVectors)
        {
            this.endpoint = endpoint;
            this.action = action;
            this.requestContent = requestContent;
            this.optionVectors = optionVectors;
        }
    }
}