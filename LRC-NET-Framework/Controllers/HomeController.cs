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


namespace LRC_NET_Framework.Controllers
{
    public class HomeController : Controller
    {
        private LRCEntities db = new LRCEntities();

        private string GetExportString(string searchString, int CollegeID)
        {

            List<tb_Department> deps = new List<tb_Department>();
            deps = db.tb_Department.Where(c => c.CollegeID == CollegeID).ToList();
            List<tb_MemberMaster> members = new List<tb_MemberMaster>();


            foreach (var dep in deps)
            {
                List<tb_MemberMaster> membersInDep = db.tb_MemberMaster.Where(t => t.DepartmentID == dep.DepartmentID).ToList();
                if (membersInDep.Count > 0)
                {
                    foreach (var item in membersInDep)
                    {
                        if (String.IsNullOrEmpty(searchString))
                        {
                            members.Add(item);
                        }
                        //Searching @ Filtering
                        else if (item.LastName.ToUpper().Contains(searchString.ToUpper())
                            || item.FirstName.ToUpper().Contains(searchString.ToUpper()))
                        {
                            members.Add(item);
                        }
                    }
                }
            }

            //if (members == null)
            //{
            //    return HttpNotFound();
            //}

            var sb = new StringBuilder();
            sb.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18}",
                "ID",
                "INSTRCTR",
                "CAMPUS",
                "LOCATION",
                "BUILDING",
                "ROOM",
                "DIV",
                "CLASS#",
                "SECT",
                "SUBJCD",
                "CATBR",
                "LEC LAB",
                "SBTM",
                "ATT TP",
                "BEGTIME",
                "ENDTIME",
                "DAYS",
                "CLASSEND DT",
                Environment.NewLine);

