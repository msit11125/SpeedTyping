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
            var id = Context.ConnectionId;
            var speakerName = UserHandler.Users.Find(u => u.Id == id).Name;

            Clients.All.getMessage(speakerName, msg, id);
        }
    }
}