using Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

[assembly: OwinStartup(typeof(SpeedTyping.Startup))]
namespace SpeedTyping
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                //識別的Cookie名稱
                AuthenticationType = "ApplicationCookie",
                //無權限時導頁
                LoginPath = new PathString("/Account/Login"),
                ExpireTimeSpan = TimeSpan.FromDays(5)
            });
            app.MapSignalR();
        }
    }
}