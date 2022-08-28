using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application2
{
    internal class MessageItem
    {
        public string clientId { get; set; }
        public string payload { get; set; }

        public MessageItem(string clientId, string payload)
        {
            this.clientId = clientId;
            this.payload = payload;
        }

        public MessageItem()
        {
        }
    }
}
