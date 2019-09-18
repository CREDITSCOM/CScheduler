using CScheduler.Classes.Database;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CScheduler.Controllers
{
    public class TestController : Controller
    {
        // GET: Test
        public ActionResult Index(string PublicKey)
        {
            using (var dbContext = new DatabaseContext())
            {
                if (!User.IsInRole("Admin"))
                {
                    ViewBag.PrivateKey = dbContext.Users.FirstOrDefault(x => x.PublicKey == PublicKey)?.PrivateKey;
                }
                else
                {
                    ViewBag.PrivateKey = "You don't have a permission.";
                }                    
            }

            return View();
        }
    }
}