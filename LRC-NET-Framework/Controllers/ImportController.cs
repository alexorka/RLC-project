using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Data.Entity;
using System.Web;
using System.Web.Mvc;
using System.Net;
using LRC_NET_Framework.Models;
using System.IO;
using ExcelImport.Models;
using Microsoft.AspNet.Identity;

namespace LRC_NET_Framework.Controllers
{
    public class ImportController : Controller
    {
        private LRCEntities db = new LRCEntities();
        //
        // GET: /Account/AdminImportCBU
        [Authorize(Roles = "admin")]
        public ActionResult AdminImportCBU()
        {
            var tb_Semesters = db.tb_Semesters.ToList();
            List<SelectListItem> semesters = new List<SelectListItem>();
            //semesters.Add(new SelectListItem() { Text = "-- Select One --", Value = "0" });
            foreach (var semester in tb_Semesters)
            {
                bool result = Int32.TryParse(semester.SemesterYear, out int semesterYear);
                semesterYear++; //Current and previous years semesters only
                if (semesterYear >= DateTime.UtcNow.Year)
                    semesters.Add(new SelectListItem()
                    {
                        Text = semester.SemesterName + " " + semester.SemesterYear + ": " +
                        Convert.ToDateTime(semester.SemesterStartDate).ToString("MM/dd/yyyy") + " - " + Convert.ToDateTime(semester.SemesterEndDate).ToString("MM/dd/yyyy"),
                        Value = semester.SemesterID.ToString()
                    });
            }

            ViewBag.Semesters = semesters;
            ViewBag.CollegeID = 0;

            List<string> errs = new List<string>();
            if (TempData["ErrorList"] == null)
            {
                errs.Add("Empty");
            }
            else
                errs = TempData["ErrorList"] as List<string>;

            ViewData["ErrorList"] = errs;
            return View();
        }

        [HttpPost]
        public ActionResult UploadExcel(HttpPostedFileBase FileUpload, int? ImportType, FormCollection formCollection)
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
            //test = GetCollegeCode("01ARCMAIN");
            //test = GetCollegeCode("01SRPSTC");
            //test = GetCollegeCode("02CRCMAIN");
            //test = GetCollegeCode("04FLCMAIN");
            //test = GetCollegeCode("04EDC");
            //test = GetCollegeCode("05SCCMAIN");
            //test = GetCollegeCode("03ETHAN");
            //test = GetCollegeCode("03DO");
            #endregion

            Error error = new Error();
            List<string> errs = new List<string>();
            if (FileUpload == null)
            {
                error.errCode = ErrorDetail.Failed;
                error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Please choose file to upload";
                errs.Add(error.errMsg);
                TempData["ErrorList"] = errs;
                return RedirectToAction("AdminImportCBU");
            }

            string filename = FileUpload.FileName;
            var extension = filename.Split('.').Last().ToUpper();
            if (extension != "XSL" && extension != "XLSX")
            {
                error.errCode = ErrorDetail.Failed;
                error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Only Excel file format is allowed (.xls or .xlsx)";
                errs.Add(error.errMsg);
                TempData["ErrorList"] = errs;
                return RedirectToAction("AdminImportCBU");
            }

            if (ImportType == null)
            {
                error.errCode = ErrorDetail.Failed;
                error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Import type (Excel sheet) is not selected";
                errs.Add(error.errMsg);
                TempData["ErrorList"] = errs;
                return RedirectToAction("AdminImportCBU");
            }

            bool result = Int32.TryParse(formCollection["Semesters"], out int semesterRecID);
            if (!result)
            {
                error.errCode = ErrorDetail.Failed;
                error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Please select semester";
                errs.Add(error.errMsg);
                TempData["ErrorList"] = errs;
                return RedirectToAction("AdminImportCBU");
            }


            string filepath = "~/ImportExcel/1/";
            if (!Directory.Exists(Server.MapPath(filepath)))
                Directory.CreateDirectory(Server.MapPath(filepath));
            string targetpath = Server.MapPath(filepath);
            FileUpload.SaveAs(targetpath + filename);
            string pathToExcelFile = targetpath + filename;
            string sheetName = String.Empty;
            ExcelMembers excelMembers = new ExcelMembers();
            ExcelSchedules excelSchedules = new ExcelSchedules();
            //var uName = HttpContext.GetOwinContext().Authentication.User.Identity.GetUserName();
            var userId = HttpContext.GetOwinContext().Authentication.User.Identity.GetUserId();

