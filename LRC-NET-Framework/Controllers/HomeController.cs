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

namespace LRC_NET_Framework.Controllers
{
    public class HomeController : Controller
    {
        private LRCEntities db = new LRCEntities();

        // GET: tb_MemberMaster
        public ActionResult Index(string sortOrder, string searchString, int? page, int? CollegeID, int? DepartmentID)
        {
            var tb_MemberMasters = db.tb_MemberMaster.Include(t => t.tb_Area).Include(t => t.tb_Department).Include(t => t.tb_Dues).Include(t => t.tb_LatestUnionAssessment).Include(t => t.tb_Dues);

            tb_MemberMasters.Select(t => t.tb_Department.tb_College);
            ViewData["MemberQty"] = tb_MemberMasters.Count();

            //SelectList colleges = new SelectList(db.tb_College, "CollegeID", "CollegeDesc");

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
            // устанавливаем начальный элемент, который позволит выбрать всех
            //_colleges.Insert(0, new tb_College { CollegeDesc = "All", CollegeID = 0 });
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
            int pageSize = 3;
            int pageNumber = (page ?? 1);

            return View(tb_MemberMasters.ToPagedList(pageNumber, pageSize));
        }

        // GET: ManageWorker
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
            tb_MemberAddress ma = Worker.tb_MemberAddress.Where(t => t.MemberID == id).Where(t => t.IsPrimary == true).FirstOrDefault();
            ViewBag.MemberAddress = ma.HomeStreet1 + " " + ma.HomeStreet2 + ", " + ma.tb_CityState.CityName + ", " + ma.tb_CityState.CityAlias + ", " + ma.ZipCode;

            //tb_AssessmentName assessmentName = new tb_AssessmentName();
            //assessmentName = db.tb_AssessmentName;
            List<tb_AssessmentName> AssessmentNames = new List<tb_AssessmentName>();
            AssessmentNames = db.tb_AssessmentName.ToList();

            var model = new ManageWorkerModels()
            {
                _Worker = Worker,
                _AssessmentName = AssessmentNames
            };
            return View(model);
        }

        // GET: tb_MemberMaster/Edit/5
        public ActionResult Edit(int? id)
        {
            //id = 1; // test REMOVE IT
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tb_MemberMaster tb_MemberMaster = db.tb_MemberMaster.Find(id);
            if (tb_MemberMaster == null)
            {
                return HttpNotFound();
            }
            //ViewBag.CollegeID = new SelectList(db.tb_College, "CollegeID", "CollegeDesc", tb_MemberMaster.tb_Department.CollegeID);
            //ViewBag.DepartmentID = new SelectList(db.tb_Department, "DepartmentID", "DepartmentName", tb_MemberMaster.DepartmentID);
            //ViewBag.JobStatusID = new SelectList(db.tb_JobStatus, "JobStatusID", "JobStatusName", tb_MemberMaster.tb_JobStatus.JobStatusID);
            //ViewBag.DivisionID = new SelectList(db.tb_Division, "DivisionID", "DivisionName", tb_MemberMaster.DivisionID);
            //ViewBag.CategoryID = new SelectList(db.tb_Categories, "CategoryID", "CategoryName", tb_MemberMaster.CategoryID); //Class Category: It represents what the person was over a period of time. Valid values in drop down are Member, Non-Member, Retired

            EditWorkerModels model = new EditWorkerModels()
            {
                _MemberID = tb_MemberMaster.MemberID,
                _WorkerFullName = tb_MemberMaster.LastName + ", " + tb_MemberMaster.FirstName,
                _CollegeID = tb_MemberMaster.tb_Department.CollegeID,
                _Colleges = new SelectList(db.tb_College, "CollegeID", "CollegeDesc", tb_MemberMaster.tb_Department.CollegeID),
                _JobStatusID = tb_MemberMaster.JobStatusID,
                _JobStatuses = new SelectList(db.tb_JobStatus, "JobStatusID", "JobStatusName", tb_MemberMaster.JobStatusID),
                _DivisionID = tb_MemberMaster.DivisionID,
                _Divisions = new SelectList(db.tb_Division, "DivisionID", "DivisionName", tb_MemberMaster.DivisionID),
                _DepartmentID = tb_MemberMaster.DepartmentID,
                _Departments = new SelectList(db.tb_Department, "DepartmentID", "DepartmentName", tb_MemberMaster.DepartmentID),
                _CategoryID = tb_MemberMaster.CategoryID,
                _Categories = new SelectList(db.tb_Categories, "CategoryID", "CategoryName", tb_MemberMaster.CategoryID),
                _HireDate = tb_MemberMaster.HireDate?? DateTime.Now,
                _TwitterHandle = tb_MemberMaster.tb_MemberActivity.Where(t => t.MemberID == id).LastOrDefault().TwitterHandle,
                _FaceBookID = tb_MemberMaster.tb_MemberActivity.Where(t => t.MemberID == id).LastOrDefault().FacebookID
            };
            return View(model);
        }