            foreach (var item in members)
            {
                List<tb_SemesterTaught> semesterTaught = db.tb_SemesterTaught.Where(t => t.MemberID == item.MemberID).ToList();
                foreach (var taught in semesterTaught)
                {
                    sb.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18}",
                    item.MemberIDNumber ?? String.Empty, //ID
                    item.FirstName + " " + item.LastName + " " + item.MiddleName, //INSTRCTR
                    taught.tb_Building.tb_Campus.CollegeCode ?? String.Empty, //CAMPUS
                    taught.tb_Building.tb_Campus.CollegeCode + " MAIN", //LOCATION
                    taught.tb_Building.BuildingName ?? String.Empty, //BUILDING
                    taught.Room ?? String.Empty, //ROOM
                    item.tb_Division.DivisionName ?? String.Empty, //DIV
                    taught.Class ?? String.Empty, //CLASS#
                    "?", //SECT
                    "?", //SUBJCD
                    "?", //CATBR
                    "?", //LEC LAB
                    "?", //SBTM
                    "?", //ATT TP
                    taught.ClassStart.ToString(@"hh\:mm"), //BEGTIME
                    taught.ClassEnd.ToString(@"hh\:mm"), //ENDTIME
                    taught.tb_WeekDay.WeekDayName, //DAYS
                    "?", //CLASSEND DT
                    Environment.NewLine);
                }
            }
            return sb.ToString();
        }

        // GET: tb_MemberMaster
        [Authorize(Roles = "admin, organizer")]
        public ActionResult Index(string sortOrder, string searchString, int? page, int? CollegeID, int? DepartmentID)
        {
            //Error error = new Error();
            //ExcelImport.Models.ExcelMembers excelMembers= new ExcelImport.Models.ExcelMembers();
            //error = excelMembers.SetPhonePrimaryFalse(1);

            //var uName = HttpContext.GetOwinContext().Authentication.User.Identity.GetUserId();

            var tb_MemberMasters = db.tb_MemberMaster.Include(t => t.tb_Area).Include(t => t.tb_Department).Include(t => t.tb_Dues).Include(t => t.tb_LatestUnionAssessment).Include(t => t.tb_Dues);

            tb_MemberMasters.Select(t => t.tb_Department.tb_College);
            ViewData["MemberQty"] = tb_MemberMasters.Count();

            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "Name desc" : "";

            //Searching @ Filtering
            if (!String.IsNullOrEmpty(searchString))
            {
                tb_MemberMasters = tb_MemberMasters.Where(s => s.LastName.ToUpper().Contains(searchString.ToUpper())
                                       || s.FirstName.ToUpper().Contains(searchString.ToUpper()));
            }
            if (CollegeID != null && CollegeID != 0)
            {
                tb_MemberMasters = tb_MemberMasters.Where(f => f.tb_Department.CollegeID == CollegeID);
            }
            if (DepartmentID != null && DepartmentID != 0)
            {
                tb_MemberMasters = tb_MemberMasters.Where(f => f.DepartmentID == DepartmentID);
            }
            List<tb_College> colleges = db.tb_College.ToList();
            ViewBag.Colleges = new SelectList(colleges, "CollegeID", "CollegeName");
            List<tb_Department> departments = db.tb_Department.ToList();
            tb_MemberMaster tb_MemberMaster = db.tb_MemberMaster.Find(1);
            SelectList Departments = new SelectList(db.tb_Department, "DepartmentID", "DepartmentName", tb_MemberMaster.DepartmentID);
            SelectListItem selListItem = new SelectListItem() { Value = "0", Text = " + Filter by Department " };
            ViewBag.DepartmentID = CommonFunctions.AddFirstItem(Departments, selListItem);
            SelectList Colleges = new SelectList(db.tb_College, "CollegeID", "CollegeName", tb_MemberMaster.tb_Department.CollegeID);
            selListItem = new SelectListItem() { Value = "0", Text = " + Filter by College " };
            ViewBag.CollegeID = CommonFunctions.AddFirstItem(Colleges, selListItem);

            //Sorting
            switch (sortOrder)
            {
                case "Name desc":
                    tb_MemberMasters = tb_MemberMasters.OrderByDescending(s => s.LastName);
                    break;
                //case "Date":
                //    tb_MemberMasters = tb_MemberMasters.OrderBy(s => s.HireDate);
                //    break;
                //case "Date desc":
                //    tb_MemberMasters = tb_MemberMasters.OrderByDescending(s => s.HireDate);
                //    break;
                default:
                    tb_MemberMasters = tb_MemberMasters.OrderBy(s => s.LastName);
                    break;
            }

            //Paging
            int pageSize = 20;
            int pageNumber = (page ?? 1);

            return View(tb_MemberMasters.ToPagedList(pageNumber, pageSize));
        }

        // GET: Details
        [Authorize(Roles = "admin, organizer")]
        public ActionResult Details(int? id)
        {
            //id = 1; // test REMOVE IT
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tb_MemberMaster Worker = db.tb_MemberMaster.Find(id); //.Include(t => t.tb_Area).Include(t => t.tb_Department).Include(t => t.tb_Division).Include(t => t.tb_Dues).Include(t => t.tb_Gender).Include(t => t.tb_LatestUnionAssessment)
            if (Worker == null)
            {
                return HttpNotFound();
            }
            tb_MemberAddress ma = db.tb_MemberAddress.Where(t => t.MemberID == id).Where(t => t.IsPrimary == true).FirstOrDefault();
            if (ma != null)
            {
                var space = String.Empty;
                if (!String.IsNullOrEmpty(ma.HomeStreet2))
                    space = " ";
                ViewBag.MemberAddress = ma.HomeStreet1 + space + ma.HomeStreet2 + ", " + ma.City + ", " + ma.tb_States.StateCode + ", " + ma.ZipCode;
            }
            else
                ViewBag.MemberAddress = "Primary Address is not present";

            //tb_AssessmentName assessmentName = new tb_AssessmentName();
            //assessmentName = db.tb_AssessmentName;
            List<tb_AssessmentName> AssessmentNames = new List<tb_AssessmentName>();
            AssessmentNames = db.tb_AssessmentName.ToList();

            var model = new MemberDetailsModel()
            {
                _Member = Worker,
                _AssessmentName = AssessmentNames
            };
            return View(model);
        }

        // GET: MembersBySchool
        [Authorize(Roles = "admin, organizer")]
        public ActionResult MembersBySchool(string sortOrder, string searchString, int? page, int? CollegeID, int? DepartmentID, FormCollection formCollection)
        {
            CollegeID = CollegeID ?? int.Parse(formCollection["CollegeID"]);
            if (CollegeID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.CollegeID = CollegeID;
            ViewBag.Search = searchString;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "Name desc" : "";

            List<tb_Department> deps = new List<tb_Department>();
            deps = db.tb_Department.Where(c => c.CollegeID == CollegeID).ToList();
            List<tb_MemberMaster> members = new List<tb_MemberMaster>();


            foreach (var dep in deps)
            {
                List<tb_MemberMaster> membersInDep = db.tb_MemberMaster.Where(t => t.DepartmentID == dep.DepartmentID).ToList();
                if (membersInDep.Count > 0)
                {
                    foreach (var item in membersInDep)
                    {
                        if (String.IsNullOrEmpty(searchString))
                        {
                            members.Add(item);
                        }
                        //Searching @ Filtering
                        else if (item.LastName.ToUpper().Contains(searchString.ToUpper())
                            || item.FirstName.ToUpper().Contains(searchString.ToUpper()))
                        {
                            members.Add(item);
                        }
                    }
                }
            }

            if (members == null)
            {
                return HttpNotFound();
            }

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
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(membersOrdered.ToPagedList(pageNumber, pageSize));
        }

        // GET: ExportData
        [Authorize(Roles = "admin, organizer")]
        public ActionResult ExportData(int CollegeID)
        {
            ViewBag.CollegeID = CollegeID;
            return PartialView("ExportData");
        }
        
        // GET: ExportToCsv
        [Authorize(Roles = "admin, organizer")]
        public ActionResult ExportToCsv(string searchString, int CollegeID)
        {
            //Get Current Response  
            var response = System.Web.HttpContext.Current.Response;
            response.BufferOutput = true;
            response.Clear();
            response.ClearHeaders();
            response.ContentEncoding = Encoding.Unicode;
            response.AddHeader("content-disposition", "attachment;filename=MembersBySchool.CSV ");
            response.ContentType = "text/plain";
            response.Write(GetExportString(searchString, CollegeID));
            response.End();

            return View();
        }


        // GET: ExportToTxt
        [Authorize(Roles = "admin, organizer")]
        public ActionResult ExportToTxt(string searchString, int CollegeID)
        {
            //Get Current Response  
            var response = System.Web.HttpContext.Current.Response;
            response.BufferOutput = true;
            response.Clear();
            response.ClearHeaders();
            response.ContentEncoding = Encoding.Unicode;
            response.AddHeader("content-disposition", "attachment;filename=MembersBySchool.TXT ");
            response.ContentType = "text/plain";
            response.Write(GetExportString(searchString, CollegeID));
            response.End();

            //File(new UTF8Encoding().GetBytes(sb.ToString()), "text/csv", "MembersBySchool.txt");

            return View();
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
                _MemberFullName = tb_MemberMaster.LastName + ", " + tb_MemberMaster.FirstName,
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
                worker.tb_MemberActivity.Where(t => t.MemberID == model._MemberID).LastOrDefault().TwitterHandle = model._TwitterHandle;
                worker.tb_MemberActivity.Where(t => t.MemberID == model._MemberID).LastOrDefault().FacebookID = model._FaceBookID;

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
                _MemberName = FM.LastName + ", " + FM.FirstName,
                //PHONE
                //_PhoneNumber = String.Empty,
                _IsPhonePrimary = true,
                _PhoneTypeID = 1,
                _PhoneTypes = new SelectList(db.tb_PhoneType, "PhoneTypeID", "PhoneTypeName"),
                _MemberPhoneNumbers = db.tb_MemberPhoneNumbers.Where(t => t.MemberID == id).ToList(),
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
                _MemberAddresses = db.tb_MemberAddress.Where(t => t.MemberID == id).ToList(),
                //EMAIL
                _EmailTypeID = 1,
                _IsEmailPrimary = true,
                _EmailTypes = new SelectList(db.tb_EmailType.ToList(), "EmailTypeID", "EmailTypeName"),
                _MemberEmails = db.tb_MemberEmail.Where(t => t.MemberID == id).ToList()
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
            var uName = HttpContext.GetOwinContext().Authentication.User.Identity.GetUserId();
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
                        errs = CreateMemberModel.AssignPhoneNumber(model._PhoneNumber, model._PhoneTypeID, model._IsPhonePrimary, "Form", model._MemberID, uName);
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
                        errs = CreateMemberModel.AssignAddress(model._HomeStreet1, model._HomeStreet2, model._City, model._StateID.ToString(), 
                            model._ZipCode, model._AddressTypeID, model._IsAdressPrimary, "Form", model._SourceID, model._MemberID, uName);
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
                        errs = CreateMemberModel.AssignEmail(model._EmailAddress, model._EmailTypeID, model._IsEmailPrimary, "Form", model._MemberID, uName);
                    break;
            }
            var FM = db.tb_MemberMaster.Find(model._MemberID);
            model._MemberName = FM.LastName + ", " + FM.FirstName;
            model._PhoneTypes = new SelectList(db.tb_PhoneType, "PhoneTypeID", "PhoneTypeName");
            model._MemberPhoneNumbers = db.tb_MemberPhoneNumbers.Where(t => t.MemberID == model._MemberID).ToList();
            // >> check here
            model._StateCode = db.tb_States.Where(p => p.StateID == db.tb_MemberAddress.Where(t => t.MemberID == model._MemberID).FirstOrDefault().StateID).FirstOrDefault().StateCode;
            model._AddressSources = new SelectList(db.tb_AddressSource, "SourceID", "SourceName");
            // >> check here
            model._States = new SelectList(db.tb_States.ToList(), "StateID", "StateName");
            model._AddressTypes = new SelectList(db.tb_AddressType, "AddressTypeID", "AddressTypeName");
            model._MemberAddresses = db.tb_MemberAddress.Where(t => t.MemberID == model._MemberID).ToList();
            model._EmailTypes = new SelectList(db.tb_EmailType.ToList(), "EmailTypeID", "EmailTypeName");
            model._MemberEmails = db.tb_MemberEmail.Where(t => t.MemberID == model._MemberID).ToList();

            return View(model);
        }

        // GET: Home/AddMembershipForm
        public ActionResult AddMembershipForm(int? id)
        {
            //id = 1; // test REMOVE IT
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
                _CollectedBy = 2,
                _MembershipForms = db.tb_MembershipForms.Where(t => t.MemberID == id).ToList()
            };
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
            string imagesFolder = "~/Images/MembershipForms/";
            if (file != null && file.ContentLength > 0)
                try
                {
                    string path = Path.Combine(Server.MapPath(imagesFolder),
                                               Path.GetFileName(file.FileName));
                    file.SaveAs(path);
                    ViewBag.Message = "File uploaded successfully";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                }
            else
            {
                ViewBag.Message = "You have not specified a file.";
                model._MembershipForms = db.tb_MembershipForms.Where(t => t.MemberID == model._MemberID).ToList();
                return View(model);
            }

            var memberForms = db.tb_MembershipForms.Where(s => s.FormImagePath.ToUpper() == imagesFolder.ToUpper());
            //Check dublicates
            if (memberForms.ToList().Count == 0)
            {
                tb_MembershipForms memberForm = new tb_MembershipForms()
                {
                    MemberID = model._MemberID,
                    Signed = model._Signed,
                    FormVersion = model._FormVersion,
                    FormImagePath = imagesFolder + Path.GetFileName(file.FileName),
                    CollectedBy = model._CollectedBy
                };
                db.tb_MembershipForms.Add(memberForm);
            }
            else
            {
                tb_MembershipForms memberForm = memberForms.FirstOrDefault();
                memberForm.Signed = model._Signed;
                memberForm.FormVersion = model._FormVersion;
                memberForm.FormImagePath = imagesFolder + Path.GetFileName(file.FileName);
                memberForm.CollectedBy = 2;
                db.tb_MembershipForms.Attach(memberForm);
            }
            db.SaveChanges();
            model._MembershipForms = db.tb_MembershipForms.Where(t => t.MemberID == model._MemberID).ToList();
            return View(model);
        }

        // GET: Home/AddCopeForm
        public ActionResult AddCopeForm(int? id)
        {
            //id = 1; // test REMOVE IT
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
                _CollectedBy = 2,
                _CopeForms = db.tb_CopeForms.Where(t => t.MemberID == id).ToList()
            };
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
            string imagesFolder = "~/Images/CopeForms/";
            if (file != null && file.ContentLength > 0)
                try
                {
                    string path = Path.Combine(Server.MapPath(imagesFolder),
                                               Path.GetFileName(file.FileName));
                    file.SaveAs(path);
                    ViewBag.Message = "File uploaded successfully";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                }
            else
            {
                ViewBag.Message = "You have not specified a file.";
                model._CopeForms = db.tb_CopeForms.Where(t => t.MemberID == model._MemberID).ToList();
                return View(model);
            }

            var copeForms = db.tb_CopeForms.Where(s => s.FormImagePath.ToUpper() == imagesFolder.ToUpper());
            //Check dublicates
            if (copeForms.ToList().Count == 0)
            {
                tb_CopeForms copeForm = new tb_CopeForms()
                {
                    MemberID = model._MemberID,
                    Signed = model._Signed,
                    MonthlyContribution = model._MonthlyContribution,
                    FormImagePath = imagesFolder + Path.GetFileName(file.FileName),
                    CollectedBy = model._CollectedBy
                };
                db.tb_CopeForms.Add(copeForm);
            }
            else
            {
                tb_CopeForms copeForm = copeForms.FirstOrDefault();
                copeForm.Signed = model._Signed;
                copeForm.MonthlyContribution = model._MonthlyContribution;
                copeForm.FormImagePath = imagesFolder + Path.GetFileName(file.FileName);
                copeForm.CollectedBy = 2;
                db.tb_CopeForms.Attach(copeForm);
            }
            db.SaveChanges();
            model._CopeForms = db.tb_CopeForms.Where(t => t.MemberID == model._MemberID).ToList();
            return View(model);
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
            int pageSize = 20;
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

            return View(_Departments.ToPagedList(pageNumber, pageSize));
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
            int pageSize = 20;
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

            return View(_Departments.ToPagedList(pageNumber, pageSize));
        }

        // GET: Home/AddBuilding
        public ActionResult AddBuilding()
        {
            var colleges = new SelectList(db.tb_College, "CollegeID", "CollegeName");
            ViewBag.Colleges = colleges;
            ViewBag.CollegeID = 0;
            return View(db.tb_College.ToList());
        }

        // POST: Home/AddBuilding
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin, organizer")]
        public ActionResult AddBuilding(HttpPostedFileBase file, int Campus, int College, string BuildingName, FormCollection formCollection)
        {
            //FormCollection formCollection
            string imagesFolder = "~/Images/Buildings/";
            if (file != null && file.ContentLength > 0)
                try
                {
                    string path = Path.Combine(Server.MapPath(imagesFolder),
                    Path.GetFileName(file.FileName));
                    file.SaveAs(path);
                    ViewBag.Message = "File uploaded successfully";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                }
            else
            {
                ViewBag.Message = "You have not specified a file.";
                ViewBag.Colleges = new SelectList(db.tb_College, "CollegeID", "CollegeName", College);
                return View();
            }

            var buildings = db.tb_Building.Where(s => s.BuildingName.ToUpper() == BuildingName.ToUpper());
            //Check dublicates
            if (buildings.ToList().Count == 0)
            {
                tb_Building building = new tb_Building()
                {
                    CampusID = Campus,
                    BuildingName = BuildingName,
                    ImagePath = imagesFolder + Path.GetFileName(file.FileName)
                };
                db.tb_Building.Add(building);
            }
            else
            {
                tb_Building building = buildings.FirstOrDefault();
                building.CampusID = Campus;
                building.BuildingName = BuildingName;
                building.ImagePath = imagesFolder + Path.GetFileName(file.FileName);
                db.tb_Building.Attach(building);
            }
            db.SaveChanges();
            ViewBag.Colleges = new SelectList(db.tb_College, "CollegeID", "CollegeName");
            ViewBag.CollegeID = 0;
            return View(db.tb_College.ToList());
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

            var uName = HttpContext.GetOwinContext().Authentication.User.Identity.GetUserName();

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
                                fm.LastName + ", " + fm.FirstName + " " + fm.MiddleName;
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
                            error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Member with name " + fm.LastName + ", " + fm.FirstName + " " + fm.MiddleName + " already exist in the tb_MemberMaster table";
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
                            AddedBy = uName,
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
                            model._ZipCode, model._AddressTypeID, true, "Form", 1, FM.MemberID, uName);
                        if (errs.Count > 0)
                        {
                            ViewData["ErrorList"] = errs;
                            transaction.Rollback();
                            return View(model);
                        }

                        errs = CreateMemberModel.AssignPhoneNumber(model._PhoneNumber, model._PhoneTypeID, true, "Form", FM.MemberID, uName);
                        if (errs.Count > 0)
                        {
                            ViewData["ErrorList"] = errs;
                            transaction.Rollback();
                            return View(model);
                        }

                        errs = CreateMemberModel.AssignEmail(model._EmailAddress, model._EmailTypeID, true, "Form", FM.MemberID, uName);
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
                try
                {
                    tb_MemberEmail email = context.tb_MemberEmail.Find(EmailID);
                    if (email == null)
                    {
                        return HttpNotFound();
                    }
                    context.tb_MemberEmail.Remove(email);
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

        // GET: AddFilter
        [Authorize(Roles = "admin, organizer")]
        public ActionResult AddFilter(int CollegeID, int DepartmentID)
        {
            ViewBag.CollegeID = CollegeID;
            if (DepartmentID == 0)
                DepartmentID = 3;
            var departments = new SelectList(db.tb_Department, "DepartmentID", "DepartmentName", DepartmentID);
            ViewBag.Departments = departments;

            var colleges = new SelectList(db.tb_College, "CollegeID", "CollegeName", CollegeID);
            ViewBag.Colleges = colleges;

            return PartialView("AddFilter");
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