            switch (ImportType)
            {
                case 1: errs = excelMembers.MembersImport(pathToExcelFile, "Full time", semesterRecID, userId); break;
                case 2: errs = excelMembers.MembersImport(pathToExcelFile, "Adjunct", semesterRecID, userId); break;
                case 3: errs = excelSchedules.ScheduleImport(pathToExcelFile, "REG-Schedule", semesterRecID, userId); break;
                case 4: errs = excelSchedules.ScheduleImport(pathToExcelFile, "ADJ-Schedule", semesterRecID, userId); break;
                default: return RedirectToAction("AdminImportCBU"/*, new { fileTypeSelectResult = "Select appropriate file type" }*/);
            }
            if (errs == null || errs.Count == 0)
                return RedirectToAction("Index", "Home");
            else
            {
                TempData["ErrorList"] = errs;
                return RedirectToAction("AdminImportCBU");
            }
        }

        // GET: Member Import Errors
        [Authorize(Roles = "admin, organizer")]
        public ActionResult MemberImportErrors()
        {
            var tb_MemberError = db.tb_MemberError.ToList();
            return View(tb_MemberError);
        }

        // GET: Schedule Import Errors
        [Authorize(Roles = "admin, organizer")]
        public ActionResult ScheduleImportErrors()
        {
            var tb_Schedule_Error = db.tb_Schedule_Error.ToList();
            return View(tb_Schedule_Error);
        }

