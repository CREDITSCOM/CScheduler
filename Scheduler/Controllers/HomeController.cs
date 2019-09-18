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
using Rule = CScheduler.Classes.Database.Rule;
using static CScheduler.Classes.Database.SmartJob;
using Microsoft.AspNet.Identity.EntityFramework;

namespace CScheduler.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private DatabaseContext dbContext = new DatabaseContext();

        [AllowAnonymous]
        public ActionResult Index()
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

                var model = new List<SmartJob>();

                using (var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new DatabaseContext())))
                {
                    var userId = userManager.FindByName(identity.Name).Id;

                    if (userManager.IsInRole(userId, "Admin"))
                    {
                        model = dbContext.SmartJobs.Include("Rule").Include("CreditsNet").Include("CreatedBy").Where(x => x.IsDeleted != true).ToList();
                    }
                    else
                    {
                        model = dbContext.SmartJobs.Include("Rule").Include("CreditsNet").Include("CreatedBy").Where(x => x.CreatedBy.Id == userId && x.IsDeleted != true).ToList();
                    }
                }

                return View(model);
            }
        }

        [HttpGet]
        public ActionResult Edit(int ID = -1)
        {
            SmartJob smartJob = null;

            if (ID == -1)
            {
                smartJob = new SmartJob();
                smartJob.ID = -1;
                smartJob.IsActive = true;
                smartJob.ExecutionMode = ExecutionModeEnum.Regular;
                smartJob.Rule = new Rule() { RegularPeriod = PeriodEnum.Minute };
                smartJob.Events = new List<JobEvent>();
            }
            else
            {
                smartJob = dbContext.SmartJobs.Include("Rule").Include("CreditsNet").Include("Events").Include("CreatedBy").FirstOrDefault(x => x.ID == ID);

                if (smartJob == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    if (!User.IsInRole("Admin"))
                    {
                        var identity = HttpContext.GetOwinContext().Authentication.GetExternalIdentity(DefaultAuthenticationTypes.ApplicationCookie);

                        var user = dbContext.Users.FirstOrDefault(x => x.Email == identity.Name);

                        if (user.Id != smartJob.CreatedBy.Id)
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }
            }

            return View(smartJob);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(SmartJob model)
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

                if (ModelState.IsValid)
                {
                    var smartJob = await dbContext.SmartJobs.Include("Rule").Include("CreditsNet").Include("Events").Include("CreatedBy").FirstOrDefaultAsync(x => x.ID == model.ID);

                    if (smartJob == null)
                    {
                        smartJob = new SmartJob();
                        smartJob.CreatedBy = user;
                        smartJob.CreatedAt = DateTime.Now;
                        smartJob.Rule = new Rule();
                        smartJob.Events = new List<JobEvent>();

                        dbContext.SmartJobs.Add(smartJob);
                    }

                    smartJob.Name = model.Name;
                    smartJob.IsActive = model.IsActive;
                    smartJob.Method = model.Method;
                    smartJob.Address = model.Address;
                    smartJob.CreditsNet = await dbContext.CreditsNets.FirstOrDefaultAsync(x => x.ID == model.CreditsNet.ID);
                    smartJob.ExecutionMode = model.ExecutionMode;
                    smartJob.Rule.RegularDateFrom = model.Rule.RegularDateFrom;
                    smartJob.Rule.RegularDateTo = model.Rule.RegularDateTo;
                    smartJob.Rule.RegularPeriod = model.Rule.RegularPeriod;
                    smartJob.Rule.RegularValue = model.Rule.RegularValue;
                    smartJob.Rule.OnceDate = model.Rule.OnceDate;
                    smartJob.Rule.CronExpression = model.Rule.CronExpression;
                    smartJob.Rule.Presentation = Rule.GeneratePresentation(smartJob);

                    await dbContext.SaveChangesAsync();

                    //Обновляем данные по задаче
                    await QuartzTasks.UpdateJob(smartJob.ID);

                    return RedirectToAction("Index", "Home");
                }

                return View(model);
            }
        }

        [HttpGet]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            SmartJob smartJob = await dbContext.SmartJobs.FindAsync(id);
            if (smartJob == null)
            {
                return RedirectToAction("Index");
            }

            return View(smartJob);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var smartJob = await dbContext.SmartJobs.FirstOrDefaultAsync(x => x.ID == id);
            var events = await dbContext.JobEvents.Include("SmartJob").Where(x => x.SmartJob.ID == id).ToListAsync();

            dbContext.JobEvents.RemoveRange(events);
            dbContext.SmartJobs.Remove(smartJob);

            await dbContext.SaveChangesAsync();

            await QuartzTasks.DeleteJob(id, false);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<ActionResult> Execute(int id)
        {
            await QuartzTasks.ExecuteJob(id);

            return RedirectToAction("Edit", new { id = id });
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