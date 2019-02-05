using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LRC_NET_Framework;
using PagedList;
using PagedList.Mvc;
using LRC_NET_Framework.Models;
using System.Data.Entity.Validation;
using System.IO;
using System.Text;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;

namespace LRC_NET_Framework.Controllers
{
    public class HomeController : Controller
    {
        private LRCEntities db = new LRCEntities();


        // GET: tb_MemberMaster
        [Authorize(Roles = "admin, organizer")]
        public ActionResult Index(string sortOrder, string searchString, int? page, int? CollegeID, int? DepartmentID, bool? IsActiveMembers, FormCollection formCollection)
        {
            if (!String.IsNullOrEmpty(searchString))
                searchString = searchString.Trim();

            // Check if it PostBack from Add Filter button
            if (Int32.TryParse(formCollection["Departments"], out int depId))
                DepartmentID = depId;
            if (Int32.TryParse(formCollection["Colleges"], out int colId))
                CollegeID = colId;
            if (Boolean.TryParse(formCollection["IsActiveMembers"], out bool IsActive))
                IsActiveMembers = IsActive;

            CollegeID = CollegeID ?? 0;
            DepartmentID = DepartmentID ?? 0;
            if (String.IsNullOrEmpty(searchString))
                searchString = " ";

            ViewBag._CollegeID = CollegeID;
            ViewBag._DepartmentID = DepartmentID;
            ViewBag._searchString = searchString;
            
            //Fill out DDLs (Filter)
            SelectListItem selListItem = new SelectListItem() { Value = "0", Text = " ALL DEPARTMENTS " };
            ViewBag.Departments = CommonFunctions.GetDepartments(CollegeID ?? 0, DepartmentID ?? 0, true, selListItem);

            var colleges = new SelectList(db.tb_College, "CollegeID", "CollegeName", CollegeID).ToList();
            colleges.Insert(0, new SelectListItem() { Value = "0", Text = " ALL COLLEGES " });
            ViewBag.Colleges = colleges;

            ViewBag.searchString = searchString;

            // Return filtered Facility Members 
            IQueryable<tb_MemberMaster> fms = db.tb_MemberMaster.Include(p => p.tb_Department).Include(p => p.tb_Department.tb_College);
            if (CollegeID != null && CollegeID > 0)
                fms = fms.Where(x => x.tb_Department.tb_College.CollegeID == CollegeID);
            if (DepartmentID != null && DepartmentID > 0)
                fms = fms.Where(x => x.DepartmentID == DepartmentID);
            if (!String.IsNullOrWhiteSpace(searchString))
                fms = fms.Where(x => x.FirstName.ToUpper().Contains(searchString.Trim().ToUpper()) || x.LastName.ToUpper().Contains(searchString.Trim().ToUpper()));

            List<tb_MemberMaster> members = fms.ToList();

            ViewData["MemberQty"] = members.Count();

            //Sorting
            var membersOrdered = members.OrderBy(s => s.LastName);
            switch (sortOrder)
            {
                case "Name desc":
                    membersOrdered = members.OrderByDescending(s => s.LastName);
                    break;
            }
            //Paging
            int pageNumber = (page ?? 1);
            return View(membersOrdered.ToPagedList(pageNumber, MvcApplication.PageSize_Home));

        }


        //// GET: Assessment/Filter
        //[Authorize(Roles = "admin, organizer")]
        //public ActionResult Filter()
        //{
        //    string searchString = (string)System.Web.HttpContext.Current.Profile.GetPropertyValue("SearchString");
        //    int  departmentID = (int)System.Web.HttpContext.Current.Profile.GetPropertyValue("DepartmentID");
        //    int collegeID = (int)System.Web.HttpContext.Current.Profile.GetPropertyValue("CollegeID");

        //    List<tb_Department> departments = db.tb_Department.ToList();
        //    SelectList Departments = new SelectList(db.tb_Department, "DepartmentID", "DepartmentName");
        //    SelectListItem selListItem = new SelectListItem() { Value = "0", Text = " + Filter by Department " };
        //    ViewBag.DepartmentID = CommonFunctions.AddFirstItem(Departments, selListItem);

        //    var colleges = new SelectList(db.tb_College, "CollegeID", "CollegeName", collegeID);
        //    ViewBag.Colleges = colleges;

        //    return PartialView("FilterDialogBox");
        //}

        // GET: AddFilter
        [Authorize(Roles = "admin, organizer")]
        public ActionResult AddFilter(int CollegeID, int DepartmentID)
        {
            List<tb_Department> departments = db.tb_Department.ToList();
            SelectList Departments = new SelectList(db.tb_Department/*.Where(x => x.CollegeID == CollegeID)*/, "DepartmentID", "DepartmentName");

            SelectListItem selListItem = new SelectListItem() { Value = "0", Text = " ALL DEPARTMENTS " };
            ViewBag.DepartmentID = CommonFunctions.AddFirstItem(Departments, selListItem);

            var colleges = new SelectList(db.tb_College, "CollegeID", "CollegeName", CollegeID);
            ViewBag.Colleges = colleges;

            return PartialView("AddFilter");
        }

