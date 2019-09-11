using CScheduler.Classes.Database;
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
                ViewBag.PrivateKey = dbContext.Users.FirstOrDefault(x => x.PublicKey == PublicKey)?.PrivateKey;
            }

            return View();
        }
    }
}