        // POST: tb_MemberMaster/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditWorkerModels model)
         {
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

                db.tb_MemberMaster.Attach(worker);
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
        public ActionResult ManageContactInfo(int? id)
        {
            //id = 1; // test REMOVE IT
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //var _StateCode = db.tb_States.Where(p => p.StateCodeID == db.tb_CityState.Where(r => r.CityID == db.tb_MemberAddress.Where(t => t.MemberID == id).FirstOrDefault().CityID).FirstOrDefault().StateCodeID).FirstOrDefault().StateCode;
            

            ManageContactInfoModels model = new ManageContactInfoModels()
            {
                //PHONE
                _MemberID = id ?? 0,
                //_PhoneNumber = String.Empty,
                _IsPhonePrimary = true,
                _PhoneTypeID = 1,
                _PhoneTypes = new SelectList(db.tb_PhoneType, "PhoneTypeID", "PhoneTypeName"),
                _MemberPhoneNumbers = db.tb_MemberPhoneNumbers.Where(t => t.MemberID == id).ToList(),
                // ADDRESS
                _StateCode = db.tb_States.Where(p => p.StateCodeID == db.tb_CityState.Where(r => r.CityID == db.tb_MemberAddress.Where(t => t.MemberID == id).FirstOrDefault().CityID).FirstOrDefault().StateCodeID).FirstOrDefault().StateCode,
                _CreatedAdressBy = 2,
                _CreatedAdressDateTime = DateTime.Now,
                _IsAdressPrimary = true,
                _SourceID = 1,
                _AddressSources = new SelectList(db.tb_AddressSource, "SourceID", "SourceName"),
                _CityID = 1,
                _CityStates = new SelectList(db.tb_CityState.ToList(), "CityID", "CityName"),
                _MemberAddresses = db.tb_MemberAddress.Where(t => t.MemberID == id).ToList(),
                //EMAIL
                _EmailTypeID = 1,
                _IsEmailPrimary = true,
                _EmailTypes = new SelectList(db.tb_EmailType.ToList(), "EmailTypeID", "EmailTypeName"),
                _MemberEmails = db.tb_MemberEmail.Where(t => t.MemberID == id).ToList()
            };
            return View(model);
        }

        // POST: Home/AlsoWorksAt
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ManageContactInfo(string submit, ManageContactInfoModels model)
        {
            if (ModelState.IsValid)
            {
                switch (submit)
                {
                case "Submit New Phone":
                    var memberPhones = db.tb_MemberPhoneNumbers.Where(s => s.PhoneNumber.ToUpper() == model._PhoneNumber.ToUpper());
                    //Check dublicates
                    if (memberPhones.ToList().Count == 0)
                    {
                        tb_MemberPhoneNumbers phoneNumber = new tb_MemberPhoneNumbers()
                        {
                            MemberID = model._MemberID,
                            PhoneNumber = model._PhoneNumber,
                            IsPrimary = model._IsPhonePrimary,
                            PhoneTypeID = model._PhoneTypeID,
                            CreatedDateTime = DateTime.Now
                        };
                        db.tb_MemberPhoneNumbers.Add(phoneNumber);
                    }
                    else
                    {
                        tb_MemberPhoneNumbers phoneNumber = memberPhones.FirstOrDefault();
                        phoneNumber.PhoneNumber = model._PhoneNumber;
                        phoneNumber.IsPrimary = model._IsPhonePrimary;
                        phoneNumber.PhoneTypeID = model._PhoneTypeID;
                        phoneNumber.CreatedDateTime = DateTime.Now;
                        phoneNumber.CreatedBy = 2;
                        db.tb_MemberPhoneNumbers.Attach(phoneNumber);
                    }
                    db.SaveChanges();
                    break;
                case "Submit New Address":
                    var memberAddresses = db.tb_MemberAddress.Where(s => s.HomeStreet1.ToUpper() == model._HomeStreet1.ToUpper()
                    && s.HomeStreet2.ToUpper() == model._HomeStreet2.ToUpper()
                    && s.ZipCode.ToUpper() == model._ZipCode.ToUpper()
                    && s.CityID == model._CityID);
                    //Check dublicates
                    if (memberAddresses.ToList().Count == 0)
                    {
                        tb_MemberAddress memberAddress = new tb_MemberAddress()
                        {
                            MemberID = model._MemberID,
                            HomeStreet1 = model._HomeStreet1,
                            HomeStreet2 = model._HomeStreet2,
                            CityID = model._CityID,
                            ZipCode = model._ZipCode,
                            Country = "USA",
                            CreatedDateTime = model._CreatedAdressDateTime,
                            IsPrimary = model._IsAdressPrimary,
                            SourceID = model._SourceID
                        };
                        db.tb_MemberAddress.Add(memberAddress);
                    }
                    else
                    {
                        tb_MemberAddress memberAddress = memberAddresses.FirstOrDefault();
                        memberAddress.MemberID = model._MemberID;
                        memberAddress.HomeStreet1 = model._HomeStreet1;
                        memberAddress.HomeStreet2 = model._HomeStreet2;
                        memberAddress.CityID = model._CityID;
                        memberAddress.ZipCode = model._ZipCode;
                        memberAddress.Country = "USA";
                        memberAddress.CreatedDateTime = model._CreatedAdressDateTime;
                        memberAddress.IsPrimary = model._IsAdressPrimary;
                        memberAddress.SourceID = model._SourceID;
                        db.tb_MemberAddress.Attach(memberAddress);
                    }
                    db.SaveChanges();
                    break;
                 case "Submit New Email":
                    var memberEmails = db.tb_MemberEmail.Where(s => s.EmailAddress.ToUpper() == model._EmailAddress.ToUpper());
                    //Check dublicates
                    if (memberEmails.ToList().Count == 0)
                    {
                        tb_MemberEmail memberEmail = new tb_MemberEmail()
                        {
                            MemberID = model._MemberID,
                            EmailAddress = model._EmailAddress,
                            EmailTypeID = model._EmailTypeID,
                            IsPrimary = model._IsEmailPrimary,
                            CreatedDateTime = DateTime.Now,
                            CreatedBy = 2
                        };
                        db.tb_MemberEmail.Add(memberEmail);
                    }
                    else
                    {
                        tb_MemberEmail memberEmail = memberEmails.FirstOrDefault();
                        memberEmail.EmailAddress = model._EmailAddress;
                        memberEmail.EmailTypeID = model._EmailTypeID;
                        memberEmail.IsPrimary = model._IsEmailPrimary;
                        memberEmail.CreatedDateTime = DateTime.Now;
                        memberEmail.CreatedBy = 2;
                        db.tb_MemberEmail.Attach(memberEmail);
                    }
                    db.SaveChanges();
                    break;
                    //default:
                    //    break;
                }
            }

            model._PhoneTypes = new SelectList(db.tb_PhoneType, "PhoneTypeID", "PhoneTypeName");
            model._MemberPhoneNumbers = db.tb_MemberPhoneNumbers.Where(t => t.MemberID == model._MemberID).ToList();
            model._StateCode = db.tb_States.Where(p => p.StateCodeID == db.tb_CityState.Where(r => r.CityID == db.tb_MemberAddress.Where(t => t.MemberID == model._MemberID).FirstOrDefault().CityID).FirstOrDefault().StateCodeID).FirstOrDefault().StateCode;
            model._AddressSources = new SelectList(db.tb_AddressSource, "SourceID", "SourceName");
            model._CityStates = new SelectList(db.tb_CityState.ToList(), "CityID", "CityName");
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
            AddMembershipForm model = new AddMembershipForm()
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
        public ActionResult AddMembershipForm(AddMembershipForm model, HttpPostedFileBase file)
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
            AddCopeForm model = new AddCopeForm()
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
        public ActionResult AddCopeForm(AddCopeForm model, HttpPostedFileBase file)
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
        public ActionResult AlsoWorksAt(int? id)
        {
            //id = 1; // test REMOVE IT
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AlsoWorksAt model = new AlsoWorksAt()
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
        public ActionResult AlsoWorksAt(AlsoWorksAt model)
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

        // GET: Home/AddMembershipForm
        public ActionResult AddBuilding()
        {
            var colleges = new SelectList(db.tb_College, "CollegeID", "CollegeDesc");
            ViewBag.Colleges = colleges;
            ViewBag.CollegeID = 0;
            return View(db.tb_College.ToList());
        }

        [HttpPost]
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

        // POST: Home/AddMembershipForm
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
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
                ViewBag.Colleges = new SelectList(db.tb_College, "CollegeID", "CollegeDesc", College);
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
            ViewBag.Colleges = new SelectList(db.tb_College, "CollegeID", "CollegeDesc");
            ViewBag.CollegeID = 0;
            return View(db.tb_College.ToList());
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}