        public JsonResult GetDepartmentsByCollegeId(int CollegeID)
        {
            string json = String.Empty;
            using (LRCEntities context = new LRCEntities())
            {
                SelectListItem selListItem = new SelectListItem() { Value = "0", Text = " ALL DEPARTMENTS " };
                SelectList Departments = CommonFunctions.GetDepartments(CollegeID, 0, true, selListItem);
                json = JsonConvert.SerializeObject(Departments, Formatting.Indented);
            }
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        // GET: MembersBySchool
        [Authorize(Roles = "admin, organizer")]
        public ActionResult MembersBySchool(string sortOrder, string searchString, int? page, int? CollegeID, int? DepartmentID, FormCollection formCollection)
        {
            if (!String.IsNullOrEmpty(searchString))
                searchString = searchString.Trim();

            // Check if it PostBack from Add Filter button
            if (Int32.TryParse(formCollection["Colleges"], out int colId))
                CollegeID = colId;
            if (Int32.TryParse(formCollection["Departments"], out int depId))
                DepartmentID = depId;

            //// User session properties. Set or get values 
            //if (DepartmentID == null)
            //    DepartmentID = (int)System.Web.HttpContext.Current.Profile.GetPropertyValue("DepartmentID");
            //else
            //    System.Web.HttpContext.Current.Profile.SetPropertyValue("DepartmentID", DepartmentID);

            //if (searchString == null)
            //    searchString = (string)System.Web.HttpContext.Current.Profile.GetPropertyValue("SearchString");
            //else
            //   System.Web.HttpContext.Current.Profile.SetPropertyValue("SearchString", searchString);

            //if (CollegeID == null)
            //    CollegeID = (int)System.Web.HttpContext.Current.Profile.GetPropertyValue("CollegeID");
            //else
            //    System.Web.HttpContext.Current.Profile.SetPropertyValue("CollegeID", CollegeID);

            CollegeID = CollegeID ?? 0;
            DepartmentID = DepartmentID ?? 0;
            if (String.IsNullOrEmpty(searchString))
                searchString = " ";

            ViewBag._CollegeID = CollegeID;
            ViewBag._DepartmentID = DepartmentID;
            ViewBag._searchString = searchString;

            //Fill out DDLs (Filter)
            SelectListItem selListItem = new SelectListItem() { Value = "0", Text = " ALL DEPARTMENTS " };
            ViewBag.Departments = CommonFunctions.GetDepartments(CollegeID ?? 0, DepartmentID ?? 0, true, selListItem);

            var colleges = new SelectList(db.tb_College, "CollegeID", "CollegeName", CollegeID);
            ViewBag.Colleges = colleges;

            // Return filtered Facility Members 
            IQueryable<tb_MemberMaster> fms = db.tb_MemberMaster.Include(p => p.tb_Department).Include(p => p.tb_Department.tb_College);
            if (CollegeID != null && CollegeID > 0)
                fms = fms.Where(x => x.tb_Department.tb_College.CollegeID == CollegeID);
            if (DepartmentID != null && DepartmentID > 0)
                fms = fms.Where(x => x.DepartmentID == DepartmentID);
            if (!String.IsNullOrWhiteSpace(searchString))
                fms = fms.Where(x => x.FirstName.ToUpper().Contains(searchString.ToUpper()) || x.LastName.ToUpper().Contains(searchString.ToUpper()));

            List < tb_MemberMaster> members = fms.ToList();

            ViewData["MemberQty"] = members.Count();

            //Sorting
            var membersOrdered = members.OrderBy(s => s.LastName);
            switch (sortOrder)
            {
                case "Name desc":
                    membersOrdered = members.OrderByDescending(s => s.LastName);
                    break;
            }           
            //Paging
            int pageNumber = (page ?? 1);
            return View(membersOrdered.ToPagedList(pageNumber, MvcApplication.PageSize_MembersBySchool));
        }

        // GET: ExportData
        [Authorize(Roles = "admin, organizer")]
        public ActionResult ExportData(int CollegeID, int DepartmentID, string searchString)
        {
            ViewBag.CollegeID = CollegeID;
            ViewBag.DepartmentID = DepartmentID;
            ViewBag.searchString = searchString;

            return PartialView("ExportData");
        }
        
        // GET: ExportToCsv
        [Authorize(Roles = "admin, organizer")]
        public ActionResult ExportToCsv(int CollegeID, int DepartmentID, string searchString)
        {
            //Get Current Response  
            var response = System.Web.HttpContext.Current.Response;
            response.BufferOutput = true;
            response.Clear();
            response.ClearHeaders();
            response.ContentEncoding = Encoding.Unicode;
            response.AddHeader("content-disposition", "attachment;filename=MembersBySchool.CSV ");
            response.ContentType = "text/plain";
            response.Write(CommonFunctions.GetExportString(CollegeID, DepartmentID, searchString));
            response.End();

            return View();
        }

        // GET: ExportToTxt
        [Authorize(Roles = "admin, organizer")]
        public ActionResult ExportToTxt(int CollegeID, int DepartmentID, string searchString)
        {
            //Get Current Response  
            var response = System.Web.HttpContext.Current.Response;
            response.BufferOutput = true;
            response.Clear();
            response.ClearHeaders();
            response.ContentEncoding = Encoding.Unicode;
            response.AddHeader("content-disposition", "attachment;filename=MembersBySchool.TXT ");
            response.ContentType = "text/plain";
            response.Write(CommonFunctions.GetExportString(CollegeID, DepartmentID, searchString));
            response.End();

            return View();
        }

        // GET: Details
        [Authorize(Roles = "admin, organizer")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tb_MemberMaster Worker = db.tb_MemberMaster.Find(id); //.Include(t => t.tb_Area).Include(t => t.tb_Department).Include(t => t.tb_Division).Include(t => t.tb_Dues).Include(t => t.tb_Gender).Include(t => t.tb_LatestUnionAssessment)
            if (Worker == null)
            {
                return HttpNotFound();
            }

            List<tb_AssessmentName> AssessmentNames = new List<tb_AssessmentName>();
            AssessmentNames = db.tb_AssessmentName.ToList();

            var model = new MemberDetailsModel()
            {
                _Member = Worker,
                _AssessmentName = AssessmentNames
            };

            var address = Worker.tb_MemberAddress.Where(t => t.IsPrimary == true).FirstOrDefault();
            var space = String.Empty;
            if (!String.IsNullOrEmpty(address.HomeStreet2))
                space = " ";
            ViewBag.MemberAddress = address.HomeStreet1 + space + address.HomeStreet2 + ", " + address.City + ", " + address.tb_States.StateCode + ", " + address.ZipCode;

            return View(model);
        }
        
        // GET: tb_MemberMaster/Edit/5
        [Authorize(Roles = "admin, organizer")]
        public ActionResult Edit(int? id, int? CollegeID)
        {
            ViewBag.CollegeID = CollegeID;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tb_MemberMaster tb_MemberMaster = db.tb_MemberMaster.Find(id);
            if (tb_MemberMaster == null)
            {
                return HttpNotFound();
            }

            string twitterHandle = String.Empty;
            string facebookID = String.Empty;
            var activities = tb_MemberMaster.tb_MemberActivity.Where(t => t.MemberID == id);
            if (activities.Count() > 0)
            {
                twitterHandle = activities.LastOrDefault().TwitterHandle;
                facebookID = activities.LastOrDefault().FacebookID;
            }

            MemberEditModel model = new MemberEditModel()
            {
                _MemberID = tb_MemberMaster.MemberID,
                _MemberFullName = tb_MemberMaster.FirstName + " " + tb_MemberMaster.LastName,
                _CollegeID = tb_MemberMaster.tb_Department.CollegeID,
                _Colleges = new SelectList(db.tb_College.OrderBy(s => s.CollegeName), "CollegeID", "CollegeName", tb_MemberMaster.tb_Department.CollegeID),
                _JobStatusID = tb_MemberMaster.JobStatusID,
                _JobStatuses = new SelectList(db.tb_JobStatus.OrderBy(s => s.JobStatusName), "JobStatusID", "JobStatusName", tb_MemberMaster.JobStatusID),
                _DivisionID = tb_MemberMaster.DivisionID,
                _Divisions = new SelectList(db.tb_Division.OrderBy(s => s.DivisionName), "DivisionID", "DivisionName", tb_MemberMaster.DivisionID),
                _DepartmentID = tb_MemberMaster.DepartmentID,
                _Departments = new SelectList(db.tb_Department.OrderBy(s => s.DepartmentName), "DepartmentID", "DepartmentName", tb_MemberMaster.DepartmentID),
                _CategoryID = tb_MemberMaster.CategoryID,
                _Categories = new SelectList(db.tb_Categories, "CategoryID", "CategoryName", tb_MemberMaster.CategoryID),
                _HireDate = tb_MemberMaster.HireDate?? DateTime.Now,
                _TwitterHandle = twitterHandle,
                _FaceBookID = facebookID
            };
            return View(model);
        }

        // POST: tb_MemberMaster/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin, organizer")]
        public ActionResult Edit(MemberEditModel model, int? CollegeID)
         {
            ViewBag.CollegeID = CollegeID;
            if (ModelState.IsValid)
            {
                tb_MemberMaster worker = db.tb_MemberMaster.Where(t => t.MemberID == model._MemberID).FirstOrDefault();
                worker.tb_Department.CollegeID = model._CollegeID;
                worker.JobStatusID = model._JobStatusID;
                worker.DivisionID = model._DivisionID;
                worker.DepartmentID = model._DepartmentID;
                worker.CategoryID = model._CategoryID;
                worker.HireDate = model._HireDate;
                var activities = worker.tb_MemberActivity.Any(t => t.MemberID == model._MemberID);
                if (activities)
                {
                    worker.tb_MemberActivity.Where(t => t.MemberID == model._MemberID).LastOrDefault().TwitterHandle = model._TwitterHandle ?? String.Empty;
                    worker.tb_MemberActivity.Where(t => t.MemberID == model._MemberID).LastOrDefault().FacebookID = model._FaceBookID ?? String.Empty;
                }

                //db.tb_MemberMaster.Attach(worker);
                var entry = db.Entry(worker);
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

                return RedirectToAction("Details", "Home", new { @id = model._MemberID });
            }
            return View(model);
        }

        // GET: Home/ManageContactInfo
        [Authorize(Roles = "admin, organizer")]
        public ActionResult ManageContactInfo(int? id, int? CollegeID)
        {
            //id = 1; // test REMOVE IT
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ViewBag.CollegeID = CollegeID;
            var FM = db.tb_MemberMaster.Find(id);
            MemberContactInfoModel model = new MemberContactInfoModel()
            {
                _MemberID = id ?? 0,
                _MemberName = FM.FirstName + " " + FM.LastName,
                //PHONE
                //_PhoneNumber = String.Empty,
                _IsPhonePrimary = true,
                _PhoneTypeID = 1,
                _PhoneTypes = new SelectList(db.tb_PhoneType, "PhoneTypeID", "PhoneTypeName"),
                _MemberPhoneNumbers = db.tb_MemberPhoneNumbers.Where(t => t.MemberID == id).OrderByDescending(s => s.StartDate).ThenByDescending(c => c.CreatedDateTime).ToList(),
                // ADDRESS >>check here
                _StateCode = db.tb_States.Where(r => r.StateID == db.tb_MemberAddress.Where(t => t.MemberID == id).FirstOrDefault().StateID).FirstOrDefault().StateCode,
                //_CreatedAdressBy = 2,
                _CreatedAdressDateTime = DateTime.Now,
                _IsAdressPrimary = true,
                _AddressTypeID = 1,
                _AddressTypes = new SelectList(db.tb_AddressType, "AddressTypeID", "AddressTypeName"),
                _SourceID = 1,
                _AddressSources = new SelectList(db.tb_AddressSource, "SourceID", "SourceName"),
                // >>check here
                _City = String.Empty,
                // >>check here
                _States = new SelectList(db.tb_States.ToList(), "StateID", "StateCode"),
                _MemberAddresses = db.tb_MemberAddress.Where(t => t.MemberID == id).OrderByDescending(s => s.StartDate).ThenByDescending(c => c.CreatedDateTime).ToList(),
                //EMAIL
                _EmailTypeID = 1,
                _IsEmailPrimary = true,
                _EmailTypes = new SelectList(db.tb_EmailType.ToList(), "EmailTypeID", "EmailTypeName"),
                _MemberEmails = db.tb_MemberEmail.Where(t => t.MemberID == id).OrderByDescending(s => s.StartDate).ThenByDescending(c => c.CreatedDateTime).ToList()
            };
            return View(model);
        }

        // POST: Home/ManageContactInfo
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        [Authorize(Roles = "admin, organizer")]
        public ActionResult ManageContactInfo(string submit, MemberContactInfoModel model, int? CollegeID)
        {
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            List<string> errs = new List<string>();
            ViewBag.CollegeID = CollegeID;
            var userId = HttpContext.GetOwinContext().Authentication.User.Identity.GetUserId();
            bool isValid = true;
            switch (submit)
            {
                case "Submit New Phone":
                    //Check 1st Html.BeginForm validation only. _PhoneNumber has attributes [Phone] and [Required] in the MemberContactInfoModel
                    foreach (var modelState in ModelState)
                    {
                        if (modelState.Key == "_PhoneNumber")
                        {
                            if (modelState.Value.Errors.Count > 0)
                                isValid = false;
                        }
                        else
                            modelState.Value.Errors.Clear();
                    }
                    if (isValid)
                        errs = CreateMemberModel.AssignPhoneNumber(model._PhoneNumber, model._PhoneTypeID, model._IsPhonePrimary, "Form", model._MemberID, userId);
                    break;

                case "Submit New Address":
                    //Check 2nd Html.BeginForm validation only. _HomeStreet1: [Required], _City: [Required], _ZipCode: [Required]
                    foreach (var modelState in ModelState)
                    {
                        if (modelState.Key == "_HomeStreet1" || modelState.Key == "_City" || modelState.Key == "_ZipCode")
                        {
                            if (modelState.Value.Errors.Count > 0)
                                isValid = false;
                        }
                        else
                            modelState.Value.Errors.Clear();
                    }
                    if (isValid)
                    {
                        string source = String.Empty;
                        //LZ requirement: Add "Current User" as once of the options and if selected, insert the session user name as the source.
                        if (model._SourceID == 3) //Current User (Fill out Source field in tb_MemberAddress)
                            source = userId;
                        errs = CreateMemberModel.AssignAddress(model._HomeStreet1, model._HomeStreet2, model._City, model._StateID.ToString(),
                            model._ZipCode, model._AddressTypeID, model._IsAdressPrimary, source, model._SourceID, model._MemberID, userId);
                    }
                    break;
                case "Submit New Email":
                    //Check 3rd Html.BeginForm validation only. _HomeStreet1: [Required], _City: [Required], _ZipCode: [Required]
                    foreach (var modelState in ModelState)
                    {
                        if (modelState.Key == "_EmailAddress")
                        {
                            if (modelState.Value.Errors.Count > 0)
                                isValid = false;
                        }
                        else
                            modelState.Value.Errors.Clear();
                    }
                    if (isValid)
                        errs = CreateMemberModel.AssignEmail(model._EmailAddress, model._EmailTypeID, model._IsEmailPrimary, "Form", model._MemberID, userId);
                    break;
            }
            var FM = db.tb_MemberMaster.Find(model._MemberID);
            model._MemberName = FM.FirstName + " " + FM.LastName;
            model._PhoneTypes = new SelectList(db.tb_PhoneType, "PhoneTypeID", "PhoneTypeName");
            model._MemberPhoneNumbers = db.tb_MemberPhoneNumbers.Where(t => t.MemberID == model._MemberID).OrderByDescending(s => s.StartDate).ThenByDescending(c => c.CreatedDateTime).ToList();
            // >> check here
            model._StateCode = db.tb_States.Where(p => p.StateID == db.tb_MemberAddress.Where(t => t.MemberID == model._MemberID).FirstOrDefault().StateID).FirstOrDefault().StateCode;
            model._AddressSources = new SelectList(db.tb_AddressSource, "SourceID", "SourceName");
            // >> check here
            model._States = new SelectList(db.tb_States.ToList(), "StateID", "StateName");
            model._AddressTypes = new SelectList(db.tb_AddressType, "AddressTypeID", "AddressTypeName");
            model._MemberAddresses = db.tb_MemberAddress.Where(t => t.MemberID == model._MemberID).OrderByDescending(s => s.StartDate).ThenByDescending(c => c.CreatedDateTime).ToList();
            model._EmailTypes = new SelectList(db.tb_EmailType.ToList(), "EmailTypeID", "EmailTypeName");
            model._MemberEmails = db.tb_MemberEmail.Where(t => t.MemberID == model._MemberID).OrderByDescending(s => s.StartDate).ThenByDescending(c => c.CreatedDateTime).ToList();

            FM.LastSeenDate = DateTime.UtcNow; 
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
            return View(model);
        }

        // GET: Notes
        [Authorize(Roles = "admin, organizer")]
        public ActionResult MembershipFormsList(string sortOrder, string searchString, int? page)
        {
            var MembershipForms = db.tb_MembershipForms.Include(t => t.tb_MemberMaster);
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "Date" : "";
            //Searching @ Filtering
            if (!String.IsNullOrEmpty(searchString))
            {
                MembershipForms = MembershipForms.Where(s => s.tb_MemberMaster.LastName.ToUpper().Contains(searchString.ToUpper())
                                       || s.tb_MemberMaster.FirstName.ToUpper().Contains(searchString.ToUpper()));
            }
            //Sorting
            switch (sortOrder)
            {
                case "Name desc":
                    break;
                case "Date":
                    MembershipForms = MembershipForms.OrderByDescending(s => s.Signed);
                    break;
                default:
                    MembershipForms = MembershipForms.OrderBy(s => s.Signed);
                    break;
            }

            //Paging
            int pageNumber = (page ?? 1);

            ViewData["MemberQty"] = MembershipForms.Count();


            return View(MembershipForms.ToPagedList(pageNumber, MvcApplication.PageSize_Default));
            //return View(MemberNotes.ToList());
        }

        // GET: Home/AddMembershipForm
        public ActionResult AddMembershipForm(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AddMembershipFormModel model = new AddMembershipFormModel()
            {
                _MemberID = id ?? 0,
                _Signed = DateTime.Now,
                _FormVersion = String.Empty,
                _FormImagePath = "No file chosen",
                _MembershipForms = db.tb_MembershipForms.Where(t => t.MemberID == id).OrderByDescending(t => t.MembershipFormID).ToList()
            };
            ViewBag._CollectedBy = new SelectList(db.AspNetUsers.OrderBy(s => s.LastFirstName), "Id", "LastFirstName");
            tb_MemberMaster fm = db.tb_MemberMaster.Find(id);
            ViewBag.MemberName = fm.FirstName + ", " + fm.LastName;

            return View(model);

        }

        // POST: Home/AddMembershipForm
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin, organizer")]
        public ActionResult AddMembershipForm(AddMembershipFormModel model, HttpPostedFileBase file)
        {
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            error.errMsg = ErrorDetail.GetMsg(error.errCode);
            List<string> errs = new List<string>();

            ViewBag._CollectedBy = new SelectList(db.AspNetUsers.OrderBy(s => s.LastFirstName), "Id", "LastFirstName");
            tb_MemberMaster fm = db.tb_MemberMaster.Find(model._MemberID);
            ViewBag.MemberName = fm.FirstName + " " + fm.LastName;
            model._MembershipForms = db.tb_MembershipForms.Where(t => t.MemberID == model._MemberID).OrderByDescending(t => t.MembershipFormID).ToList();

            if (file != null && file.ContentLength > 0)
                try
                {
                    var extension = file.FileName.Split('.').Last().ToUpper();
                    if (extension != "PDF" && extension != "JPG")
                    {
                        ViewBag.Message = "Selected file type is not PDF or JPEG";
                        return View(model);
                    }

                    string path = Path.Combine(Server.MapPath(MvcApplication.MembershipFormsFolder), file.FileName);
                    file.SaveAs(path);
                    ViewBag.Message = "File uploaded successfully";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                    return View(model);
                }
            else
            {
                ViewBag.Message = "You have not specified a file";
                return View(model);
            }

            var userId = HttpContext.GetOwinContext().Authentication.User.Identity.GetUserId();
            try
            {
                //Check dublicates by FileName and MemberID
                var mf = db.tb_MembershipForms.Where(s => s.FormImagePath.ToUpper() == file.FileName.ToUpper()
                && s.MemberID == model._MemberID).FirstOrDefault();
                if (mf == null) // Add new record
                {
                    tb_MembershipForms memberForm = new tb_MembershipForms()
                    {
                        MemberID = model._MemberID,
                        Signed = model._Signed,
                        FormVersion = model._FormVersion,
                        FormImagePath = file.FileName,
                        CollectedBy = model._CollectedBy,
                        AddedBy = userId
                    };
                    db.tb_MembershipForms.Add(memberForm);
                }
                else // Overwrite record with same PDF file name and MemberId to add new one later
                {
                    mf.Signed = model._Signed;
                    mf.FormVersion = model._FormVersion;
                    mf.FormImagePath = file.FileName;
                    mf.CollectedBy = model._CollectedBy;
                    mf.AddedBy = userId;
                    //db.tb_MembershipForms.Attach(mf);
                }

                db.SaveChanges();

                //Need to remove extra entries. Not more then MaxRecordsIn_MembershipFormsHistory
                model._MembershipForms = db.tb_MembershipForms.Where(t => t.MemberID == model._MemberID).OrderByDescending(t => t.MembershipFormID).ToList();
                int record = 0;
                foreach (var item in model._MembershipForms)
                {
                    if (++record > MvcApplication.MaxRecordsIn_MembershipFormsHistory)
                    {
                        db.tb_MembershipForms.Remove(item);
                    }
                }
                db.SaveChanges();
                model._MembershipForms = db.tb_MembershipForms.Where(t => t.MemberID == model._MemberID).OrderByDescending(t => t.MembershipFormID).ToList();
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
                errs.Add("Error #" + error.errCode.ToString() + "!" + error.errMsg);
                ViewData["ErrorList"] = errs;
                return View(model);
            }
            return View(model);
        }

        // GET: Home/AddCopeForm
        public ActionResult AddCopeForm(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AddCopeFormModel model = new AddCopeFormModel()
            {
                _MemberID = id ?? 0,
                _Signed = DateTime.Now,
                _MonthlyContribution = 0m,
                _FormImagePath = "No file chosen",
                _CopeForms = db.tb_CopeForms.Where(t => t.MemberID == id).OrderByDescending(t => t.CopeFormID).ToList()
            };
            ViewBag._CollectedBy = new SelectList(db.AspNetUsers.OrderBy(s => s.LastFirstName), "Id", "LastFirstName");
            tb_MemberMaster fm = db.tb_MemberMaster.Find(id);
            ViewBag.MemberName = fm.FirstName + " " + fm.LastName;

            return View(model);

        }

        // POST: Home/AddCopeForm
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin, organizer")]
        public ActionResult AddCopeForm(AddCopeFormModel model, HttpPostedFileBase file)
        {
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            error.errMsg = ErrorDetail.GetMsg(error.errCode);
            List<string> errs = new List<string>();

            ViewBag._CollectedBy = new SelectList(db.AspNetUsers.OrderBy(s => s.LastFirstName), "Id", "LastFirstName");
            tb_MemberMaster fm = db.tb_MemberMaster.Find(model._MemberID);
            ViewBag.MemberName = fm.FirstName + " " + fm.LastName;
            model._CopeForms = db.tb_CopeForms.Where(t => t.MemberID == model._MemberID).OrderByDescending(t => t.CopeFormID).ToList();

            if (file != null && file.ContentLength > 0)
                try
                {
                    var extension = file.FileName.Split('.').Last().ToUpper();
                    if (extension != "PDF" && extension != "JPG")
                    {
                        ViewBag.Message = "Selected file type is not PDF or JPEG";
                        return View(model);
                    }

                    string path = Path.Combine(Server.MapPath(MvcApplication.CopeFormsFolder), file.FileName);
                    file.SaveAs(path);
                    ViewBag.Message = "File uploaded successfully";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                    return View(model);
                }
            else
            {
                ViewBag.Message = "You have not specified a file";
                return View(model);
            }


            var userId = HttpContext.GetOwinContext().Authentication.User.Identity.GetUserId();
            try
            {
                //Check dublicates by FileName and MemberID
                var mf = db.tb_CopeForms.Where(s => s.FormImagePath.ToUpper() == file.FileName.ToUpper()
                && s.MemberID == model._MemberID).FirstOrDefault();
                if (mf == null) // Add new record
                {
                    tb_CopeForms copeForm = new tb_CopeForms()
                    {
                        MemberID = model._MemberID,
                        Signed = model._Signed,
                        MonthlyContribution = model._MonthlyContribution,
                        FormImagePath = file.FileName,
                        CollectedBy = model._CollectedBy,
                        AddedBy = userId
                    };
                    db.tb_CopeForms.Add(copeForm);
                }
                else // Overwrite record with same PDF file name and MemberId to add new one later
                {
                    mf.Signed = model._Signed;
                    mf.MonthlyContribution = model._MonthlyContribution;
                    mf.FormImagePath = file.FileName;
                    mf.CollectedBy = model._CollectedBy;
                    mf.AddedBy = userId;
                }

                db.SaveChanges();

                //Need to remove extra entries. Not more then MaxRecordsIn_MembershipFormsHistory
                model._CopeForms = db.tb_CopeForms.Where(t => t.MemberID == model._MemberID).OrderByDescending(t => t.CopeFormID).ToList();
                int record = 0;
                foreach (var item in model._CopeForms)
                {
                    if (++record > MvcApplication.MaxRecordsIn_CopeFormsHistory)
                    {
                        db.tb_CopeForms.Remove(item);
                    }
                }
                db.SaveChanges();
                model._CopeForms = db.tb_CopeForms.Where(t => t.MemberID == model._MemberID).OrderByDescending(t => t.CopeFormID).ToList();
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
                errs.Add("Error #" + error.errCode.ToString() + "!" + error.errMsg);
                ViewData["ErrorList"] = errs;
                return View(model);
            }

            return View(model);
        }

        public ActionResult GetPdfOrJpeg(string fileName, string form)
        {
            var extension = fileName.Split('.').Last().ToLower();
            var contentType = String.Empty;
            if (extension == "pdf")
                contentType = "application/pdf";
            else
                contentType = "image/jpeg";

            var formsFolder = String.Empty;
            switch (form)
            {
                case "MembershipForms":
                    formsFolder = MvcApplication.MembershipFormsFolder; break;
                case "CopeForms":
                    formsFolder = MvcApplication.CopeFormsFolder; break;
                case "Building":
                    formsFolder = MvcApplication.BuildingsFolder; break;
                default:
                    break;
            }

            var path = Server.MapPath(@formsFolder + fileName);
            var fileStream = new FileStream(path,
                                             FileMode.Open,
                                             FileAccess.Read
                                           );
            var fsResult = new FileStreamResult(fileStream, contentType);
            return fsResult;
        }
        
        // GET: Home/AlsoWorksAt/5
        [Authorize(Roles = "admin, organizer")]
        public ActionResult AlsoWorksAt(int? id)
        {
            //id = 1; // test REMOVE IT
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AlsoWorksAtModel model = new AlsoWorksAtModel()
            {
                _MemberID = id ?? 0,
                _EmployerID = 1,
                _Employers = new SelectList(db.tb_Employers, "EmployerID", "EmployerName"),
                _Note = String.Empty,
                _AddedDateTime = DateTime.Now,
                _AddedBy = 2,
                _AlsoWorksAts = db.tb_AlsoWorksAt.Where(t => t.MemberID == id).ToList()
            };
            return View(model);
        }

        // POST: Home/AlsoWorksAt
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AlsoWorksAt(AlsoWorksAtModel model)
        {
            var memberAlsoWorksAts = db.tb_AlsoWorksAt.Where(s => s.Note.ToUpper() == model._Note.ToUpper());
            //Check dublicates
            if (memberAlsoWorksAts.ToList().Count == 0)
            {
                tb_AlsoWorksAt memberAlsoWorks = new tb_AlsoWorksAt()
                {
                    MemberID = model._MemberID,
                    EmployerID = model._EmployerID,
                    Note = model._Note,
                    AddedDateTime = DateTime.Now,
                    AddedBy = 2
                };
                db.tb_AlsoWorksAt.Add(memberAlsoWorks);
            }
            else
            {
                tb_AlsoWorksAt memberAlsoWorks = memberAlsoWorksAts.FirstOrDefault();
                memberAlsoWorks.EmployerID = model._EmployerID;
                memberAlsoWorks.Note = model._Note;
                memberAlsoWorks.AddedDateTime = DateTime.Now;
                memberAlsoWorks.AddedBy = 2;
                db.tb_AlsoWorksAt.Attach(memberAlsoWorks);
            }
            db.SaveChanges();
            model._Employers = new SelectList(db.tb_Employers, "EmployerID", "EmployerName");
            model._AlsoWorksAts = db.tb_AlsoWorksAt.Where(t => t.MemberID == model._MemberID).ToList();
            return View(model);
        }

        // GET: Assessment/NotSure
        [Authorize(Roles = "admin, organizer")]
        public ActionResult NotSure()
        {
            return PartialView("NotSure");
        }

        // GET: Assessment/NotSure
        [Authorize(Roles = "admin, organizer")]
        public ActionResult NotSureMembershipForms()
        {
            return PartialView("NotSureMembershipForms");
        }
        
        // GET: Home/AddDepartment
        [Authorize(Roles = "admin, organizer")]
        public ActionResult AddDepartment(string sortOrder, string searchString, int? page, int? id)
        {
            ViewBag.MemberID = id ?? 1;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Paging
            int pageNumber = (page ?? 1);
            var _Departments = db.tb_Department.Include(t => t.tb_College);
            ViewData["MemberQty"] = _Departments.Count();
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "Name desc" : "";
            ViewBag.SearchString = searchString;

            //Searching
            if (!String.IsNullOrEmpty(searchString))
            {
                _Departments = _Departments.Where(s => s.DepartmentName.ToUpper().Contains(searchString.ToUpper()));
            }            
            
            //Sorting
            switch (sortOrder)
            {
                case "Name desc":
                    _Departments = _Departments.OrderByDescending(s => s.DepartmentName);
                    break;
                default:
                    _Departments = _Departments.OrderBy(s => s.DepartmentName);
                    break;
            }

            return View(_Departments.ToPagedList(pageNumber, MvcApplication.PageSize_Default));
        }

        // POST: Home/AddDepartment
        [HttpPost]
        [Authorize(Roles = "admin, organizer")]
        public ActionResult AddDepartment(string sortOrder, string searchString, int? page, string DepartmentName, int? CollegeID, int? id)
        {
            ViewBag.MemberID = id ?? 1;
            var _Departments = db.tb_Department.Where(s => s.DepartmentName.ToUpper() == DepartmentName.ToUpper());
            if (!String.IsNullOrEmpty(DepartmentName))
            {
                //Check dublicates
                if (_Departments.ToList().Count == 0)
                {
                    tb_Department newDepartment = new tb_Department() { DepartmentName = DepartmentName, CollegeID = CollegeID ?? 1 }; // I do not know yet which College will be appointed
                    db.tb_Department.Add(newDepartment);
                    db.SaveChanges();
                    ViewBag.Duplicate = String.Empty;
                }
                else
                {
                    ViewBag.Duplicate = DepartmentName + " department is already in the list";
                }
            }

            _Departments = db.tb_Department.Include(t => t.tb_College);
            //Paging
            int pageNumber = (page ?? 1);
            ViewData["MemberQty"] = _Departments.Count();
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "Name desc" : "";
            ViewBag.SearchString = searchString;

            //Searching
            if (!String.IsNullOrEmpty(searchString))
            {
                _Departments = _Departments.Where(s => s.DepartmentName.ToUpper().Contains(searchString.ToUpper()));
            }

            //Sorting
            switch (sortOrder)
            {
                case "Name desc":
                    _Departments = _Departments.OrderByDescending(s => s.DepartmentName);
                    break;
                default:
                    _Departments = _Departments.OrderBy(s => s.DepartmentName);
                    break;
            }

            return View(_Departments.ToPagedList(pageNumber, MvcApplication.PageSize_Default));
        }

        [HttpPost]
        [Authorize(Roles = "admin, organizer")]
        //AJAX Partitial View
        public ActionResult GetCampuses(int College)
        {
            var campuses = db.tb_Campus.Where(s => s.CollegeID == College).ToList();
            if (campuses.Count <= 0)
            {
                return HttpNotFound();
            }
            ViewBag.Campuses = new SelectList(campuses, "CampusID", "CampusName");
            ViewBag.CollegeID = College;
            return PartialView(campuses);
        }

        // GET: Home/AddMembershipForm
        public ActionResult CreateNewMember()
        {
            var colleges = db.tb_Campus.Where(t => t.IsMain == true).OrderBy(s => s.CollegeID);
            var collegeList = new SelectList(colleges, "CollegeID", "CampusName");
            var collegeID = colleges.ToArray().FirstOrDefault().CollegeID; //Initial value for Home School DDL
            ViewBag.Colleges = collegeList;
            ViewBag.CollegeID = collegeID;
            CreateMemberModel model = new CreateMemberModel()
            {
                _StateID = 0,
                _States = new SelectList(db.tb_States.ToList(), "StateID", "StateCode"),
                _AddressTypeID = 0,
                _AddressTypes = new SelectList(db.tb_AddressType, "AddressTypeID", "AddressTypeName"),
                _PhoneTypeID = 0,
                _PhoneTypes = new SelectList(db.tb_PhoneType.ToList(), "PhoneTypeID", "PhoneTypeName"),
                _EmailTypeID = 0,
                _EmailTypes = new SelectList(db.tb_EmailType.ToList(), "EmailTypeID", "EmailTypeName"),
                _CategoryID = 0,
                _Categories = new SelectList(db.tb_Categories.ToList(), "CategoryID", "CategoryName"),
                _JobStatusID = 0,
                _JobStatuses = new SelectList(db.tb_JobStatus.ToList(), "JobStatusID", "JobStatusName"),
                _DepartmentID = 0,
                _Departments = new SelectList(db.tb_Department.Where(t => t.CollegeID == collegeID).ToList(), "DepartmentID", "DepartmentName"),
                //_CampusID = 0,
                //_Campuses = new SelectList(db.tb_Campus.Where(t => t.IsMain == true).ToList(), "CampusID", "CollegeCode")
            };

            List<string> errs = new List<string>();
            if (TempData["ErrorList"] == null)
            {
                errs.Add("Empty");
            }
            else
                errs = TempData["ErrorList"] as List<string>;

            ViewData["ErrorList"] = errs;

            return View(model);
        }

        // POST: Home/AddMembershipForm
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin, organizer")]
        public ActionResult CreateNewMember(CreateMemberModel model, int? College, int? Department, FormCollection formCollection)
        {

            Error error = new Error();
            List<string> errs = new List<string>();
            int newMemberID = 0;

            var colleges = db.tb_Campus.Where(t => t.IsMain == true);
            var collegeList = new SelectList(colleges, "CollegeID", "CampusName");
            ViewBag.Colleges = collegeList;
            ViewBag.CollegeID = College;
            model._States = new SelectList(db.tb_States.ToList(), "StateID", "StateCode");
            model._AddressTypes = new SelectList(db.tb_AddressType, "AddressTypeID", "AddressTypeName");
            model._PhoneTypes = new SelectList(db.tb_PhoneType.ToList(), "PhoneTypeID", "PhoneTypeName");
            model._EmailTypes = new SelectList(db.tb_EmailType.ToList(), "EmailTypeID", "EmailTypeName");
            model._Categories = new SelectList(db.tb_Categories.ToList(), "CategoryID", "CategoryName");
            model._JobStatuses = new SelectList(db.tb_JobStatus.ToList(), "JobStatusID", "JobStatusName");
            //model._CampusID = College;
            //model._Campuses = new SelectList(colleges.ToList(), "CampusID", "CollegeCode");
            //model._DepartmentID = Department;
            model._Departments = new SelectList(db.tb_Department.Where(t => t.CollegeID == College).ToList(), "DepartmentID", "DepartmentName");

            if (model._DepartmentID == null && Department == null)
            {
                error.errCode = ErrorDetail.Failed;
                error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!DepartmentID = NULL (Department is not selected)";
                errs.Add(error.errMsg);
                ViewData["ErrorList"] = errs;
                return View(model);
            }
            if (model._DepartmentID == null)
                model._DepartmentID = Department;

            var userId = HttpContext.GetOwinContext().Authentication.User.Identity.GetUserId();

            using (LRCEntities context = new LRCEntities())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        // Checking unique EmployeeID (tb_MemberMaster.MemberIDNumber) if not exist in tb_MemberMaster table
                        var tb_MemberMasters = context.tb_MemberMaster.Where(s => s.MemberIDNumber.ToUpper() == model._MemberIDNumber.ToUpper()).ToArray();
                        if (tb_MemberMasters.Count() > 0) //Address found in the DB
                        {
                            var fm = tb_MemberMasters.FirstOrDefault();
                            error.errCode = ErrorDetail.Failed;
                            error.errMsg = ErrorDetail.GetMsg(error.errCode) + "! EmployeeID = " + fm.MemberIDNumber + " (MemberIDNumber in tb_MemberMaster table) already exist for Member: " +
                                fm.FirstName + " " + fm.MiddleName + " " + fm.LastName;
                            errs.Add(error.errMsg);
                            ViewData["ErrorList"] = errs;
                            return View(model);
                        }

                        // Checking Member Name if not exist in tb_MemberMaster table
                        tb_MemberMasters = context.tb_MemberMaster.Where(s => s.LastName.ToUpper() == model._LastName.ToUpper() &&
                            s.FirstName.ToUpper() == model._FirstName.ToUpper() &&
                            s.MiddleName.ToUpper() == model._MiddleName.ToUpper()).ToArray();
                        if (tb_MemberMasters.Count() > 0) //Address found in the DB
                        {
                            var fm = tb_MemberMasters.FirstOrDefault();
                            error.errCode = ErrorDetail.Failed;
                            error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Member with name " + fm.FirstName + " " + fm.MiddleName + " " + fm.LastName + " already exist in the tb_MemberMaster table";
                            errs.Add(error.errMsg);
                            ViewData["ErrorList"] = errs;
                            return View(model);
                        }

                        //Check if Area (from Role input) exist in tb_Area
                        //Optional field. Check if it entered firstly
                        int areaID = -1;
                        if (!String.IsNullOrEmpty(model._Area))
                        {
                            errs = CreateMemberModel.GetAreaID(model._Area, out areaID);
                            if (errs.Count() > 0)
                            {
                                ViewData["ErrorList"] = errs;
                                return View(model);
                            }
                        }

                        // Fill out tb_MemberMaster fields from model
                        tb_MemberMaster FM = new tb_MemberMaster
                        {
                            MemberIDNumber = model._MemberIDNumber,
                            FirstName = model._FirstName,
                            LastName = model._LastName,
                            MiddleName = model._MiddleName,
                            CategoryID = model._CategoryID,
                            JobStatusID = model._JobStatusID,
                            DepartmentID = model._DepartmentID, //from AJAX particial view
                            CampusID = College, //selecting MAIN campuses from tb_Campus only - its a College Name (from AJAX particial view)
                            LastSeenDate = DateTime.UtcNow,
                            AddedBy = userId,
                            AddedDateTime = DateTime.UtcNow
                        };
                        if (areaID > 0)
                            FM.AreaID = areaID;

                        context.tb_MemberMaster.Add(FM);
                        try
                        {
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
                            errs.Add("Error #" + error.errCode.ToString() + "!" + error.errMsg);
                            ViewData["ErrorList"] = errs;
                            return View(model);
                        }
                        newMemberID = FM.MemberID;
                        errs = CreateMemberModel.AssignAddress(model._HomeStreet1, model._HomeStreet2, model._City, model._StateID.ToString(), 
                            model._ZipCode, model._AddressTypeID, true, "Form", 1, FM.MemberID, userId);
                        if (errs.Count > 0)
                        {
                            ViewData["ErrorList"] = errs;
                            transaction.Rollback();
                            return View(model);
                        }

                        errs = CreateMemberModel.AssignPhoneNumber(model._PhoneNumber, model._PhoneTypeID, true, "Form", FM.MemberID, userId);
                        if (errs.Count > 0)
                        {
                            ViewData["ErrorList"] = errs;
                            transaction.Rollback();
                            return View(model);
                        }

                        errs = CreateMemberModel.AssignEmail(model._EmailAddress, model._EmailTypeID, true, "Form", FM.MemberID, userId);
                        if (errs.Count > 0)
                        {
                            ViewData["ErrorList"] = errs;
                            transaction.Rollback();
                            return View(model);
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                    }
                }
            }
            errs.Add("Empty");
            ViewData["ErrorList"] = errs;
            return RedirectToAction("Details", "Home", new { @id = newMemberID });
            //return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "admin, organizer")]
        //AJAX Partitial View
        public ActionResult GetDepartments(int College, FormCollection formCollection)
        {
            var tb_Departments = db.tb_Department.Where(s => s.CollegeID == College).ToList();
            if (tb_Departments.Count <= 0)
            {
                return HttpNotFound();
            }
            ViewBag.Departments = new SelectList(tb_Departments, "DepartmentID", "DepartmentName");
            ViewBag.CollegeID = College;
            return PartialView(tb_Departments);
        }

