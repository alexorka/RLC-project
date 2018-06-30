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
using System.Threading;
using LinqToExcel.Query;

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

            if (ModelState.IsValid)
            {
                string selectedRole = roles.Where(t => t.Value == Roles).FirstOrDefault().Text;
                var user = new ApplicationUser { UserName = model.Email + ":" + selectedRole, Email = model.Email }; // placing selected Role Name to User Name in AspNetUsers table
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
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
            var user = await UserManager.FindByEmailAsync(uName);
            var result = await UserManager.AddToRoleAsync(user.Id, uRole);
            if (result.Succeeded)
            {
                //Place email to UserName field in AspNetUsers table. We keeped RoleName there before
                user.UserName = uName;
                await UserManager.UpdateAsync(user);

                //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            AddErrors(result);
            return RedirectToAction("AdminTasks", "Account");
        }

        //
        // GET: /Account/RejectRegistrationRequest
        //[AllowAnonymous]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> RejectRegistrationRequest(string userName)
        {
            //Delete user on Reject action
            var user = await UserManager.FindByEmailAsync(userName.Trim());
            var result = await UserManager.DeleteAsync(user);

            //string currentRoles = UserManager.GetRolesAsync(user.Id).Result.FirstOrDefault();
            //var result = await UserManager.RemoveFromRoleAsync(user.Id, currentRoles);

            //result = await UserManager.AddToRoleAsync(user.Id, currentRoles);
            //if (result.Succeeded)
            //{
            //    //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            //}
            AddErrors(result);
            return RedirectToAction("AdminTasks", "Account");
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
        // GET: /Account/AdminTasks
        [Authorize(Roles = "admin")]
        public ActionResult AdminTasks(/*string fileTypeSelectResult*/)
        {
            var _users = db.AspNetUsers.ToList();
            List<SelectListItem> _UsersRoles = new List<SelectListItem>();
            foreach (var _user in _users)
            {
                string currentRoles = UserManager.GetRolesAsync(_user.Id).Result.FirstOrDefault();
                if (String.IsNullOrEmpty(currentRoles))
                    _UsersRoles.Add(new SelectListItem() { Text = /*_user.Email + ":" + */_user.UserName, Value = currentRoles });
                else
                    _UsersRoles.Add(new SelectListItem() { Text = _user.Email, Value = currentRoles });
            }
            ViewBag.UsersAndRoles = _UsersRoles;

            List<string> errs = new List<string>();
            if (TempData["ErrorList"] == null)
            {
                errs.Add("Empty");
            }
            else
                errs = TempData["ErrorList"] as List<string>;

            ViewData["ErrorList"] = errs;
            //ViewBag.FileTypeMessage = fileTypeSelectResult ?? String.Empty;

            return View();
        }

        [HttpPost]
        public ActionResult UploadExcel(HttpPostedFileBase FileUpload, int? ImportType)
        {
            string test = String.Empty;
            #region Test GetAreaName
            ////Case 1 – If char 5 to 8 <> PROF, but upper 5 to 8 does = COUNS set field 8 to Counselor 
            //test = GetAreaName("Adj Counslr-DSP&S-428A"); //Counselor
            ////Case 2 – If char 5 to 8 <> PROF, AND upper 5 to 8 does = LIBR set field 8 to Librarian
            //test = GetAreaName("Adj Librarian-Supplementl-014C"); //Librarian
            //test = GetAreaName("Adj Librarian FLC Supp 014C"); //Librarian
            ////Case 3 – If char 5 to 8 <> PROF, but upper 5 to 8 does = Nurses set field 8 to Nurses
            //test = GetAreaName("Adj Nurses-Unrestricted-015F"); //Nurses
            ////Case 4 – If char 5 to 8 <> PROF, but upper 5 to 8 does = "COORD" set field to Coord to end of string
            //test = GetAreaName("Adj Coord-Miscellaneous"); //Coord-Miscellaneous
            //test = GetAreaName("ADJ Coord-Work Experience"); //Coord-Work Experience 
            ////Case 5 – Set to “Miscellaneous” WHERE Field 8 is still NULL and “misc” exists anywhere in the string
            //test = GetAreaName("Adj Prof-TS-SCC Miscellaneous"); //Miscellaneous
            //test = GetAreaName("Adj Prof-TS-CRC Miscellaneous"); //Miscellaneous
            //test = GetAreaName("Adj Prof- Miscellaneous Servic"); //Miscellaneous
            //test = GetAreaName("Adj Counslr-Misc Categorical"); //Counselor 
            //test = GetAreaName("Adj Counselor-Miscellaneous"); //Counselor 
            ////Case 6 – Set to “CJTC” WHERE Field 8 is still NULL and “CJTC” exists anywhere in the string
            //test = GetAreaName("Adjunct Professor CJTC"); //CJTC
            ////Case 7 – Has at least one dash AND there are two dashes (a dash found searching from the start and a dash found searching from the end are not in the same position
            //test = GetAreaName("Adj Prof-MAIN-Law"); //Law
            //test = GetAreaName("Adj Prof-MAIN-Eng & Ind Tech"); //Eng & Ind Tech     
            //test = GetAreaName("Adj Prof-MCCL-Psychology"); //Psychology
            //test = GetAreaName("OL Prof-MAIN-Education"); //Education
            //test = GetAreaName("SUM Prof-DAVS-Fine & App Arts"); //Fine & App Arts
            //test = GetAreaName("Adj Prof-Miscellaneou"); //Miscellaneous
            ////Case 7b – Only one dash (a dash found searching from the start and a dash found searching from the end ARE in the same position).
            //test = GetAreaName("Adjunct Professors - Fire Tech"); //Fire Tech
            //test = GetAreaName("ADJ Coord-Work Experience"); //Coord-Work Experience
            //test = GetAreaName("Adj Instr Coord - Writing Cent"); //Writing Cent
            //test = GetAreaName("Adj Coord-Natomas 037B"); //Coord-Natomas 037B
            ////Case 8 - Get entire string when Field8 value is still NULL

            ////from Excel Full-Time page
            //test = GetAreaName("Professor-Mathematics"); //Mathematics
            //test = GetAreaName("English (Reading) Professor"); //English (Reading) Professor
            //test = GetAreaName("Professor-Humanities"); //Humanities
            //test = GetAreaName("Professor-Biology"); //Biology
            //test = GetAreaName("Professor-Physical Education"); //Physical Education
            #endregion

            #region  Test GetDepartmentName
            ////Case 1 – If "COUNS" exists anywhere in the string, set field "DepartmentName" to "Counselor" 
            //test = GetDepartmentName("Adj Counslr-DSP&S-428A"); //Counselor

            ////Case 3 –If dash exists, set field "DepartmentName" to text from dash to end of string (Existing) trim leading space.
            //test = GetDepartmentName("Adj Librarian-Supplementl-014C"); //Supplementl-014C

            ////If no 'prof', take first word
            //test = GetDepartmentName("Adj Librarian FLC Supp 014C"); //Librarian FLC Supp 014C

            ////Case 1.1 – If "NURSE" exists anywhere in string, set field "DepartmentName" to "Nurse".  Note do not change "nursing"
            //test = GetDepartmentName("Adj Nurses-Unrestricted-015F"); //Nurses

            ////Case 2 – If "COORD" exists anywhere in the string, set field "DepartmentName" to text from "COORD" set field to Coord to end of string
            //test = GetDepartmentName("Adj Coord-Miscellaneous"); //Coord-Miscellaneous
            //test = GetDepartmentName("ADJ Coord-Work Experience"); //Coord-Work Experience

            ////If contain 'prof'
            ////If first four chars is not Prof, take whole field
            ////Case 3 –If dash exists, set field "DepartmentName" to text from dash to end of string (Existing) trim leading space.
            //test = GetDepartmentName("Adj Prof-TS-SCC Miscellaneous"); //TS-SCC Miscellaneous
            //test = GetDepartmentName("Adj Prof-TS-CRC Miscellaneous"); //TS-CRC Miscellaneous
            //test = GetDepartmentName("Adj Prof- Miscellaneous Servic"); //Miscellaneous Servic

            ////Case 1 – If "COUNS" exists anywhere in the string, set field "DepartmentName" to "Counselor" 
            //test = GetDepartmentName("Adj Counslr-Misc Categorical"); //Counselor 
            //test = GetDepartmentName("Adj Counselor-Miscellaneous"); //Counselor

            ////If first four chars is not Prof, take whole field
            //test = GetDepartmentName("Adjunct Professor CJTC"); //Adjunct Professor CJTC

            ////If contain 'prof'
            ////If first four chars is not Prof, take whole field
            ////Case 3 –If dash exists, set field "DepartmentName" to text from dash to end of string (Existing) trim leading space.
            //test = GetDepartmentName("Adj Prof-MAIN-Law"); //MAIN-Law
            //test = GetDepartmentName("Adj Prof-MAIN-Eng & Ind Tech"); //MAIN-Eng & Ind Tech   
            //test = GetDepartmentName("Adj Prof-MCCL-Psychology"); //MCCL-Psychology
            //test = GetDepartmentName("OL Prof-MAIN-Education"); //MAIN-Education
            //test = GetDepartmentName("SUM Prof-DAVS-Fine & App Arts"); //DAVS-Fine & App Arts
            //test = GetDepartmentName("Adj Prof-Miscellaneous"); //Miscellaneous
            //test = GetDepartmentName("Adjunct Professors - Fire Tech"); //Fire Tech

            ////Case 2 – If "COORD" exists anywhere in the string, set field "DepartmentName" to text from "COORD" set field to Coord to end of string
            //test = GetDepartmentName("ADJ Coord-Work Experience"); //Coord-Work Experience
            //test = GetDepartmentName("Adj Instr Coord - Writing Cent"); //Coord - Writing Cent
            //test = GetDepartmentName("Adj Coord-Natomas 037B"); //Coord-Natomas 037B

            ////If contain 'prof'
            ////If first four chars is Prof.., begin take after first dash - 
            //test = GetDepartmentName("Professor-Mathematics"); //Mathematics

            ////If contain 'prof'
            ////If first four chars is not Prof, take whole field
            //test = GetDepartmentName("English (Reading) Professor"); //English (Reading) Professor

            ////If contain 'prof'
            ////If first four chars is Prof.., begin take after first dash - 
            //test = GetDepartmentName("Professor-Humanities"); //Humanities
            //test = GetDepartmentName("Professor-Biology"); //Biology
            //test = GetDepartmentName("Professor-Physical Education"); //Physical Education
            #endregion

            #region Test GetCampusDescr
            //test = GetCampusCode("01ARCMAIN");
            //test = GetCampusCode("01SRPSTC");
            //test = GetCampusCode("02CRCMAIN");
            //test = GetCampusCode("04FLCMAIN");
            //test = GetCampusCode("04EDC");
            //test = GetCampusCode("05SCCMAIN");
            //test = GetCampusCode("03ETHAN");
            //test = GetCampusCode("03DO");
            #endregion

            Error error = new Error();
            List<string> errs = new List<string>();
            if (FileUpload == null)
            {
                error.errCode = ErrorDetail.Failed;
                error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Please choose file to upload";
                errs.Add(error.errMsg);
                TempData["ErrorList"] = errs;
                return RedirectToAction("AdminTasks");
            }

            string filename = FileUpload.FileName;
            var extension = filename.Split('.').Last().ToUpper();
            if (extension != "XSL" && extension != "XLSX")
            {
                error.errCode = ErrorDetail.Failed;
                error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Only Excel file format is allowed (.xls or .xlsx)";
                errs.Add(error.errMsg);
                TempData["ErrorList"] = errs;
                return RedirectToAction("AdminTasks");
            }

            if (ImportType == null)
            {
                error.errCode = ErrorDetail.Failed;
                error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Import type (Excel sheet) is not selected";
                errs.Add(error.errMsg);
                TempData["ErrorList"] = errs;
                return RedirectToAction("AdminTasks");
            }
            string filepath = "~/ImportExcel/1/";
            if (!Directory.Exists(Server.MapPath(filepath)))
                Directory.CreateDirectory(Server.MapPath(filepath));
            string targetpath = Server.MapPath(filepath);
            FileUpload.SaveAs(targetpath + filename);
            string pathToExcelFile = targetpath + filename;
            string sheetName = String.Empty;
            switch (ImportType)
            {
                case 1: errs = MembersCbuImport(pathToExcelFile, "Full time"); break;
                case 2: errs = MembersCbuImport(pathToExcelFile, "Adjunct"); break;
                case 3: errs = FacultyScheduleImport(pathToExcelFile, "REG-Schedule"); break;
                case 4: errs = FacultyScheduleImport(pathToExcelFile, "ADJ-Schedule"); break;
                default: return RedirectToAction("AdminTasks"/*, new { fileTypeSelectResult = "Select appropriate file type" }*/);
            }
            if (errs == null || errs.Count == 0)
                return RedirectToAction("Index", "Home");
            else
            {
                TempData["ErrorList"] = errs;
                return RedirectToAction("AdminTasks");
            }
        }

        #region CBU Import

        private List<string> MembersCbuImport(string pathToExcelFile, string sheetName)
        {
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            List<string> errs = new List<string>();

            var factory = new ExcelQueryFactory(pathToExcelFile);
            //Mapping ExcelMembers Model properties with an Excel fields
            factory.AddMapping<ExcelMembers>(x => x.Location, "Location");
            factory.AddMapping<ExcelMembers>(x => x.FullName, "Name");
            factory.AddMapping<ExcelMembers>(x => x.Description, "Descr");
            factory.AddMapping<ExcelMembers>(x => x.Address, "Address");
            factory.AddMapping<ExcelMembers>(x => x.City, "City");
            factory.AddMapping<ExcelMembers>(x => x.State, "St");
            factory.AddMapping<ExcelMembers>(x => x.Zip, "Postal");
            factory.AddMapping<ExcelMembers>(x => x.Phone, "Phone");
            factory.AddMapping<ExcelMembers>(x => x.Status, "Status");
            factory.AddMapping<ExcelMembers>(x => x.EmployeeID, "EmployeeID");

            factory.StrictMapping = StrictMappingType.ClassStrict;
            factory.TrimSpaces = TrimSpacesType.Both;
            factory.ReadOnly = true;
            List<ExcelMembers> members = new List<ExcelMembers>();
            try
            {
                members = factory.Worksheet<ExcelMembers>(sheetName).ToList();
            }
            catch (Exception ex)
            {
                error.errCode = ErrorDetail.Failed;
                error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!" + ex.Message;
                errs.Add(error.errMsg);
                return errs;
            }

            //Common Fields Check before
            errs = CheckCbuFields(members);
            if (errs.Count > 0)
                return errs;

            int record = 0;
            foreach (var cbuItem in members)
            {
                record++;
                try
                {
                    tb_MemberMaster FM = new tb_MemberMaster();
                    errs = SplitFullName(cbuItem.FullName, "MT", out string lastName, out string firstName, out string middleName);
                    if (errs.Count == 0)
                    {
                        //CBU.Name
                        FM.LastName = lastName;
                        FM.FirstName = firstName;
                        FM.MiddleName = middleName;
                        //CBU.EmployeeID
                        FM.MemberIDNumber = cbuItem.EmployeeID;
                    }
                    else
                    {
                        error.errMsg = "!Row #" + record.ToString();
                        errs.Add(error.errMsg);
                        return errs;
                    }

                    if (sheetName == "Full time")
                    {
                        //Adjunct or Full-Time
                        FM.JobStatusID = 2; //2 = Full-Time
                                            //Status
                        FM.DuesID = 5; // 5 = ‘Unknown - Full-time’ in tb_Dues table
                    }
                    else
                    {
                        //Adjunct or Full-Time
                        FM.JobStatusID = 1; //2 = Adjunct
                                            //Status
                        FM.DuesID = 4; // 5 = ‘Unknown - Adjunct’ in tb_Dues table
                    }
                    //CBU.Location
                    errs = GetCampusCode(cbuItem.Location, out string campusCode);
                    if (errs.Count > 0)
                        return errs;
                    errs = GetCampusID(campusCode, out int campusId);
                    if (errs.Count > 0)
                        return errs;

                    FM.CampusID = campusId;
                    //CBU.Descr (1)
                    errs = GetAreaName(cbuItem.Description, out string areaName);
                    if (errs.Count > 0)
                        return errs;
                    errs = GetAreaID(areaName, out int areaID);
                    if (errs.Count > 0)
                        return errs;
                    FM.AreaID = areaID;
                    //CBU.Descr (2)
                    errs = GetDepartmentID(GetDepartmentName(cbuItem.Description), campusId, out int departmentID);
                    if (errs.Count > 0)
                        return errs;
                    FM.DepartmentID = departmentID;
                    //DivisionID (Required field. Need to be filled)
                    FM.DivisionID = 108; //108 = 'Unknown' from tb_Division table
                                         //CategoryID (Required field. Need to be filled)
                    FM.CategoryID = 4; //4 = 'Unknown' from tb_Categories table
                    //Check is Facility Member exist in DB. Returned memberID = 0 means new member
                    errs = IsMemberExistInDB(FM.LastName, FM.FirstName, FM.MiddleName, out int memberID);
                    if (errs.Count > 0)
                        return errs;
                    FM.MemberID = memberID;

                    if (FM.MemberID == 0) // New Facility Member
                    {

                        db.tb_MemberMaster.Add(FM);

                        try
                        {
                            db.SaveChanges();
                        }
                        catch (DbEntityValidationException ex)
                        {
                            error.errCode = ErrorDetail.UnknownError;
                            error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!MembersCbuImport(...) function failed";
                            errs.Add(error.errMsg);
                            foreach (DbEntityValidationResult validationError in ex.EntityValidationErrors)
                            {
                                error.errMsg = "!Object: " + validationError.Entry.Entity.ToString() + ";";
                                errs.Add(error.errMsg);
                                foreach (DbValidationError err in validationError.ValidationErrors)
                                {
                                    error.errMsg = ">!" + err.ErrorMessage + ";";
                                    errs.Add(error.errMsg);
                                }
                            }
                            return errs;
                        }
                    }

                    errs = AssignAddress(cbuItem.Address, cbuItem.City, cbuItem.State, cbuItem.Zip, FM.MemberID);
                    if (errs.Count > 0)
                        return errs;
                    errs = AssignPhoneNumber(cbuItem.Phone, FM.MemberID);
                    if (errs.Count > 0)
                        return errs;
                }

                catch (DbEntityValidationException ex)
                {
                    error.errCode = ErrorDetail.UnknownError;
                    error.errMsg = ErrorDetail.GetMsg(error.errCode);
                    errs.Add(error.errMsg);
                    foreach (var entityValidationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in entityValidationErrors.ValidationErrors)
                        {
                            error.errMsg = ">!Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage + ";";
                            errs.Add(error.errMsg);
                        }
                    }
                    return errs;
                }

            }
            //deleting excel file from folder  
            if ((System.IO.File.Exists(pathToExcelFile)))
            {
                System.IO.File.Delete(pathToExcelFile);
            }
            return errs;
        }

        // Check excel spreadsheet fields are correct
        private List<string> CheckCbuFields(List<ExcelMembers> members)
        {
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            List<string> errs = new List<string>();
            int record = 0;
            foreach (var cbuItem in members)
            {
                record++;

                if (String.IsNullOrEmpty(cbuItem.Location))
                {
                    error.errCode = ErrorDetail.DataImportError;
                    error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Row #" + record + ". Field 'Location' is empty;";
                    errs.Add(error.errMsg);
                }

                if (String.IsNullOrEmpty(cbuItem.FullName))
                {
                    error.errCode = ErrorDetail.DataImportError;
                    error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Row #" + record + ". Field 'Name' is empty;";
                    errs.Add(error.errMsg);
                }

                if (String.IsNullOrEmpty(cbuItem.Description))
                {
                    error.errCode = ErrorDetail.DataImportError;
                    error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Row #" + record + ". Field 'Descr' is empty;";
                    errs.Add(error.errMsg);
                }

                if (String.IsNullOrEmpty(cbuItem.Address))
                {
                    error.errCode = ErrorDetail.DataImportError;
                    error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Row #" + record + ". Field 'Address'is empty;";
                    errs.Add(error.errMsg);
                }

                if (String.IsNullOrEmpty(cbuItem.City))
                {
                    error.errCode = ErrorDetail.DataImportError;
                    error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Row #" + record + ". Field 'Location' is empty;";
                    errs.Add(error.errMsg);
                }
                if (String.IsNullOrEmpty(cbuItem.State))
                {
                    error.errCode = ErrorDetail.DataImportError;
                    error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Row #" + record + ". Field 'St' is empty;";
                    errs.Add(error.errMsg);
                }
                if (String.IsNullOrEmpty(cbuItem.Zip))
                {
                    error.errCode = ErrorDetail.DataImportError;
                    error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Row #" + record + ". Field 'Postal' is empty;";
                    errs.Add(error.errMsg);
                }
                if (String.IsNullOrEmpty(cbuItem.Phone))
                {
                    error.errCode = ErrorDetail.DataImportError;
                    error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Row #" + record + ". Field 'Phone' is empty;";
                    errs.Add(error.errMsg);
                }
                if (String.IsNullOrEmpty(cbuItem.EmployeeID))
                {
                    error.errCode = ErrorDetail.DataImportError;
                    error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Row #" + record + ". Field 'EmployeeID' is empty;";
                    errs.Add(error.errMsg);
                }
            }
            return errs;
        }

        // Extract Last, First, Middle Names from FullName 
        private List<string> SplitFullName(string _fullName, string importType, out string _lastName, out string _firstName, out string _middleName)
        {
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            error.errMsg = ErrorDetail.GetMsg(error.errCode);
            List<string> errs = new List<string>();
            _lastName = String.Empty;
            _firstName = String.Empty;
            _middleName = String.Empty;

            var namesComma = _fullName.Split(',');
            if (namesComma.Length == 0)
            {
                error.errCode = ErrorDetail.DataImportError;
                error.errMsg = ErrorDetail.GetMsg(error.errCode);
                errs.Add(error.errMsg);
                if (importType == "MF")
                {
                    error.errMsg = ">!Field 'Name' is empty (Imported file);";
                    errs.Add(error.errMsg);
                }
                else if (importType == "ST")
                {
                    error.errMsg = ">!Field 'INSTRCTR' is empty (Imported file);";
                    errs.Add(error.errMsg);
                }
                }
            else if (namesComma.Length == 1)
            {
                error.errCode = ErrorDetail.DataImportError;
                error.errMsg = ErrorDetail.GetMsg(error.errCode);
                errs.Add(error.errMsg);
                if (importType == "MF")
                {
                    error.errMsg = "!Comma is absent in 'Name' field (Imported file);";
                    errs.Add(error.errMsg);
                }
                else if (importType == "ST")
                {
                    error.errMsg = "!Comma is absent in 'INSTRCTR' field (Imported file);";
                    errs.Add(error.errMsg);
                }
                }
            else if (namesComma.Length == 2)
            {
                _lastName = namesComma[0].Trim();
                var namesSpace = namesComma[1].Trim().Split(' ');
                if (namesSpace.Length == 1)
                    _firstName = namesSpace[0].Trim();
                else if (namesSpace.Length == 2)
                {
                    _firstName = namesSpace[0].Trim();
                    _middleName = namesSpace[1].Replace(".", "").Trim();
                }
            }
            return errs;
        }

        //Extract Campus Code from CBU.location
        private List<string> GetCampusCode(string location, out string campusCode)
        {
            campusCode = String.Empty;
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            error.errMsg = ErrorDetail.GetMsg(error.errCode);
            List<string> errs = new List<string>();
            string[,] s = new string[,]
            {
                {"01ARCMAIN",   "ARC"}, //ARC
                {"01SRPSTC",    "ARC"}, //ARC
                {"02CRCMAIN",   "CRC"}, //CRC
                {"04FLCMAIN",   "FLC"}, //Folsom Lk College   
                {"04EDC",       "EDC"}, //El dorado Center
                {"05SCCMAIN",   "SCC"}, //SCC
                {"03ETHAN",     "DOF"}, //District Offices (DO)
                {"03DO",        "DOF"}  //District Offices (DO)
            };
            List<string> campuses = s.Cast<string>().ToList();

            int indx = campuses.IndexOf(location);
            if (indx != -1)
                campusCode = campuses[indx + 1];
            else
            {
                error.errCode = ErrorDetail.Failed;
                error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!GetCampusCode(...) function failed. Wrong Campus Code;";
                errs.Add(error.errMsg);
            }
            return errs;
        }

        //Check if current Campus is present in tb_Campus and add it if not
        private List<string> GetCampusID(string CampusCode, out int campusID)
        {
            campusID = 0;
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            error.errMsg = ErrorDetail.GetMsg(error.errCode);
            List<string> errs = new List<string>();
            tb_Campus tb_campus = new tb_Campus();
            var campuses = db.tb_Campus.Where(t => t.CampusCode.ToUpper() == CampusCode.ToUpper() && t.CampusName.ToUpper().Contains(":MAIN"));
            if (campuses.Count() == 0)
            {
                //add new Campus
                tb_campus.CampusCode = CampusCode;
                tb_campus.CampusName = String.Empty; // ??? may be add it later with some Edit Campuses Form
                db.tb_Campus.Add(tb_campus);
                try
                {
                    db.SaveChanges();
                    campusID = tb_campus.CampusID; // new campusID of added Campus
                }
                catch (DbEntityValidationException ex)
                {
                    error.errCode = ErrorDetail.UnknownError;
                    error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!GetCampusID(...) function failed;";
                    errs.Add(error.errMsg);
                    foreach (DbEntityValidationResult validationError in ex.EntityValidationErrors)
                    {
                        error.errMsg = ">!Object: " + validationError.Entry.Entity.ToString() + ";";
                        errs.Add(error.errMsg);
                        foreach (DbValidationError err in validationError.ValidationErrors)
                        {
                            error.errMsg = ">!" + err.ErrorMessage + ";";
                            errs.Add(error.errMsg);
                        }
                    }
                    return errs;
                }
            }
            else
                //return CampusID of founded Campus
                campusID = campuses.FirstOrDefault().CampusID;

            return errs;
        }

        //Conversion CBU 'descr' to AreaName
        private List<string> GetAreaName(string descr, out string areaName)
        {
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            error.errMsg = ErrorDetail.GetMsg(error.errCode);
            List<string> errs = new List<string>();
            //add new Area
            string strTmp1, strTmp2;
            areaName = String.Empty;
            strTmp2 = descr.Substring(4, 6);
            if (descr.Length >= 10) //Exclude Substring() function issue ig Descr too short 
            {
                strTmp1 = descr.Substring(4, 4).ToUpper(); //chars 5 to 8

                //conversion rules:
                if (strTmp1 != "PROF")
                {
                    strTmp2 = descr.Substring(4, 5).ToUpper(); //chars 5 to 9
                                                               //Case 1 – If char 5 to 8 != PROF, but upper 5 to 9 does = 'COUNS' set field 'AreaName' to 'Counselor'
                    if (strTmp2 == "COUNS")
                    { areaName = "Counselor"; return errs; }

                    //Case 2 – If char 5 to 8 != PROF, AND upper 5 to 8 does = 'LIBR' set field 'AreaName' to 'Librarian' 
                    strTmp2 = descr.Substring(4, 4).ToUpper(); //chars 5 to 8
                    if (strTmp2 == "LIBR")
                    { areaName = "Librarian"; return errs; }

                    //Case 3 – If char 5 to 8 != PROF, but upper 5 to 10 does = 'Nurses' set field 'AreaName' to 'Nurses' 
                    strTmp2 = descr.Substring(4, 6).ToUpper(); //chars 5 to 10
                    if (strTmp2 == "NURSES")
                    { areaName = "Nurses"; return errs; }

                    //Case 4 – If char 5 to 8 != PROF, but upper 5 to 9 does = "COORD" set field to Coord to end of string
                    strTmp2 = descr.Substring(4, 5).ToUpper(); //chars 5 to 9
                    if (strTmp2 == "COORD")
                    {
                        StringComparison comp = StringComparison.OrdinalIgnoreCase;
                        int indx = descr.IndexOf("COORD", comp);
                        areaName = descr.Substring(indx).Trim();
                        return errs;
                    }
                }
            }

            //Case 5 – Set to 'Miscellaneous' WHERE Field 'AreaName' is still NULL and 'misc' exists anywhere in the string
            strTmp2 = descr.ToUpper();
            if (String.IsNullOrEmpty(areaName) && strTmp2.Contains("MISC"))
            { areaName = "Miscellaneous"; return errs; }

            //Case 6 – Set to 'CJTC' WHERE Field 'AreaName' is still NULL and 'CJTC' exists anywhere in the string
            strTmp2 = descr.ToUpper();
            if (String.IsNullOrEmpty(areaName) && strTmp2.Contains("CJTC"))
            { areaName = "CJTC"; return errs; }

            //Case 7a – Has at least one dash AND there are two dashes (a dash found searching from the start and a dash found searching from the end are not in the same position).
            if (strTmp2.Contains("-"))
            {
                if (strTmp2.LastIndexOf("-") != strTmp2.IndexOf("-"))
                { areaName = descr.Substring(descr.LastIndexOf("-") + 1).Trim(); return errs; }
                //Case 7b – Only one dash (a dash found searching from the start and a dash found searching from the end ARE in the same position).
                else // strTmp2.LastIndexOf("-") == strTmp2.IndexOf("-")
                { areaName = descr.Substring(descr.IndexOf("-") + 1).Trim(); return errs; }

            }

            //Case 8 - Get entire string when 'AreaName' value is still NULL
            if (String.IsNullOrEmpty(areaName))
                areaName = descr.Trim();

            return errs;
        }

        //Check if current AreaName is present in tb_Area already and add it if not
        private List<string> GetAreaID(string AreaName, out int areaID)
        {
            areaID = 0;
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            error.errMsg = ErrorDetail.GetMsg(error.errCode);
            List<string> errs = new List<string>();
            tb_Area tb_area = new tb_Area();
            var areas = db.tb_Area.Where(t => t.AreaName.ToUpper() == AreaName.ToUpper());
            if (areas.Count() == 0)
            {
                tb_area.AreaName = AreaName;
                tb_area.AreaDesc = String.Empty; //??? may be add it later with some Edit Area Form
                db.tb_Area.Add(tb_area);
                try
                {
                    db.SaveChanges();
                    areaID = tb_area.AreaID; // new AreaID of added Area
                }
                catch (DbEntityValidationException ex)
                {
                    error.errCode = ErrorDetail.UnknownError;
                    error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!GetAreaName(...) function failed;";
                    errs.Add(error.errMsg);
                    foreach (DbEntityValidationResult validationError in ex.EntityValidationErrors)
                    {
                        error.errMsg = ">!Object: " + validationError.Entry.Entity.ToString() + ";";
                        errs.Add(error.errMsg);
                        foreach (DbValidationError err in validationError.ValidationErrors)
                        {
                            error.errMsg = ">!" + err.ErrorMessage + ";";
                            errs.Add(error.errMsg);
                        }
                    }
                    return errs;
                }
            }
            else
                //return AreaID of founded Area
                areaID = areas.FirstOrDefault().AreaID;
            return errs;
        }

        //Conversion CBU 'descr' to DepartmentName
        private string GetDepartmentName(string descr)
        {
            //add new Department
            string strTmp1, strTmp2 = String.Empty;
            string departmentName = descr.Trim(); //If no match on rule, provide entire string 

            //conversion rules:
            if (descr.ToUpper().Contains("PROF")) //If contain 'prof'
            {
                strTmp1 = descr.Substring(0, 4).ToUpper();
                if (strTmp1 != "PROF") //If first four chars is not Prof, take whole field
                    departmentName = descr.Trim();
                else //If first four chars is Prof.., begin take after first dash - 
                    departmentName = descr.Substring(descr.IndexOf("-") + 1).Trim();
            }
            else //If no 'prof', take first word
                departmentName = descr.Substring(descr.IndexOf(" ") + 1).Trim();

            //Case 1 – If "COUNS" exists anywhere in the string, set field "DepartmentName" to "Counselor" 
            if (descr.ToUpper().Contains("COUNS"))
                return "Counselor";

            //Case 1.1 – If "NURSE" exists anywhere in string, set field "DepartmentName" to "Nurse".  Note do not change "nursing" 
            if (descr.ToUpper().Contains("NURSE") && !descr.ToUpper().Contains("NURSING"))
                return "Nurse";

            //Case 2 – If "COORD" exists anywhere in the string, set field "DepartmentName" to text from "COORD" set field to Coord to end of string
            if (descr.ToUpper().Contains("COORD"))
            {
                StringComparison comp = StringComparison.OrdinalIgnoreCase;
                int indx = descr.IndexOf("COORD", comp);
                return descr.Substring(indx).Trim();
            }

            //Case 3 –If dash exists, set field "DepartmentName" to text from dash to end of string (Existing) trim leading space.
            if (departmentName.Contains("-"))
                departmentName = departmentName.Substring(departmentName.IndexOf("-") + 1).Trim();

            return departmentName;
        }

        //Check if current DepartmentName is present in tb_Department already and add it if not
        private List<string> GetDepartmentID(string DepartmentName, int CampusID, out int departmentID)
        {
            departmentID = 0;
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            error.errMsg = ErrorDetail.GetMsg(error.errCode);
            List<string> errs = new List<string>();
            tb_Department tb_department = new tb_Department();
            var departments = db.tb_Department.Where(t => t.DepartmentName.ToUpper() == DepartmentName.ToUpper());
            if (departments.Count() == 0)
            {
                tb_department.DepartmentName = DepartmentName;
                tb_department.CollegeID = db.tb_Campus.Find(CampusID).CollegeID; //from founded before CampusID
                db.tb_Department.Add(tb_department);
                try
                {
                    db.SaveChanges();
                    departmentID = tb_department.DepartmentID; // new DepartmentID of added Department
                }
                catch (DbEntityValidationException ex)
                {
                    error.errCode = ErrorDetail.UnknownError;
                    error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!GetDepartmentID(...) function failed;";
                    errs.Add(error.errMsg);
                    foreach (DbEntityValidationResult validationError in ex.EntityValidationErrors)
                    {
                        error.errMsg = ">!Object: " + validationError.Entry.Entity.ToString() + ";";
                        errs.Add(error.errMsg);
                        foreach (DbValidationError err in validationError.ValidationErrors)
                        {
                            error.errMsg = ">!" + err.ErrorMessage + ";";
                            errs.Add(error.errMsg);
                        }
                    }
                    return errs;
                }
            }
            else
                //return AreaID of founded Area
                departmentID = departments.FirstOrDefault().DepartmentID;

            return errs;
        }

        //Check if current City is present in tb_CityState and add it if not
        private int GetCityID(string city)
        {
            tb_CityState tb_city = new tb_CityState();

            if (db.tb_CityState.Where(t => t.CityName.ToUpper() == city.ToUpper()).Count() == 0)
            {
                tb_city.CityName = city;
                tb_city.StateCodeID = 1; //we have only 1 record for now
                db.tb_CityState.Add(tb_city);
                db.SaveChanges();
            }
            return db.tb_CityState.Where(t => t.CityName.ToUpper() == city.ToUpper()).FirstOrDefault().CityID;
        }

        //Find memberID by CBU Full Name. Return memberID = 0 if not found
        private List<string> IsMemberExistInDB(string lastname, string firstname, string middlename, out int memberID)
        {
            memberID = 0;
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            error.errMsg = ErrorDetail.GetMsg(error.errCode);
            List<string> errs = new List<string>();
            //1. Find memberID by Full Name
            try
            {
                var fms = db.tb_MemberMaster.Where(s => s.LastName.ToUpper() == lastname.ToUpper() &&
                s.FirstName.ToUpper() == firstname.ToUpper() &&
                s.MiddleName.ToUpper() == middlename.ToUpper());
                //2. If such member was found
                if (fms.Count() > 0)
                    memberID = fms.FirstOrDefault().MemberID; //set MemberID
            }
            catch (Exception ex)
            {
                error.errCode = ErrorDetail.UnknownError;
                error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!IsMemberExistInDB(...) function failed." + ex.Message + ";";
                errs.Add(error.errMsg);
                return errs;
            }
            return errs;
        }

        //Get tb_MemberAddress record for current Member
        //Assign MemberID for existing Member or return tb_MemberAddress.MemberID = 0 for new one
        private List<string> AssignAddress(string address, string city, string st, string postal, int mID)
        {
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            error.errMsg = ErrorDetail.GetMsg(error.errCode);
            List<string> errs = new List<string>();
            int cityId = GetCityID(city);
            tb_MemberAddress ma = new tb_MemberAddress();
            // (!) We need here to set IsPrimary = false for all other addresses (have to be done)
            var memberAddresses = db.tb_MemberAddress.Where(s => s.MemberID == mID
                && s.HomeStreet1.ToUpper() == address.ToUpper()
                && s.CityID == cityId
                && s.ZipCode.ToUpper() == postal.ToUpper());
            //Checking if address from the list of current member addresses already exist
            if (memberAddresses.Count() > 0)
            {
                //Just return founded same as in CBU old address with current memberID
                ma = memberAddresses.FirstOrDefault();
                ma.IsPrimary = true;
                //db.tb_MemberAddress.Attach(ma);
                var entry = db.Entry(ma);
            }
            //Current member hasn't address as in CBU
            else
            {
                ma.MemberID = mID;
                ma.HomeStreet1 = address;
                ma.CityID = cityId;
                ma.ZipCode = postal;
                ma.Country = "USA";
                ma.IsPrimary = true;
                ma.AddressTypeID = 1; //Mailing (from tb_AddressType table)
                ma.SourceID = 2; //Employer
                ma.Source = "Employer";
                ma.CreatedDateTime = DateTime.UtcNow;
                db.tb_MemberAddress.Add(ma);
            }

            try
            {
                db.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                error.errCode = ErrorDetail.UnknownError;
                error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!AssignAddress(...) function failed;";
                errs.Add(error.errMsg);
                foreach (DbEntityValidationResult validationError in ex.EntityValidationErrors)
                {
                    error.errMsg = ">!Object: " + validationError.Entry.Entity.ToString() + ";";
                    errs.Add(error.errMsg);
                    foreach (DbValidationError err in validationError.ValidationErrors)
                    {
                        error.errMsg = ">!" + err.ErrorMessage + ";";
                        errs.Add(error.errMsg);
                    }
                }
                return errs;
            }
            return errs;
        }

        //Assign tb_MemberPhoneNumbers record for current Member
        private List<string> AssignPhoneNumber(string phone, int mID)
        {
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            error.errMsg = ErrorDetail.GetMsg(error.errCode);
            List<string> errs = new List<string>();
            //Find all member phones by memberID
            tb_MemberPhoneNumbers mp = new tb_MemberPhoneNumbers();
            //Check if phone from the list of current member phone already exist
            var memberPhoneNumbers = db.tb_MemberPhoneNumbers.Where(s => s.MemberID == mID && s.PhoneNumber.ToUpper() == phone.ToUpper());
            if (memberPhoneNumbers.Count() > 0)
            {
                //Obtained record with a same as in CBU phone number. Just set IsPrimary = true
                mp = memberPhoneNumbers.FirstOrDefault();
                mp.IsPrimary = true;
                // db.tb_MemberPhoneNumbers.Attach(mp);
                var entry = db.Entry(mp);
            }
            //Current member hasn't phone as in CBU
            else
            {
                //Create the record with new phone from CBU
                mp.MemberID = mID;
                mp.IsPrimary = true;
                mp.PhoneNumber = phone;
                mp.PhoneTypeID = 4; //Unknown
                mp.Source = "Employer";
                mp.CreatedDateTime = DateTime.UtcNow;
                db.tb_MemberPhoneNumbers.Add(mp);
            }

            try
            {
                db.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                error.errCode = ErrorDetail.UnknownError;
                error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!AssignPhoneNumber(...) function failed;";
                errs.Add(error.errMsg);
                foreach (DbEntityValidationResult validationError in ex.EntityValidationErrors)
                {
                    error.errMsg = ">!Object: " + validationError.Entry.Entity.ToString() + ";";
                    errs.Add(error.errMsg);
                    foreach (DbValidationError err in validationError.ValidationErrors)
                    {
                        error.errMsg = ">!" + err.ErrorMessage + ";";
                        errs.Add(error.errMsg);
                    }
                }
                return errs;
            }
            return errs;
        }
        #endregion

        #region Faculty Schedule Import
        ///<Author>Alex</Author>
        /// <summary>
        /// Fill Out Schedule from Excel file
        /// </summary>       
        /// <returns>Status code</returns>
        private List<string> FacultyScheduleImport(string pathToExcelFile, string sheetName)
        {
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            List<string> errs = new List<string>();
            var factory = new ExcelQueryFactory(pathToExcelFile);
            //Mapping ExcelSchedules Model properties with an Excel fields
            factory.AddMapping<ExcelSchedules>(x => x.Id, "ID");
            factory.AddMapping<ExcelSchedules>(x => x.Instructor, "INSTRCTR");
            factory.AddMapping<ExcelSchedules>(x => x.Campus, "CAMPUS");
            factory.AddMapping<ExcelSchedules>(x => x.Location, "LOCATION");
            factory.AddMapping<ExcelSchedules>(x => x.Building, "BUILDING");
            factory.AddMapping<ExcelSchedules>(x => x.Room, "ROOM");
            factory.AddMapping<ExcelSchedules>(x => x.Division, "DIV");
            factory.AddMapping<ExcelSchedules>(x => x.ClassNumber, "CLASS #");
            factory.AddMapping<ExcelSchedules>(x => x.CAT_NBR, "CAT NBR"); //?
            factory.AddMapping<ExcelSchedules>(x => x.Sect, "SECT"); //?
            factory.AddMapping<ExcelSchedules>(x => x.Subject, "SUBJ CD"); //?
            factory.AddMapping<ExcelSchedules>(x => x.LecOrLab, "LEC LAB");
            factory.AddMapping<ExcelSchedules>(x => x.SB_TM, "SB TM"); //?
            factory.AddMapping<ExcelSchedules>(x => x.ATT_TP, "ATT TP"); //?
            factory.AddMapping<ExcelSchedules>(x => x.BeginTime, "BEG TIME");
            factory.AddMapping<ExcelSchedules>(x => x.EndTime, "END TIME");
            factory.AddMapping<ExcelSchedules>(x => x.Days, "DAYS");
            factory.AddMapping<ExcelSchedules>(x => x.ClassEndDate, "CLASS END DT");

            factory.StrictMapping = StrictMappingType.ClassStrict;
            factory.TrimSpaces = TrimSpacesType.Both;
            factory.ReadOnly = true;

            List<ExcelSchedules> schedules = new List<ExcelSchedules>();
            try
            {
                schedules = factory.Worksheet<ExcelSchedules>(sheetName).ToList();
            }
            catch (Exception ex)
            {
                error.errCode = ErrorDetail.Failed;
                error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!" + ex.Message;
                errs.Add(error.errMsg);
                return errs;
            }
            //Common Fields Check before
            errs = CheckScheduleFields(schedules);
            if (errs.Count > 0)
                return errs;

            int record = 0;
            foreach (var item in schedules)
            {
                record++;
                tb_SemesterTaught ST = new tb_SemesterTaught();
                errs = SplitFullName(item.Instructor, "ST", out string lastName, out string firstName, out string middleName);
                if (errs.Count > 0)
                    return errs;

                errs = IsMemberExistInDB(lastName, firstName, middleName, out int memberID);
                if (errs.Count > 0)
                    return errs;

                if (memberID == 0)
                {
                    error.errCode = ErrorDetail.Failed;
                    error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Row #" + record.ToString() + ". Instructor name: " + item.Instructor + " wasn't found in the DataBase";
                    errs.Add(error.errMsg);
                    return errs;
                }
                ST.MemberID = memberID;

                errs = GetSemesterRecID(item.ClassEndDate.Trim(), record, out int semesterRecID, out bool scheduleStatus);
                if (errs.Count > 0)
                    return errs;
                ST.SemesterRecID = semesterRecID;
                ST.ScheduleStatus = scheduleStatus;

                errs = GetClassWeekDayID(item.Days, record, out int classWeekDayID);
                if (errs.Count > 0)
                    return errs;
                ST.ClassWeekDayID = classWeekDayID;

                ST.Class = item.ClassNumber.Trim();

                item.BeginTime = item.BeginTime.Trim().Insert(5, " ");
                ST.ClassStart = DateTime.ParseExact(item.BeginTime.Trim(), "hh:mm tt", CultureInfo.InvariantCulture).TimeOfDay;

                item.EndTime = item.EndTime.Trim().Insert(5, " ");
                ST.ClassEnd = DateTime.ParseExact(item.EndTime.Trim(), "hh:mm tt", CultureInfo.InvariantCulture).TimeOfDay;

                errs = GetBuildingID(item.Building, item.Campus, out int buildingID);
                if (errs.Count > 0)
                    return errs;
                ST.BuildingID = buildingID;

                if (String.IsNullOrEmpty(item.Room) && item.Building.ToUpper() == "ONLINE")
                    ST.Room = "ONLINE";
                else
                    ST.Room = item.Room.Trim();

                //Check dublicates
                var _st = db.tb_SemesterTaught.Where(s => s.SemesterRecID == ST.SemesterRecID
                    && s.MemberID == ST.MemberID
                    && s.Room.ToUpper() == ST.Room.ToUpper()
                    && s.Class.ToUpper() == ST.Class.ToUpper()
                    && s.ClassStart == ST.ClassStart
                    && s.ClassEnd == ST.ClassEnd
                    && s.ClassWeekDayID == ST.ClassWeekDayID
                    && s.BuildingID == ST.BuildingID);
                if (_st.ToList().Count == 0) // Add new
                {
                    db.tb_SemesterTaught.Add(ST);
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (DbEntityValidationException ex)
                    {
                        error.errCode = ErrorDetail.UnknownError;
                        error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!FacultyScheduleImport(...) function failed";
                        errs.Add(error.errMsg);
                        foreach (DbEntityValidationResult validationError in ex.EntityValidationErrors)
                        {
                            error.errMsg = ">!Object: " + validationError.Entry.Entity.ToString() + ";";
                            errs.Add(error.errMsg);
                            foreach (DbValidationError err in validationError.ValidationErrors)
                            {
                                error.errMsg = ">!" + err.ErrorMessage + ";";
                                errs.Add(error.errMsg);
                            }
                        }
                        return errs;
                    }
                }
            }
            //deleting excel file from folder  
            if ((System.IO.File.Exists(pathToExcelFile)))
            {
                System.IO.File.Delete(pathToExcelFile);
            }

            return errs;
        }

        // Check excel spreadsheet fields are correct
        private List<string> CheckScheduleFields(List<ExcelSchedules> schedules)
        {
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            WeekDay wd = new WeekDay();
            List<string> errs = new List<string>();
            IEnumerable<SelectListItem> campuses = db.tb_Campus
                                          .GroupBy(t => t.CampusCode)
                                          .Select(g => g.FirstOrDefault())
                                          .Select(c => new SelectListItem
                                          {
                                              Value = c.CampusID.ToString(),
                                              Text = c.CampusCode
                                          });

            int record = 1;
            foreach (var _item in schedules)
            {
                record++;

                if (String.IsNullOrEmpty(_item.Instructor))
                {
                    error.errCode = ErrorDetail.DataImportError;
                    error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Row #" + record + ". Field 'INSTRCTR' is empty;";
                    errs.Add(error.errMsg);
                }

                if (String.IsNullOrEmpty(_item.Campus))
                {
                    error.errCode = ErrorDetail.DataImportError;
                    error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Row #" + record + ". Field 'CAMPUS' is empty;";
                    errs.Add(error.errMsg);
                }

                if (campuses.Where(c => c.Text == _item.Campus).Count() <= 0)
                {
                    error.errCode = ErrorDetail.DataImportError;
                    error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Row #" + record + ", column 'CAMPUS'. Value: " + _item.Campus + " not exist in the tb_Campus table;";
                    errs.Add(error.errMsg);
                }

                if (String.IsNullOrEmpty(_item.Location))
                {
                    error.errCode = ErrorDetail.DataImportError;
                    error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Row #" + record + ". Field 'LOCATION' is empty;";
                    errs.Add(error.errMsg);
                }

                if (String.IsNullOrEmpty(_item.Building))
                {
                    error.errCode = ErrorDetail.DataImportError;
                    error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Row #" + record + ". Field 'BUILDING' is empty;";
                    errs.Add(error.errMsg);
                }

                if (String.IsNullOrEmpty(_item.Room) && !String.IsNullOrEmpty(_item.Building))
                {
                    if (_item.Building.ToUpper() != "ONLINE")
                    {
                        error.errCode = ErrorDetail.DataImportError;
                        error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Row #" + record + ". Field 'ROOM' can be empty for 'BUILDING' = 'Online' only;";
                        errs.Add(error.errMsg);
                    }
                }
                //if (String.IsNullOrEmpty(_item.Division))
                //{
                //    error.errCode = ErrorDetail.DataImportError;
                //    error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Row #" + record + ". Field 'DIV' is empty;";
                //    errs.Add(error.errMsg);
                //}
                if (String.IsNullOrEmpty(_item.ClassNumber))
                {
                    error.errCode = ErrorDetail.DataImportError;
                    error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Row #" + record + ". Field 'CLASS #' is empty;";
                    errs.Add(error.errMsg);
                }
                if (String.IsNullOrEmpty(_item.BeginTime))
                {
                    error.errCode = ErrorDetail.DataImportError;
                    error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Row #" + record + ". Field 'BEG TIME' is empty;";
                    errs.Add(error.errMsg);
                }
                try
                {
                    var check = DateTime.ParseExact(_item.BeginTime.Trim().Insert(5, " "), "hh:mm tt", CultureInfo.InvariantCulture).TimeOfDay;
                }
                catch (Exception)
                {
                    error.errCode = ErrorDetail.DataImportError;
                    error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Row #" + record + ". Field 'BEG TIME' wrong format;";
                    errs.Add(error.errMsg);
                }
                if (String.IsNullOrEmpty(_item.EndTime))
                {
                    error.errCode = ErrorDetail.DataImportError;
                    error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Row #" + record + ". Field 'END TIME' is empty;";
                    errs.Add(error.errMsg);
                }
                try
                {
                    var check = DateTime.ParseExact(_item.EndTime.Trim().Insert(5, " "), "hh:mm tt", CultureInfo.InvariantCulture).TimeOfDay;
                }
                catch (Exception)
                {
                    error.errCode = ErrorDetail.DataImportError;
                    error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Row #" + record + ". Field 'END TIME' wrong format;";
                    errs.Add(error.errMsg);
                }
                if (String.IsNullOrEmpty(_item.Days))
                {
                    error.errCode = ErrorDetail.DataImportError;
                    error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Row #" + record + ". Field 'DAYS' is empty;";
                    errs.Add(error.errMsg);
                }
                else
                {
                    wd.codeInExcel = _item.Days;
                    wd.codeInDB = WeekDaysMatch.GetDaysCode(wd.codeInExcel);
                    if (wd.codeInDB == "Error")
                    {
                        error.errCode = ErrorDetail.Failed;
                        error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Row #" + record.ToString() + ". Field 'DAYS' not matching any record in the WeekDaysMatch class";
                        errs.Add(error.errMsg);
                        return errs;
                    }
                }

                if (String.IsNullOrEmpty(_item.ClassEndDate))
                {
                    error.errCode = ErrorDetail.DataImportError;
                    error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Row #" + record + ". Field 'CLASS END DT' is empty;";
                    errs.Add(error.errMsg);
                }
                try
                {
                    var check = DateTime.ParseExact(_item.ClassEndDate.Trim(), "MM-dd-yyyy", CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    error.errCode = ErrorDetail.DataImportError;
                    error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Row #" + record + ". Field 'CLASS END DT' wrong format;";
                    errs.Add(error.errMsg);
                }
            }
            return errs;
        }

        private List<string> GetSemesterRecID(string classEndDate, int rec, out int semesterRecID, out bool scheduleStatus)
        {
            semesterRecID = 0;
            scheduleStatus = false;
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            List<string> errs = new List<string>();
            var endDate = DateTime.ParseExact(classEndDate.Trim(), "MM-dd-yyyy", CultureInfo.InvariantCulture);
            try
            {
                semesterRecID = db.tb_Semesters.Where(t => t.FiscalYear == endDate.Year.ToString())
                    .Where(t => t.DateFrom <= endDate)
                    .Where(t => t.DateTo >= endDate).FirstOrDefault().SemesterRecID;
            }
            catch (Exception)
            {
                error.errCode = ErrorDetail.Failed;
                error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Row #" + rec.ToString() + ". Class End Date: " + classEndDate + " There is no apropriate semester in the tb_Semesters table.";
                errs.Add(error.errMsg);
                return errs;
            }
            if (semesterRecID <= 0)
            {
                error.errCode = ErrorDetail.Failed;
                error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Row #" + rec.ToString() + ". Class End Date: " + classEndDate + " There is no apropriate semester in the tb_Semesters table.";
                errs.Add(error.errMsg);
                return errs;
            }
            if (endDate >= DateTime.UtcNow)
                scheduleStatus = true;

            return errs;
        }

        private List<string> GetClassWeekDayID(string days, int rec, out int classWeekDayID)
        {
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            error.errMsg = ErrorDetail.GetMsg(error.errCode);
            List<string> errs = new List<string>();
            classWeekDayID = 0;
            WeekDay wd = new WeekDay();
            if (!String.IsNullOrEmpty(days))
            {
                wd.codeInExcel = days.Trim();
                wd.codeInDB = WeekDaysMatch.GetDaysCode(wd.codeInExcel);
            }
            else
            {
                error.errCode = ErrorDetail.Failed;
                error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Row #" + rec.ToString() + ". Field 'DAYS' is empty.";
                errs.Add(error.errMsg);
                return errs;
            }
            try
            {
                classWeekDayID = db.tb_WeekDay.Where(t => t.WeekDayName == wd.codeInDB).FirstOrDefault().ClassWeekDayID;
            }
            catch (Exception ex)
            {
                error.errCode = ErrorDetail.Failed;
                error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Row #" + rec.ToString() + ". Field 'DAYS' not matching any record in the tb_WeekDay table." + ex.Message;
                errs.Add(error.errMsg);
                return errs;
            }

            if (classWeekDayID <= 0)
            {
                error.errCode = ErrorDetail.Failed;
                error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Row #" + rec.ToString() + ". Field 'DAYS' not matching any record in the tb_WeekDay table.";
                errs.Add(error.errMsg);
                return errs;
            }

            return errs;
        }

        //Check if current AreaName is present in tb_Area already and add it if not
        private List<string> GetBuildingID(string building, string campus, out int buildingID)
        {
            buildingID = 0;
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            error.errMsg = ErrorDetail.GetMsg(error.errCode);
            List<string> errs = new List<string>();
            tb_Building tb_building = new tb_Building();
            var buildings = db.tb_Building.Where(t => t.BuildingName.ToUpper() == building.ToUpper());
            if (buildings.Count() == 0)
            {
                tb_building.BuildingName = building;

                errs = GetCampusID(campus, out int campusId);
                if (error.errCode != ErrorDetail.Success)
                    return errs;

                tb_building.CampusID = campusId;

                db.tb_Building.Add(tb_building);
                try
                {
                    db.SaveChanges();
                    buildingID = tb_building.BuildingID; // new BuildingID of added Building
                }
                catch (DbEntityValidationException ex)
                {
                    error.errCode = ErrorDetail.UnknownError;
                    error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!GetBuildingID(...) function failed;";
                    errs.Add(error.errMsg);
                    foreach (DbEntityValidationResult validationError in ex.EntityValidationErrors)
                    {
                        error.errMsg = "!>Object: " + validationError.Entry.Entity.ToString() + ";";
                        errs.Add(error.errMsg);
                        foreach (DbValidationError err in validationError.ValidationErrors)
                        {
                            error.errMsg = ">!" + err.ErrorMessage + ";";
                            errs.Add(error.errMsg);
                        }
                    }
                    return errs;
                }
            }
            else
                //return BuildingID of founded Building
                buildingID = buildings.FirstOrDefault().BuildingID;
            return errs;
        }

        #endregion

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