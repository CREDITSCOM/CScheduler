using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CScheduler.Classes.Database;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity;

namespace CScheduler.Controllers
{
    public class CreditsNetController : Controller
    {
        private DatabaseContext dbContext = new DatabaseContext();

        // GET: CreditsNet
        public async Task<ActionResult> Index()
        {
            var identity = HttpContext.GetOwinContext().Authentication.GetExternalIdentity(DefaultAuthenticationTypes.ApplicationCookie);

            if (identity == null || identity.Name == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                var user = dbContext.Users.FirstOrDefault(x => x.Email == identity.Name);

                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }
                else if (!user.IsActivated)
                {
                    return RedirectToAction("WaitActivation", "Account");
                }

                return View(await dbContext.CreditsNets.ToListAsync());
            }            
        }

        [HttpGet]
        public async Task<ActionResult> Edit(int ID = -1)
        {
            CreditsNet creditsNet = await dbContext.CreditsNets.FirstOrDefaultAsync(x => x.ID == ID);

            if (creditsNet == null)
            {
                creditsNet = new CreditsNet();
                creditsNet.ID = -1;
            }

            return View(creditsNet);
        }

        // POST: CreditsNet/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(CreditsNet model)
        {
            if (ModelState.IsValid)
            {
                var creditsNet = await dbContext.CreditsNets.FirstOrDefaultAsync(x => x.ID == model.ID);

                if (creditsNet == null)
                {
                    creditsNet = new CreditsNet();

                    dbContext.CreditsNets.Add(creditsNet);
                }

                creditsNet.Name = model.Name;
                creditsNet.EndPoint = model.EndPoint;

                await dbContext.SaveChangesAsync();
                return RedirectToAction("Index", "CreditsNet");
            }

            return View(model);
        }

        // GET: CreditsNet/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            CreditsNet creditsNet = await dbContext.CreditsNets.FindAsync(id);

            if (creditsNet == null)
            {
                return HttpNotFound();
            }

            return View(creditsNet);
        }

        // POST: CreditsNet/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            CreditsNet creditsNet = await dbContext.CreditsNets.FindAsync(id);
            dbContext.CreditsNets.Remove(creditsNet);
            await dbContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                dbContext.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
