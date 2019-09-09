using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using CScheduler.Classes.Database;
using CScheduler.Models;
using System.Net;
using System.Net.Mail;
using CScheduler.Classes;

namespace CScheduler.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        [AllowAnonymous]
        public ActionResult FirstRun()
        {
            var identity = HttpContext.GetOwinContext().Authentication.GetExternalIdentity(DefaultAuthenticationTypes.ApplicationCookie);

            using (var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new DatabaseContext())))
            using (var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new DatabaseContext())))
            using (var dbContext = new DatabaseContext())
            {
                var userId = userManager.FindByName(identity.Name).Id;
                var admin = dbContext.Users.FirstOrDefault(x => x.Email == identity.Name);
                admin.IsActivated = true;
                dbContext.SaveChanges();

                if (!roleManager.RoleExists("Admin"))
                {
                    roleManager.Create(new IdentityRole { Name = "Admin" });
                }

                if (!userManager.IsInRole(userId, "Admin"))
                {
                    userManager.AddToRole(userId, "Admin");
                }

                //CreditsNets
                dbContext.CreditsNets.Add(new CreditsNet { Name = "CreditsNetwork", EndPoint = "http://wallet.credits.com/Main/api/UnsafeTransaction" });
                dbContext.CreditsNets.Add(new CreditsNet { Name = "testnet-r4_2", EndPoint = "http://wallet.credits.com/testnet-r4_2/api/UnsafeTransaction" });
                dbContext.CreditsNets.Add(new CreditsNet { Name = "DevsDappsTestnet", EndPoint = "http://wallet.credits.com/DevsDappsTestnet/api/UnsafeTransaction" });
                dbContext.SaveChanges();

                //SmartJobs 1
                var sj = new SmartJob();
                sj.Address = "5kXTAm4wYJ6P1DLACk9ehUQEzjUKLHHBKA2EK1yinvz6";
                sj.CreatedAt = DateTime.Now;
                sj.CreatedBy = admin;
                sj.CreditsNet = dbContext.CreditsNets.FirstOrDefault(x => x.Name == "Credits network");
                sj.Events = new List<JobEvent>();
                sj.ExecutionMode = SmartJob.ExecutionModeEnum.Regular;
                sj.IsActive = true;
                sj.Method = "Method 1";
                sj.Name = "Задача 1";
                sj.Rule = new Rule();
                sj.Rule.RegularDateFrom = "10.07.2019 05:05:19";
                sj.Rule.RegularDateTo = "31.07.2019 22:03:07";
                sj.Rule.RegularPeriod = PeriodEnum.Minute;
                sj.Rule.RegularValue = 5;
                sj.Rule.Presentation = Rule.GeneratePresentation(sj);
                dbContext.SmartJobs.Add(sj);

                //SmartJobs 2
                sj = new SmartJob();
                sj.Address = "5kXTAm4wYJ6P1DLACk9ehUQEzjUKLHHBKA2EK1yinvz6";
                sj.CreatedAt = DateTime.Now;
                sj.CreatedBy = admin;
                sj.CreditsNet = dbContext.CreditsNets.FirstOrDefault(x => x.Name == "Test net");
                sj.Events = new List<JobEvent>();
                sj.ExecutionMode = SmartJob.ExecutionModeEnum.Once;
                sj.IsActive = true;
                sj.Method = "Method 2";
                sj.Name = "Задача 2";
                sj.Rule = new Rule();
                sj.Rule.OnceDate = "10.07.2019 05:05:19";
                sj.Rule.Presentation = Rule.GeneratePresentation(sj);
                dbContext.SmartJobs.Add(sj);

                //SmartJobs 3
                sj = new SmartJob();
                sj.Address = "5kXTAm4wYJ6P1DLACk9ehUQEzjUKLHHBKA2EK1yinvz6";
                sj.CreatedAt = DateTime.Now;
                sj.CreatedBy = admin;
                sj.CreditsNet = dbContext.CreditsNets.FirstOrDefault(x => x.Name == "Devs & dapps");
                sj.Events = new List<JobEvent>();
                sj.ExecutionMode = SmartJob.ExecutionModeEnum.CronExpression;
                sj.IsActive = false;
                sj.Method = "Method 3";
                sj.Name = "Задача 3";
                sj.Rule = new Rule();
                sj.Rule.CronExpression = "0,13 0,55 0/7 5,16 * ? *";
                sj.Rule.Presentation = Rule.GeneratePresentation(sj);
                dbContext.SmartJobs.Add(sj);

                //SmartJobs 4
                sj = new SmartJob();
                sj.Address = "5kXTAm4wYJ6P1DLACk9ehUQEzjUKLHHBKA2EK1yinvz6";
                sj.CreatedAt = DateTime.Now;
                sj.CreatedBy = admin;
                sj.CreditsNet = dbContext.CreditsNets.FirstOrDefault(x => x.Name == "Credits network");
                sj.Events = new List<JobEvent>();
                sj.ExecutionMode = SmartJob.ExecutionModeEnum.Regular;
                sj.IsActive = true;
                sj.Method = "Method 4";
                sj.Name = "Задача 4";
                sj.Rule = new Rule();
                sj.Rule.RegularDateFrom = "10.07.2019 05:05:19";
                sj.Rule.RegularDateTo = "31.07.2019 22:03:07";
                sj.Rule.RegularPeriod = PeriodEnum.Minute;
                sj.Rule.RegularValue = 5;
                sj.Rule.Presentation = Rule.GeneratePresentation(sj);
                dbContext.SmartJobs.Add(sj);

                //SmartJobs 5
                sj = new SmartJob();
                sj.Address = "5kXTAm4wYJ6P1DLACk9ehUQEzjUKLHHBKA2EK1yinvz6";
                sj.CreatedAt = DateTime.Now;
                sj.CreatedBy = admin;
                sj.CreditsNet = dbContext.CreditsNets.FirstOrDefault(x => x.Name == "Test net");
                sj.Events = new List<JobEvent>();
                sj.ExecutionMode = SmartJob.ExecutionModeEnum.Once;
                sj.IsActive = true;
                sj.Method = "Method 5";
                sj.Name = "Задача 5";
                sj.Rule = new Rule();
                sj.Rule.OnceDate = "10.07.2019 05:05:19";
                sj.Rule.Presentation = Rule.GeneratePresentation(sj);
                dbContext.SmartJobs.Add(sj);

                //SmartJobs 6
                sj = new SmartJob();
                sj.Address = "5kXTAm4wYJ6P1DLACk9ehUQEzjUKLHHBKA2EK1yinvz6";
                sj.CreatedAt = DateTime.Now;
                sj.CreatedBy = admin;
                sj.CreditsNet = dbContext.CreditsNets.FirstOrDefault(x => x.Name == "Devs & dapps");
                sj.Events = new List<JobEvent>();
                sj.ExecutionMode = SmartJob.ExecutionModeEnum.CronExpression;
                sj.IsActive = false;
                sj.Method = "Method 6";
                sj.Name = "Задача 6";
                sj.Rule = new Rule();
                sj.Rule.CronExpression = "0,13 0,55 0/7 5,16 * ? *";
                sj.Rule.Presentation = Rule.GeneratePresentation(sj);
                dbContext.SmartJobs.Add(sj);

                //SmartJobs 7
                sj = new SmartJob();
                sj.Address = "5kXTAm4wYJ6P1DLACk9ehUQEzjUKLHHBKA2EK1yinvz6";
                sj.CreatedAt = DateTime.Now;
                sj.CreatedBy = admin;
                sj.CreditsNet = dbContext.CreditsNets.FirstOrDefault(x => x.Name == "Credits network");
                sj.Events = new List<JobEvent>();
                sj.ExecutionMode = SmartJob.ExecutionModeEnum.Regular;
                sj.IsActive = true;
                sj.Method = "Method 7";
                sj.Name = "Задача 7";
                sj.Rule = new Rule();
                sj.Rule.RegularDateFrom = "10.07.2019 05:05:19";
                sj.Rule.RegularDateTo = "31.07.2019 22:03:07";
                sj.Rule.RegularPeriod = PeriodEnum.Minute;
                sj.Rule.RegularValue = 5;
                sj.Rule.Presentation = Rule.GeneratePresentation(sj);
                dbContext.SmartJobs.Add(sj);

                //SmartJobs 8
                sj = new SmartJob();
                sj.Address = "5kXTAm4wYJ6P1DLACk9ehUQEzjUKLHHBKA2EK1yinvz6";
                sj.CreatedAt = DateTime.Now;
                sj.CreatedBy = admin;
                sj.CreditsNet = dbContext.CreditsNets.FirstOrDefault(x => x.Name == "Test net");
                sj.Events = new List<JobEvent>();
                sj.ExecutionMode = SmartJob.ExecutionModeEnum.Once;
                sj.IsActive = true;
                sj.Method = "Method 8";
                sj.Name = "Задача 8";
                sj.Rule = new Rule();
                sj.Rule.OnceDate = "10.07.2019 05:05:19";
                sj.Rule.Presentation = Rule.GeneratePresentation(sj);
                dbContext.SmartJobs.Add(sj);

                //SmartJobs 9
                sj = new SmartJob();
                sj.Address = "5kXTAm4wYJ6P1DLACk9ehUQEzjUKLHHBKA2EK1yinvz6";
                sj.CreatedAt = DateTime.Now;
                sj.CreatedBy = admin;
                sj.CreditsNet = dbContext.CreditsNets.FirstOrDefault(x => x.Name == "Devs & dapps");
                sj.Events = new List<JobEvent>();
                sj.ExecutionMode = SmartJob.ExecutionModeEnum.CronExpression;
                sj.IsActive = false;
                sj.Method = "Method 9";
                sj.Name = "Задача 9";
                sj.Rule = new Rule();
                sj.Rule.CronExpression = "0,13 0,55 0/7 5,16 * ? *";
                sj.Rule.Presentation = Rule.GeneratePresentation(sj);
                dbContext.SmartJobs.Add(sj);

                //10
                sj = new SmartJob();
                sj.Address = "5kXTAm4wYJ6P1DLACk9ehUQEzjUKLHHBKA2EK1yinvz6";
                sj.CreatedAt = DateTime.Now;
                sj.CreatedBy = admin;
                sj.CreditsNet = dbContext.CreditsNets.FirstOrDefault(x => x.Name == "Credits network");
                sj.Events = new List<JobEvent>();
                sj.ExecutionMode = SmartJob.ExecutionModeEnum.Regular;
                sj.IsActive = true;
                sj.Method = "Method 10";
                sj.Name = "Задача 10";
                sj.Rule = new Rule();
                sj.Rule.RegularDateFrom = "10.07.2019 05:05:19";
                sj.Rule.RegularDateTo = "31.07.2019 22:03:07";
                sj.Rule.RegularPeriod = PeriodEnum.Minute;
                sj.Rule.RegularValue = 5;
                sj.Rule.Presentation = Rule.GeneratePresentation(sj);
                dbContext.SmartJobs.Add(sj);

                //SmartJobs 11
                sj = new SmartJob();
                sj.Address = "5kXTAm4wYJ6P1DLACk9ehUQEzjUKLHHBKA2EK1yinvz6";
                sj.CreatedAt = DateTime.Now;
                sj.CreatedBy = admin;
                sj.CreditsNet = dbContext.CreditsNets.FirstOrDefault(x => x.Name == "Test net");
                sj.Events = new List<JobEvent>();
                sj.ExecutionMode = SmartJob.ExecutionModeEnum.Once;
                sj.IsActive = true;
                sj.Method = "Method 11";
                sj.Name = "Задача 11";
                sj.Rule = new Rule();
                sj.Rule.OnceDate = "10.07.2019 05:05:19";
                sj.Rule.Presentation = Rule.GeneratePresentation(sj);
                dbContext.SmartJobs.Add(sj);

                //SmartJobs 12
                sj = new SmartJob();
                sj.Address = "5kXTAm4wYJ6P1DLACk9ehUQEzjUKLHHBKA2EK1yinvz6";
                sj.CreatedAt = DateTime.Now;
                sj.CreatedBy = admin;
                sj.CreditsNet = dbContext.CreditsNets.FirstOrDefault(x => x.Name == "Devs & dapps");
                sj.Events = new List<JobEvent>();
                sj.ExecutionMode = SmartJob.ExecutionModeEnum.CronExpression;
                sj.IsActive = false;
                sj.Method = "Method 12";
                sj.Name = "Задача 12";
                sj.Rule = new Rule();
                sj.Rule.CronExpression = "0,13 0,55 0/7 5,16 * ? *";
                sj.Rule.Presentation = Rule.GeneratePresentation(sj);
                dbContext.SmartJobs.Add(sj);

                dbContext.SaveChanges();

                var date = new DateTime(2019, 1, 1);
                foreach (var sJob in dbContext.SmartJobs.ToList())
                {
                    for (int i = 0; i < 45; i++)
                    {
                        var sEvent = new JobEvent()
                        {
                            SmartJob = sJob,
                            IsSuccessed = true,
                            RequestDate = date.AddMinutes(i),
                            ResponseDate = date.AddMinutes(i).AddSeconds(i),
                            Text = "Ok!"
                        };

                        dbContext.JobEvents.Add(sEvent);
                        dbContext.SaveChanges();
                    }

                    date = date.AddHours(1);                    
                }                
            }

            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/WaitActivation
        [AllowAnonymous]
        public ActionResult WaitActivation()
        {
            return View();
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    using (var dbContext = new DatabaseContext())
                    {
                        var user = dbContext.Users.FirstOrDefault(x => x.Email == model.Email);
                        if (string.IsNullOrEmpty(user.ApiKey))
                        {
                            user.ApiKey = Guid.NewGuid().ToString();
                            dbContext.SaveChanges();
                        }
                    }
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var o = new Rebex.Security.Cryptography.Ed25519();
                var publicKey = Base58Check.Base58CheckEncoding.EncodePlain(o.GetPublicKey());
                var privateKey = Base58Check.Base58CheckEncoding.EncodePlain(o.GetPrivateKey());

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    FullName = string.Format("{0} {1}.", model.LastName, model.FirstName.Substring(0, 1)),
                    PublicKey = publicKey,
                    PrivateKey = privateKey,
                    IsActivated = true,
                    ApiKey = Guid.NewGuid().ToString()
                };

                var result = await UserManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null) //|| !(await UserManager.IsEmailConfirmedAsync(user.Id))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);

                Shared.SendEmail(user.Email, "Reset password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");

                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}