        // GET: DeletePhone/5
        [Authorize(Roles = "admin, organizer")]
        public ActionResult DeletePhone(int? PhoneRecID, int? MemberId, int? CollegeID)
        {
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            if (PhoneRecID == null )
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (LRCEntities context = new LRCEntities())
            {
                try
                {
                    tb_MemberPhoneNumbers phone = context.tb_MemberPhoneNumbers.Find(PhoneRecID);
                    if (phone == null)
                    {
                        return HttpNotFound();
                    }
                    context.tb_MemberPhoneNumbers.Remove(phone);
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
            return RedirectToAction("ManageContactInfo", new { @id = MemberId, CollegeID = CollegeID });
        }

        // GET: DeleteAddress/5
        [Authorize(Roles = "admin, organizer")]
        public ActionResult DeleteAddress(int? MemberAddressID, int? MemberId, int? CollegeID)
        {
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            if (MemberAddressID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (LRCEntities context = new LRCEntities())
            {
                try
                {
                    tb_MemberAddress address = context.tb_MemberAddress.Find(MemberAddressID);
                    if (address == null)
                    {
                        return HttpNotFound();
                    }
                    context.tb_MemberAddress.Remove(address);
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
            return RedirectToAction("ManageContactInfo", new { @id = MemberId, CollegeID = CollegeID });
        }

        // GET: DeleteAddress/5
        [Authorize(Roles = "admin, organizer")]
        public ActionResult DeleteEmail(int? EmailID, int? MemberId, int? CollegeID)
        {
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            if (EmailID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (LRCEntities context = new LRCEntities())
            {
                    var buildings = context.tb_Building.OrderBy(b=>b.BuildingName).ToList();

                return View(buildings);
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "This application manages current and historical data regarding Local 2279 union members working for the Los Rios Community College District (hereafter referred to as the District) colleges and campuses. The application supports administrative efforts to track and support membership and organizer efforts to support and increase participation of members.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Los Rios College Federation of Teachers";

            return View();
        }


    }
}