using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Net;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using LRC_NET_Framework.Models;
using System.Collections.Generic;
using ExcelImport.Models;
using LinqToExcel;
using System.Data.OleDb;
using System.IO;
using System.Data;
using System.Data.Entity.Validation;
using System.Data.Entity;
using System.Threading;
using LinqToExcel.Query;
using ExcelImport;

//using System.Web.Http;

namespace LRC_NET_Framework.Controllers
{

    [Authorize]
    public class AccountController : Controller
    {
        private LRCEntities db = new LRCEntities();

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public AccountController(ApplicationUserManager userManager, ISecureDataFormat<AuthenticationTicket> accessTokenFormat, ApplicationRoleManager roleManager)
        {
            ///Make an instance of the user manager in the controller to avoid null reference exception
            UserManager = userManager;
            //AccessTokenFormat = accessTokenFormat;
            ///Make an instance of the role manager in the constructor to avoid null reference exception
            RoleManager = roleManager;
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

        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? Request.GetOwinContext().GetUserManager<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
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
        //[AllowAnonymous]
        [AllowAnonymous]
        //[Authorize(Roles = "admin")]
        public ActionResult Register()
        {
            //var identity = (ClaimsIdentity)User.Identity;
            //var rr = identity.Claims.LastOrDefault().Value;
            var roles = new SelectList(db.AspNetRoles, "Id", "Name");
            ViewBag.Roles = roles;
            var _users = db.AspNetUsers.ToList();
            List<SelectListItem> _UsersRoles = new List<SelectListItem>();
            foreach (var _user in _users)
            {
                var currentRoles = UserManager.GetRolesAsync(_user.Id);
                _UsersRoles.Add(new SelectListItem() { Text = _user.UserName, Value = currentRoles.Result.FirstOrDefault() });
            }
            ViewBag.UsersAndRoles = _UsersRoles;
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        //[Authorize(Roles = "admin")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model, string Roles)
        {
            SelectList roles = new SelectList(db.AspNetRoles, "Id", "Name");
            ViewBag.Roles = roles;
            var _users = db.AspNetUsers.ToList();
            List<SelectListItem> _UsersRoles = new List<SelectListItem>();
            foreach (var _user in _users)
            {
                var currentRoles = UserManager.GetRolesAsync(_user.Id);
                _UsersRoles.Add(new SelectListItem() { Text = _user.UserName, Value = currentRoles.Result.FirstOrDefault() });
            }
            ViewBag.UsersAndRoles = _UsersRoles;
            ExcelMembers excelMembers = new ExcelMembers();

            Error error = excelMembers.SplitFullName(model.UserLastFirstName, null, 0, out string lastName, out string firstName, out string middleName);
            if (error.errCode != ErrorDetail.Success)
            {
                ModelState.AddModelError("User Name Error", error.errMsg + ". Please enter last name and first name, separated by a comma.");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                string selectedRole = roles.Where(t => t.Value == Roles).FirstOrDefault().Text;
                var user = new ApplicationUser { UserName = model.Email + ":" + selectedRole, Email = model.Email }; // placing selected Role Name to User Name in AspNetUsers table
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    using (LRCEntities context = new LRCEntities())
                    {
                        try
                        {
                            AspNetUsers aUser = context.AspNetUsers.Where(s => s.Email.ToUpper() == model.Email.ToUpper()).FirstOrDefault();
                            aUser.LastFirstName = model.UserLastFirstName;
                            context.SaveChanges();
                        }
                        catch (DbEntityValidationException ex)
                        {
                            error.errCode = ErrorDetail.DataImportError;
                            error.errMsg = ErrorDetail.GetMsg(error.errCode);
                            foreach (DbEntityValidationResult validationError in ex.EntityValidationErrors)
                            {
                                error.errMsg += ". Object: " + validationError.Entry.Entity.ToString();
                                foreach (DbValidationError err in validationError.ValidationErrors)
                                {
                                    error.errMsg += ". " + err.ErrorMessage;
                                }
                            }
                        }
                    }
                    if (error.errCode != ErrorDetail.Success)
                    {
                        ModelState.AddModelError("User Name Update Failed", error.errMsg);
                        return View(model);
                    }

                    return RedirectToAction("RegistrationRequestSentToAdmin", "Account");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ApplyRegistrationRequest
        //[AllowAnonymous]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> ApplyRegistrationRequest(string userName)
        {
            var uNameRole = userName.Split(':');
            string uName = uNameRole[0].Trim();
            string uRole = uNameRole[1].Trim();

            // LastFirstName is a added to AspNetUsers table custom field so we cant use default built-in functionality and 
            // have to use standart DB request
            var user = await UserManager.FindByEmailAsync(db.AspNetUsers.Where(s => s.LastFirstName == uName).FirstOrDefault().Email.Trim());

            var result = await UserManager.AddToRoleAsync(user.Id, uRole);
            if (result.Succeeded)
            {
                //Place Email to UserName field in AspNetUsers table. We keeped RoleName there before
                user.UserName = user.Email;
                await UserManager.UpdateAsync(user);

                //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            AddErrors(result);
            return RedirectToAction("ConfirmRegistration", "Account");
        }

        //
        // GET: /Account/RejectRegistrationRequest
        //[AllowAnonymous]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> RejectRegistrationRequest(string userName)
        {
            //Delete user on Reject action
            var user = await UserManager.FindByEmailAsync(db.AspNetUsers.Where(s => s.LastFirstName == userName).FirstOrDefault().Email.Trim());
            var result = await UserManager.DeleteAsync(user);

            //string currentRoles = UserManager.GetRolesAsync(user.Id).Result.FirstOrDefault();
            //var result = await UserManager.RemoveFromRoleAsync(user.Id, currentRoles);

            //result = await UserManager.AddToRoleAsync(user.Id, currentRoles);
            //if (result.Succeeded)
            //{
            //    //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            //}
            AddErrors(result);
            return RedirectToAction("ConfirmRegistration", "Account");
        }

        //
        // GET: /Account/RegistrationRequestSentToAdmin
        [AllowAnonymous]
        public ActionResult RegistrationRequestSentToAdmin()
        {
            return View();
        }

        //[AllowAnonymous]
        //[Route("users/{id:guid}/roles")]
        //[HttpPut]
        //public async Task<System.Web.Http.IHttpActionResult> AssignRolesToUser(string id, string[] rolesToAssign)
        //{
        //    if (rolesToAssign == null)
        //    {
        //        return this.BadRequest("No roles specified");
        //    }

        //    ///find the user we want to assign roles to
        //    var appUser = await this.UserManager.FindByIdAsync(id);

        //    if (appUser == null || appUser.IsDeleted)
        //    {
        //        return NotFound();
        //    }

        //    ///check if the user currently has any roles
        //    var currentRoles = await this.UserManager.GetRolesAsync(appUser.Id);


        //    var rolesNotExist = rolesToAssign.Except(this.RoleManager.Roles.Select(x => x.Name)).ToArray();

        //    if (rolesNotExist.Count() > 0)
        //    {
        //        ModelState.AddModelError("", string.Format("Roles '{0}' does not exist in the system", string.Join(",", rolesNotExist)));
        //        return this.BadRequest(ModelState);
        //    }

        //    ///remove user from current roles, if any
        //    IdentityResult removeResult = await this.UserManager.RemoveFromRolesAsync(appUser.Id, currentRoles.ToArray());


        //    if (!removeResult.Succeeded)
        //    {
        //        ModelState.AddModelError("", "Failed to remove user roles");
        //        return BadRequest(ModelState);
        //    }

        //    ///assign user to the new roles
        //    IdentityResult addResult = await this.UserManager.AddToRolesAsync(appUser.Id, rolesToAssign);

        //    if (!addResult.Succeeded)
        //    {
        //        ModelState.AddModelError("", "Failed to add user roles");
        //        return BadRequest(ModelState);
        //    }

        //    return Ok(new { userId = id, rolesAssigned = rolesToAssign });
        //}

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
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ConfirmRegistration
        [Authorize(Roles = "admin")]
        public ActionResult ConfirmRegistration()
        {
            var _users = db.AspNetUsers.ToList();
            List<SelectListItem> _UsersRoles = new List<SelectListItem>();
            foreach (var _user in _users)
            {
                string currentRoles = UserManager.GetRolesAsync(_user.Id).Result.FirstOrDefault();
                if (String.IsNullOrEmpty(currentRoles))
                {
                    var uNameRole = _user.UserName.Split(':');
                    string uName = uNameRole[0].Trim();
                    string uRole = uNameRole[1].Trim();
                    _UsersRoles.Add(new SelectListItem() { Text = _user.LastFirstName + " : " + uRole, Value = String.Empty });
                }
                else
                    _UsersRoles.Add(new SelectListItem() { Text = _user.LastFirstName, Value = currentRoles });
            }
            ViewBag.UsersAndRoles = _UsersRoles;
            return View();
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