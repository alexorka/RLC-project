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

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
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
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
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
                var user = new ApplicationUser { UserName = selectedRole, Email = model.Email }; // placing selected Role Name to User Name in AspNetUsers table
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
        // GET: /Account/AdminTasks
        [Authorize(Roles = "admin")]
        public ActionResult AdminTasks(string xlsSelectResult, string fileTypeSelectResult)
        {
        var _users = db.AspNetUsers.ToList();
            List<SelectListItem> _UsersRoles = new List<SelectListItem>();
            foreach (var _user in _users)
            {
                string  currentRoles = UserManager.GetRolesAsync(_user.Id).Result.FirstOrDefault();
                if (String.IsNullOrEmpty(currentRoles))
                    _UsersRoles.Add(new SelectListItem() { Text = _user.Email + ":" + _user.UserName, Value = currentRoles });
                else
                    _UsersRoles.Add(new SelectListItem() { Text = _user.Email, Value = currentRoles });
            }
            ViewBag.UsersAndRoles = _UsersRoles;
            ViewBag.ResultMessage = xlsSelectResult ?? String.Empty;
            ViewBag.FileTypeMessage = fileTypeSelectResult ?? String.Empty;

            return View();
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
            //var uNameRole = userName.Split(':');
            //string uName = uNameRole[0].Trim();
            //string uRole = uNameRole[1].Trim();
            var user = await UserManager.FindByEmailAsync(userName.Trim());
            string currentRoles = UserManager.GetRolesAsync(user.Id).Result.FirstOrDefault();
            var result = await UserManager.RemoveFromRoleAsync(user.Id, currentRoles);

            //result = await UserManager.AddToRoleAsync(user.Id, currentRoles);
            if (result.Succeeded)
            {
                user.UserName = currentRoles; //Place RoleName to UserName field in AspNetUsers table. To keep it for confirmation
                //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
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

        /// <summary>  
        /// This function is used to download excel format.  
        /// </summary>  
        /// <param name="Path"></param>  
        /// <returns>file</returns>  
        public FileResult DownloadExcel()
        {
            string path = "/AdminTasks/CBU.xlsx";
            return File(path, "application/vnd.ms-excel", "CBU.xlsx");
        }

        // Extract Last, First, Middle Names from FullName 
        private string SplitFullName(string _fullName, out string _lastName, out string _firstName, out string _middleName)
        {
            string result = "Success";
            _lastName = String.Empty;
            _firstName = String.Empty;
            _middleName = String.Empty;
            var namesComma = _fullName.Split(',');
            if (namesComma.Length == 0)
                result = "Empty 'Name' field";
            else if (namesComma.Length == 1)
                result = "Comma is absent in 'Name' field";
            else if (namesComma.Length == 2)
            {
                _lastName = namesComma[0];
                var namesSpace = namesComma[1].Split(' ');
                if (namesSpace.Length == 1)
                    _firstName = namesSpace[0];
                else if (namesSpace.Length == 2)
                {
                    _firstName = namesSpace[0];
                    _middleName = namesSpace[1];
                }
            }
            return result;
        }

        //Extract Campus Code from CBU.location
        private string GetCampusCode(string location)
        {
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
            
            return campuses[indx + 1];
        }

        //Check if current Campus is present in tb_Campus and add it if not
        private int GetCampusID(string CampusCode)
        {
            int campusID = 0;
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
                    foreach (DbEntityValidationResult validationError in ex.EntityValidationErrors)
                    {
                        Response.Write("Object: " + validationError.Entry.Entity.ToString());
                        Response.Write("");
                        foreach (DbValidationError err in validationError.ValidationErrors)
                        {
                            Response.Write(err.ErrorMessage + "");
                        }
                    }
                }
            }
            else
                //return CampusID of founded Campus
                campusID = campuses.FirstOrDefault().CampusID;

            return campusID;
        }

        //Conversion CBU 'descr' to AreaName
        private string GetAreaName(string descr)
        {
            //add new Area
            string strTmp1 = descr.Substring(4, 4).ToUpper(); //chars 5 to 8
            string strTmp2, AreaName = String.Empty;

            //conversion rules:
            if (strTmp1 != "PROF")
            {
                strTmp2 = descr.Substring(4, 5).ToUpper(); //chars 5 to 9
                //Case 1 – If char 5 to 8 != PROF, but upper 5 to 9 does = 'COUNS' set field 'AreaName' to 'Counselor'
                if (strTmp2 == "COUNS")
                    return "Counselor";

                //Case 2 – If char 5 to 8 != PROF, AND upper 5 to 8 does = 'LIBR' set field 'AreaName' to 'Librarian' 
                strTmp2 = descr.Substring(4, 4).ToUpper(); //chars 5 to 8
                if (strTmp2 == "LIBR")
                    return "Librarian";

                //Case 3 – If char 5 to 8 != PROF, but upper 5 to 10 does = 'Nurses' set field 'AreaName' to 'Nurses' 
                strTmp2 = descr.Substring(4, 6).ToUpper(); //chars 5 to 10
                if (strTmp2 == "NURSES")
                    return "Nurses";
                  
                //Case 4 – If char 5 to 8 != PROF, but upper 5 to 9 does = "COORD" set field to Coord to end of string
                strTmp2 = descr.Substring(4, 5).ToUpper(); //chars 5 to 9
                if (strTmp2 == "COORD")
                {
                    StringComparison comp = StringComparison.OrdinalIgnoreCase;
                    int indx = descr.IndexOf("COORD", comp);
                    return descr.Substring(indx).Trim();
                }
            }

            //Case 5 – Set to 'Miscellaneous' WHERE Field 'AreaName' is still NULL and 'misc' exists anywhere in the string
            strTmp2 = descr.ToUpper();
            if (String.IsNullOrEmpty(AreaName) && strTmp2.Contains("MISC"))
                return "Miscellaneous";

            //Case 6 – Set to 'CJTC' WHERE Field 'AreaName' is still NULL and 'CJTC' exists anywhere in the string
            strTmp2 = descr.ToUpper();
            if (String.IsNullOrEmpty(AreaName) && strTmp2.Contains("CJTC"))
            {
                return "CJTC";
            }
                
            //Case 7a – Has at least one dash AND there are two dashes (a dash found searching from the start and a dash found searching from the end are not in the same position).
            if (strTmp2.Contains("-"))
            {
                if (strTmp2.LastIndexOf("-") != strTmp2.IndexOf("-"))
                    return descr.Substring(descr.LastIndexOf("-") + 1).Trim();
                //Case 7b – Only one dash (a dash found searching from the start and a dash found searching from the end ARE in the same position).
                else // strTmp2.LastIndexOf("-") == strTmp2.IndexOf("-")
                    return descr.Substring(descr.IndexOf("-") + 1).Trim();
            }
           
            //Case 8 - Get entire string when 'AreaName' value is still NULL
            if (String.IsNullOrEmpty(AreaName))
                AreaName = descr.Trim();

            return AreaName.Trim();
        }

        //Check if current AreaName is present in tb_Area already and add it if not
        private int GetAreaID(string AreaName)
        {
            int areaID = 0;
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
                    foreach (DbEntityValidationResult validationError in ex.EntityValidationErrors)
                    {
                        Response.Write("Object: " + validationError.Entry.Entity.ToString());
                        Response.Write("");
                        foreach (DbValidationError err in validationError.ValidationErrors)
                        {
                            Response.Write(err.ErrorMessage + "");
                        }
                    }
                }
            }
            else
                //return AreaID of founded Area
                areaID = areas.FirstOrDefault().AreaID;

            return areaID;
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
        private int GetDepartmentID(string DepartmentName, int CampusID)
        {
            int departmentID = 0;
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
                    foreach (DbEntityValidationResult validationError in ex.EntityValidationErrors)
                    {
                        Response.Write("Object: " + validationError.Entry.Entity.ToString());
                        Response.Write("");
                        foreach (DbValidationError err in validationError.ValidationErrors)
                        {
                            Response.Write(err.ErrorMessage + "");
                        }
                    }
                }
            }
            else
                //return AreaID of founded Area
                departmentID = departments.FirstOrDefault().DepartmentID;

            return departmentID;
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
        private int IsMemberExistInDB(string lastname, string firstname, string middlename)
        {
            int mID = 0;
            //1. Find memberID by Full Name
            var fms = db.tb_MemberMaster.Where(s => s.LastName.ToUpper() == lastname.ToUpper() &&
            s.FirstName.ToUpper() == firstname.ToUpper() &&
            s.MiddleName.ToUpper() == middlename.ToUpper());
            //2. If such member was found
            if (fms.Count() > 0) mID = fms.FirstOrDefault().MemberID; //set MemberID
            return mID;
        }

        //Get tb_MemberAddress record for current Member
        //Assign MemberID for existing Member or return tb_MemberAddress.MemberID = 0 for new one
        private void AssignAddress(string address, string city, string st, string postal, int mID)
        {
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
                foreach (DbEntityValidationResult validationError in ex.EntityValidationErrors)
                {
                    Response.Write("Object: " + validationError.Entry.Entity.ToString());
                    Response.Write("");
                    foreach (DbValidationError err in validationError.ValidationErrors)
                    {
                        Response.Write(err.ErrorMessage + "");
                    }
                }
            }
        }

        //Assign tb_MemberPhoneNumbers record for current Member
        private void AssignPhoneNumber(string phone, int mID)
        {
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
                foreach (DbEntityValidationResult validationError in ex.EntityValidationErrors)
                {
                    Response.Write("Object: " + validationError.Entry.Entity.ToString());
                    Response.Write("");
                    foreach (DbValidationError err in validationError.ValidationErrors)
                    {
                        Response.Write(err.ErrorMessage + "");
                    }
                }
            }
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

            string message = String.Empty;
            List<string> data = new List<string>();
            if (FileUpload != null)
            {
                // tdata.ExecuteCommand("truncate table OtherCompanyAssets");  
                if (FileUpload.ContentType == "application/vnd.ms-excel" || FileUpload.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    string filename = FileUpload.FileName;
                    string filepath = "~/ImportExcel/1/";
                    if (!Directory.Exists(Server.MapPath(filepath)))
                        Directory.CreateDirectory(Server.MapPath(filepath));
                    string targetpath = Server.MapPath(filepath);
                    FileUpload.SaveAs(targetpath + filename);
                    string pathToExcelFile = targetpath + filename;
                    var connectionString = "";
                    if (filename.EndsWith(".xls"))
                    {
                        connectionString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0; data source={0}; Extended Properties=Excel 8.0;", pathToExcelFile);
                    }
                    else if (filename.EndsWith(".xlsx"))
                    {
                        connectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\";", pathToExcelFile);
                    }

                    var adapter = new OleDbDataAdapter("SELECT * FROM [Full time$]", connectionString);
                    var ds = new DataSet();

                    adapter.Fill(ds, "ExcelTable");

                    DataTable dtable = ds.Tables["ExcelTable"];

                    string sheetName = String.Empty;
                    switch (ImportType)
                    {
                        case 1: sheetName = "Full time"; break;
                        case 2: sheetName = "Adjunct"; break;
                        case 3: sheetName = "REG Schedule"; return RedirectToAction("AdminTasks", new { fileTypeSelectResult = "REG Schedule file type isn't realized" });
                        case 4: sheetName = "ADJ Schedule"; return RedirectToAction("AdminTasks", new { fileTypeSelectResult = "ADJ Schedule file type isn't realized" });
                        default:return RedirectToAction("AdminTasks", new { fileTypeSelectResult = "Select appropriate file type" });
                    }

                    var excelFile = new ExcelQueryFactory(pathToExcelFile);
                    var members = from a in excelFile.Worksheet<ExcelMembers>(sheetName) select a;

                    foreach (var cbuItem in members)
                    {
                        try
                        {
                            if (!String.IsNullOrEmpty(cbuItem.Name) || !String.IsNullOrEmpty(cbuItem.EmployeeID))
                            {
                                string lastName = String.Empty;
                                string firstName = String.Empty;
                                string middleName = String.Empty;
                                tb_MemberMaster FM = new tb_MemberMaster();
                                if (SplitFullName(cbuItem.Name, out lastName, out firstName, out middleName) == "Success")
                                {
                                    //CBU.Name
                                    FM.LastName =   lastName;
                                    FM.FirstName = firstName;
                                    FM.MiddleName = middleName.Replace(".", "");
                                    //CBU.EmployeeID
                                    FM.MemberIDNumber = cbuItem.EmployeeID;
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

                                int campusId = GetCampusID(GetCampusCode(cbuItem.Location));

                                //CBU.Location
                                FM.CampusID = campusId;
                                //CBU.Descr (1)
                                FM.AreaID = GetAreaID(GetAreaName(cbuItem.Descr));
                                //CBU.Descr (2)
                                FM.DepartmentID = GetDepartmentID(GetDepartmentName(cbuItem.Descr), campusId);
                                //DivisionID (Required field. Need to be filled)
                                FM.DivisionID = 108; //108 = 'Unknown' from tb_Division table
                                //CategoryID (Required field. Need to be filled)
                                FM.CategoryID = 4; //4 = 'Unknown' from tb_Categories table
                                //Check is Facility Member exist in DB
                                FM.MemberID = IsMemberExistInDB(FM.LastName, FM.FirstName, FM.MiddleName);

                                if (FM.MemberID == 0) // New Facility Member
                                {

                                    db.tb_MemberMaster.Add(FM);

                                    try
                                    {
                                        db.SaveChanges();
                                    }
                                    catch (DbEntityValidationException ex)
                                    {
                                        foreach (DbEntityValidationResult validationError in ex.EntityValidationErrors)
                                        {
                                            Response.Write("Object: " + validationError.Entry.Entity.ToString());
                                            Response.Write("");
                                            foreach (DbValidationError err in validationError.ValidationErrors)
                                            {
                                                Response.Write(err.ErrorMessage + "");
                                            }
                                        }
                                    }
                                }

                                AssignAddress(cbuItem.Address, cbuItem.City, cbuItem.St, cbuItem.Postal, FM.MemberID);
                                AssignPhoneNumber(cbuItem.Phone, FM.MemberID);
                            }
                            else
                            {
                                message = String.Empty;
                                //data.Add("<ul>");
                                if (cbuItem.Name == "" || cbuItem.Name == null)
                                    message = "Name is required; ";
                                if (cbuItem.EmployeeID == "" || cbuItem.EmployeeID == null)
                                    message = "EmployeeID is required; ";

                                return RedirectToAction("AdminTasks", new { xlsSelectResult = message });

                            }
                        }

                        catch (DbEntityValidationException ex)
                        {
                            foreach (var entityValidationErrors in ex.EntityValidationErrors)
                            {

                                foreach (var validationError in entityValidationErrors.ValidationErrors)
                                {

                                    Response.Write("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);

                                }

                            }
                        }
                    }
                    //deleting excel file from folder  
                    if ((System.IO.File.Exists(pathToExcelFile)))
                    {
                        System.IO.File.Delete(pathToExcelFile);
                    }
                    //return Json("success", JsonRequestBehavior.AllowGet);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ////alert message for invalid file format  
                    //data.Add("<ul>");
                    //data.Add("<li>Only Excel file format is allowed</li>");
                    //data.Add("</ul>");
                    //data.ToArray();
                    message = "Only Excel file format is allowed. ContentType is " + FileUpload.ContentType;
                    //return Json(data, JsonRequestBehavior.AllowGet);
                    return RedirectToAction("AdminTasks", new { xlsSelectResult = message });
                }
            }
            else
            {
                //if (FileUpload == null) data.Add("Please choose Excel file");
                //data.ToArray();
                //return Json(data, JsonRequestBehavior.AllowGet);
                if (FileUpload == null) message = "Please choose Excel file";
                return RedirectToAction("AdminTasks", new { xlsSelectResult = message });
            }
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