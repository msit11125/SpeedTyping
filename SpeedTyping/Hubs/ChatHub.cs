using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace SpeedTyping.Hubs
{
    public class ChatHub : Hub
    {
        public void SendMessage(string msg)
        {
            Clients.All.getMessage(msg, Context.ConnectionId);
        }
    }
}