        // GET: Delete Member Import Errors
        [Authorize(Roles = "admin, organizer")]
        public ActionResult DeleteMemberRecord(int? errId)
        {
            if (errId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (LRCEntities context = new LRCEntities())
            {
                var me = context.tb_MemberError.Find(errId);
                context.tb_MemberError.Remove(me);
                try
                {
                    context.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {

                }
            }
            return RedirectToAction("MemberImportErrors");
        }

        // GET: Remove All Member Import Errors
        [Authorize(Roles = "admin, organizer")]
        public ActionResult RemoveAllMemberErrorRecords()
        {
            using (LRCEntities context = new LRCEntities())
            {
                try
                {
                    context.Database.ExecuteSqlCommand("TRUNCATE TABLE [dbo].[tb_MemberError]");
                }
                catch (DbEntityValidationException ex)
                {

                }
            }
            return RedirectToAction("MemberImportErrors");
        }

        // GET: Remove All Member Import Errors
        [Authorize(Roles = "admin, organizer")]
        public ActionResult RemoveAllScheduleErrorRecords()
        {
            using (LRCEntities context = new LRCEntities())
            {
                try
                {
                    context.Database.ExecuteSqlCommand("TRUNCATE TABLE [dbo].[tb_Schedule_Error]");
                }
                catch (DbEntityValidationException ex)
                {

                }
            }
            return RedirectToAction("MemberImportErrors");
        }

        // GET: Delete Schedule Import Errors
        [Authorize(Roles = "admin, organizer")]
        public ActionResult DeleteScheduleRecord(int? errId)
        {
            if (errId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (LRCEntities context = new LRCEntities())
            {
                var me = context.tb_Schedule_Error.Find(errId);
                context.tb_Schedule_Error.Remove(me);
                try
                {
                    context.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {

                }
            }
            return RedirectToAction("ScheduleImportErrors");
        }

        // GET: Colleges Mapping (Member CBU Import)
        [Authorize(Roles = "admin, organizer")]
        public ActionResult CollegesMapping()
        {
            ViewBag.Campuses = db.tb_CampusMapping.Include(c => c.tb_Campus).Where(m => m.MemberMappingCode != null).ToList();
            var campusNames = new SelectList(db.tb_Campus.Where(m => m.IsMain == true), "CampusID", "CampusName");
            ViewBag.CampusId = campusNames.OrderBy(t => t.Text);
            return View();
        }

        // POST: Colleges Mapping (Member CBU Import)
        [Authorize(Roles = "admin, organizer")]
        [HttpPost]
        public ActionResult CollegesMapping([Bind(Include = "ID,MemberMappingCode")] tb_CampusMapping tb_CampusMapping, int? CampusId)
        {
            if (CampusId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (LRCEntities context = new LRCEntities())
            {
                tb_CampusMapping campusMapping = new tb_CampusMapping
                {
                    CampusID = CampusId ?? 0,
                    MemberMappingCode = tb_CampusMapping.MemberMappingCode,
                };
                context.tb_CampusMapping.Add(campusMapping);
                try
                {
                    context.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {

                }
            }
            ViewBag.Campuses = db.tb_CampusMapping.Include(c => c.tb_Campus).Where(m => m.MemberMappingCode != null).ToList();
            var campusNames = new SelectList(db.tb_Campus.Where(m => m.IsMain == true), "CampusID", "CampusName");
            ViewBag.CampusId = campusNames.OrderBy(t => t.Text);
            return View();
        }

        // GET: Delete College Mapping Record (Member CBU Import)
        [Authorize(Roles = "admin, organizer")]
        public ActionResult RemoveMemberCollegeMapping(int? errId)
        {
            if (errId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (LRCEntities context = new LRCEntities())
            {
                var rec = context.tb_CampusMapping.Find(errId);
                context.tb_CampusMapping.Remove(rec);
                try
                {
                    context.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {

                }
            }
            return RedirectToAction("CollegesMapping");
        }

        // GET: Campuses Mapping (Schedule CBU Import)
        [Authorize(Roles = "admin, organizer")]
        public ActionResult CampusesMapping()
        {
            ViewBag.Campuses = db.tb_CampusMapping.Include(c => c.tb_Campus).Where(m => m.ScheduleMappingName != null).ToList();
            var campusNames = new SelectList(db.tb_Campus, "CampusID", "CampusName");
            ViewBag.CampusId = campusNames.OrderBy(t => t.Text);
            return View();
        }

        // POST: Campuses Mapping (Schedule CBU Import)
        [Authorize(Roles = "admin, organizer")]
        [HttpPost]
        public ActionResult CampusesMapping([Bind(Include = "ID,ScheduleMappingName")] tb_CampusMapping tb_CampusMapping, int? CampusId)
        {
            if (CampusId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (LRCEntities context = new LRCEntities())
            {
                tb_CampusMapping campusMapping = new tb_CampusMapping
                {
                    CampusID = CampusId ?? 0,
                    ScheduleMappingName = tb_CampusMapping.ScheduleMappingName
                };
                context.tb_CampusMapping.Add(campusMapping);
                try
                {
                    context.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {

                }
            }
            ViewBag.Campuses = db.tb_CampusMapping.Include(c => c.tb_Campus).Where(m => m.ScheduleMappingName != null).ToList();
            var campusNames = new SelectList(db.tb_Campus, "CampusID", "CampusName");
            ViewBag.CampusId = campusNames.OrderBy(t => t.Text);
            return View();
        }

        // GET: Delete Campus Mapping Record (Schedule CBU Import)
        [Authorize(Roles = "admin, organizer")]
        public ActionResult RemoveScheduleCampusMapping(int? Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (LRCEntities context = new LRCEntities())
            {
                var rec = context.tb_CampusMapping.Find(Id);
                context.tb_CampusMapping.Remove(rec);
                try
                {
                    context.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {

                }
            }
            return RedirectToAction("CampusesMapping");
        }

        // GET: Member column names mapping (Member CBU Import)
        [Authorize(Roles = "admin, organizer")]
        public ActionResult MembersMapping()
        {
            ViewBag.Columns = db.tb_MembersImportMapping.ToList();
            var modelFields = new SelectList(db.tb_MembersImportMapping, "ID", "ModelCorrespondingField");
            ViewBag.ID = modelFields;
            return View();
        }

        // POST: Member column names mapping (Member CBU Import)
        [Authorize(Roles = "admin, organizer")]
        [HttpPost]
        public ActionResult MembersMapping([Bind(Include = "ID,ColumnNameCBU")] tb_MembersImportMapping tb_MembersImportMapping, int? ID)
        {
            if (ID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (LRCEntities context = new LRCEntities())
            {
                try
                {
                    var rec = db.tb_MembersImportMapping.Find(ID);
                    rec.ColumnNameCBU = tb_MembersImportMapping.ColumnNameCBU;
                    context.tb_MembersImportMapping.Attach(rec);
                    context.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {

                }
            }
            ViewBag.Columns = db.tb_MembersImportMapping.ToList();
            var modelFields = new SelectList(db.tb_MembersImportMapping, "ID", "ModelCorrespondingField");
            ViewBag.ID = modelFields;
            return View();
        }

        // GET: Schedule column names mapping (Schedule CBU Import)
        [Authorize(Roles = "admin, organizer")]
        public ActionResult ScheduleMapping()
        {
            ViewBag.Columns = db.tb_ScheduleImportMapping.ToList();
            var modelFields = new SelectList(db.tb_ScheduleImportMapping, "ID", "ModelCorrespondingField");
            ViewBag.ID = modelFields;
            return View();
        }

        // POST: Schedule column names mapping (Schedule CBU Import)
        [Authorize(Roles = "admin, organizer")]
        [HttpPost]
        public ActionResult ScheduleMapping([Bind(Include = "ID,ColumnNameCBU")] tb_ScheduleImportMapping tb_ScheduleImportMapping, int? ID)
        {
            if (ID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (LRCEntities context = new LRCEntities())
            {
                try
                {
                    var rec = db.tb_ScheduleImportMapping.Find(ID);
                    rec.ColumnNameCBU = tb_ScheduleImportMapping.ColumnNameCBU;
                    context.tb_ScheduleImportMapping.Attach(rec);
                    context.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {

                }
            }
            ViewBag.Columns = db.tb_ScheduleImportMapping.ToList();
            var modelFields = new SelectList(db.tb_ScheduleImportMapping, "ID", "ModelCorrespondingField");
            ViewBag.ID = modelFields;
            return View();
        }
    }
}