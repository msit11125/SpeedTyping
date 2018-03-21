using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace SpeedTyping.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Login(string name)
        {
            if (GameHandler.IsGameStarting)
            {
                TempData["Info"] = "遊戲進行中不能登入";
                return View();
            }


            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Name, name));
            claims.Add(new Claim(ClaimTypes.Role, "Player"));
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));

            var identity = new ClaimsIdentity(claims, "ApplicationCookie");

            Request.GetOwinContext().Authentication.SignIn(identity);


            return RedirectToAction("Index", "Game");
        }
    }
}