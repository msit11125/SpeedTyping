using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace SpeedTyping.Controllers
{
    public class GameController : Controller
    {
        [Authorize(Roles ="Admin,Player")]
        public ActionResult Index()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;

            if (claimsIdentity.IsAuthenticated && !GameHandler.IsGameStarting)
            {
                ViewBag.UserName = claimsIdentity.Name;

                return View();
            }

            return RedirectToAction("Login", "Account");
        }
    }
}