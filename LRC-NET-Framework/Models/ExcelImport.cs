using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Collections;
using System.Data.Entity.Validation;
using LRC_NET_Framework.Models;
using LinqToExcel;
using LinqToExcel.Query;
using LRC_NET_Framework;
using System.Globalization;
using Microsoft.Owin;

using Microsoft.AspNet.Identity;

namespace ExcelImport.Models
{
    public class ExcelMembers
    {
        public string Location { get; set; }
        public string FullName { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Phone { get; set; }
        public string Status { get; set; }
        public string EmployeeID { get; set; }

        private LRCEntities db = new LRCEntities();

        #region Facility Member Import

        public List<string> MembersImport(string pathToExcelFile, string sheetName, int semesterRecID, string uName)
        {
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            List<string> errs = new List<string>();
            List<string> warnings = new List<string>();
            string warning = String.Empty;

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
            errs = CheckMemberFields(members);
            if (errs.Count > 0)
                return errs;

            int record = 0;
            foreach (var excelRec in members)
            {
                record++;

                error = SplitFullName(excelRec.FullName, "MF", record, out string lastName, out string firstName, out string middleName);
                if (error.errCode != ErrorDetail.Success)
                {
                    errs.Add(error.errMsg);
                    return errs;
                }

                //Check is Facility Member exist in DB. Returned FM = NULL means new member
                errs = IsMemberExistInDB(lastName, firstName, middleName, out tb_MemberMaster FM);
                if (errs.Count > 0)
                    return errs;
                bool isNewMember = false;
                if (FM == null) // new member
                {
                    isNewMember = true;
                    FM = new tb_MemberMaster();
                    //CBU.Name
                    FM.LastName = lastName;
                    FM.FirstName = firstName;
                    FM.MiddleName = middleName;
                    FM.LastSeenDate = DateTime.MinValue;
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

                errs = UpdateMember(excelRec, FM, semesterRecID, record, isNewMember, out warning, uName);
                if (errs.Count > 0)
                    return errs;
                if (!String.IsNullOrEmpty(warning))
                    warnings.Add(warning);
            }
            //deleting excel file from folder  
            if ((System.IO.File.Exists(pathToExcelFile)))
            {
                System.IO.File.Delete(pathToExcelFile);
            }

            if (warnings.Count > 0)
            {
                foreach(var item in warnings)
                {
                    errs.Add(item);
                }
            }

            return errs;
        }

        private List<string> UpdateMember(ExcelMembers excelRec, tb_MemberMaster FM, int semesterRecID, int record, bool isNewMember, out string warning, string uName)
        {
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            List<string> errs = new List<string>();
            warning = String.Empty;

            // Check semester Date From
            try
            {
                var semesterStartDate = db.tb_Semesters.Find(semesterRecID).DateFrom;
                if (FM.LastSeenDate > semesterStartDate)
                {
                    warning = "Warning!Row #" + record.ToString() + " Facility member data has not been updated. Last Seen Date content is > Semester Start Date of the semester file being loaded.";
                    return errs;
                }
            }
            catch (Exception)
            {
                error.errCode = ErrorDetail.UnknownError;
                error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!db.tb_Semesters.Find(semesterRecID)";
                errs.Add(error.errMsg);
                return errs;
            }

            //FM.Location
            errs = GetCampusCode(excelRec.Location, out string campusCode);
            if (errs.Count > 0)
                return errs;
            errs = GetCampusID(campusCode, out int campusId);
            if (errs.Count > 0)
                return errs;
            FM.CampusID = campusId;

            //FM.Descr (1)
            errs = GetAreaID(GetAreaName(excelRec.Description), out int areaID);
            if (errs.Count > 0)
                return errs;
            FM.AreaID = areaID;

            //FM.Descr (2)
            errs = GetDepartmentID(GetDepartmentName(excelRec.Description), campusId, out int departmentID);
            if (errs.Count > 0)
                return errs;
            FM.DepartmentID = departmentID;
            //FM.EmployeeID
            FM.MemberIDNumber = excelRec.EmployeeID;
            //DivisionID (Required field. Need to be filled)
            FM.DivisionID = 108; //108 = 'Unknown' from tb_Division table
                                 //CategoryID (Required field. Need to be filled)
            FM.CategoryID = 4; //4 = 'Unknown' from tb_Categories table
            FM.LastSeenDate = DateTime.UtcNow;

            if (FM.MemberID == 0) // New Facility Member
            {
                FM.AddedBy = uName;
                FM.AddedDateTime = DateTime.UtcNow;
                db.tb_MemberMaster.Add(FM);
            }
            else
            {
                FM.ModifiedBy = uName;
                FM.ModifiedDateTime = DateTime.UtcNow;
            }

            try
            {
                db.SaveChanges();
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
                return errs;
            }

            errs = AssignAddress(excelRec.Address, excelRec.City, excelRec.State, excelRec.Zip, FM.MemberID, uName);
            if (errs.Count > 0)
                return errs;
            errs = AssignPhoneNumber(excelRec.Phone, FM.MemberID, uName);
            if (errs.Count > 0)
                return errs;

            return errs;
        }

        // Check excel spreadsheet fields are correct
        public List<string> CheckMemberFields(List<ExcelMembers> members)
        {
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            List<string> errs = new List<string>();
            int record = 0;
            foreach (var excelRec in members)
            {
                record++;

                if (String.IsNullOrEmpty(excelRec.Location))
                {
                    error.errCode = ErrorDetail.DataImportError;
                    error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Row #" + record + ". Column 'Location': Is empty";
                    errs.Add(error.errMsg);
                }

                if (String.IsNullOrEmpty(excelRec.FullName))
                {
                    error.errCode = ErrorDetail.DataImportError;
                    error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Row #" + record + ". Column 'Name': Is empty";
                    errs.Add(error.errMsg);
                }
                else
                {
                    error = SplitFullName(excelRec.FullName, "MF", record, out string lastName, out string firstName, out string middleName);
                    if (error.errCode != ErrorDetail.Success)
                        errs.Add(error.errMsg);
                }

                if (String.IsNullOrEmpty(excelRec.Description))
                {
                    error.errCode = ErrorDetail.DataImportError;
                    error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Row #" + record + ". Column 'Descr': Is empty";
                    errs.Add(error.errMsg);
                }

                if (String.IsNullOrEmpty(excelRec.Address))
                {
                    error.errCode = ErrorDetail.DataImportError;
                    error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Row #" + record + ". Column 'Address': Is empty";
                    errs.Add(error.errMsg);
                }

                if (String.IsNullOrEmpty(excelRec.City))
                {
                    error.errCode = ErrorDetail.DataImportError;
                    error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Row #" + record + ". Column 'Location': Is empty";
                    errs.Add(error.errMsg);
                }
                if (String.IsNullOrEmpty(excelRec.State))
                {
                    error.errCode = ErrorDetail.DataImportError;
                    error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Row #" + record + ". Column 'St': Is empty";
                    errs.Add(error.errMsg);
                }
                if (String.IsNullOrEmpty(excelRec.Zip))
                {
                    error.errCode = ErrorDetail.DataImportError;
                    error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Row #" + record + ". Column 'Postal': Is empty";
                    errs.Add(error.errMsg);
                }
                if (String.IsNullOrEmpty(excelRec.Phone))
                {
                    error.errCode = ErrorDetail.DataImportError;
                    error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Row #" + record + ". Column 'Phone': Is empty";
                    errs.Add(error.errMsg);
                }
                if (String.IsNullOrEmpty(excelRec.EmployeeID))
                {
                    error.errCode = ErrorDetail.DataImportError;
                    error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Row #" + record + ". Column 'EmployeeID': Is empty";
                    errs.Add(error.errMsg);
                }
            }
            return errs;
        }

        // Extract Last, First, Middle Names from FullName 
        public Error SplitFullName(string _fullName, string importType, int record, out string _lastName, out string _firstName, out string _middleName)
        {
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            error.errMsg = ErrorDetail.GetMsg(error.errCode);
            //List<string> errs = new List<string>();
            _lastName = String.Empty;
            _firstName = String.Empty;
            _middleName = String.Empty;

            var namesComma = _fullName.Split(',');
            if (namesComma.Length == 0)
            {
                error.errCode = ErrorDetail.DataImportError;
                error.errMsg = ErrorDetail.GetMsg(error.errCode);
                if (importType == "MF")
                {
                    error.errMsg += "!Row #" + record.ToString() + ". Column 'Name': Is empty";
                    //errs.Add(error.errMsg);
                }
                else if (importType == "ST")
                {
                    error.errMsg += "!Row #" + record.ToString() + ". Column 'INSTRCTR': Is empty";
                    //errs.Add(error.errMsg);
                }
            }
            else if (namesComma.Length == 1)
            {
                error.errCode = ErrorDetail.DataImportError;
                error.errMsg = ErrorDetail.GetMsg(error.errCode);
                if (importType == "MF")
                {
                    error.errMsg += "!Row #" + record.ToString() + ". Column 'Name': Comma is absent";
                    //errs.Add(error.errMsg);
                }
                else if (importType == "ST")
                {
                    error.errMsg += "!Row #" + record.ToString() + ". Column 'INSTRCTR': Comma is absent";
                    //errs.Add(error.errMsg);
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
            return error;
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
                error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!GetCampusCode(...) function failed. Wrong Campus Code";
                errs.Add(error.errMsg);
            }
            return errs;
        }

        //Check if current Campus is present in tb_Campus and add it if not
        public List<string> GetCampusID(string CampusCode, out int campusID)
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
                    return errs;
                }

            }
            else
                //return CampusID of founded Campus
                campusID = campuses.FirstOrDefault().CampusID;

            return errs;
        }

        //Conversion CBU 'descr' to AreaName
        private string GetAreaName(string descr)
        {
            //add new Area
            string strTmp1, strTmp2;
            string areaName = String.Empty;
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
                    { areaName = "Counselor"; return areaName; }

                    //Case 2 – If char 5 to 8 != PROF, AND upper 5 to 8 does = 'LIBR' set field 'AreaName' to 'Librarian' 
                    strTmp2 = descr.Substring(4, 4).ToUpper(); //chars 5 to 8
                    if (strTmp2 == "LIBR")
                    { areaName = "Librarian"; return areaName; }

                    //Case 3 – If char 5 to 8 != PROF, but upper 5 to 10 does = 'Nurses' set field 'AreaName' to 'Nurses' 
                    strTmp2 = descr.Substring(4, 6).ToUpper(); //chars 5 to 10
                    if (strTmp2 == "NURSES")
                    { areaName = "Nurses"; return areaName; }

                    //Case 4 – If char 5 to 8 != PROF, but upper 5 to 9 does = "COORD" set field to Coord to end of string
                    strTmp2 = descr.Substring(4, 5).ToUpper(); //chars 5 to 9
                    if (strTmp2 == "COORD")
                    {
                        StringComparison comp = StringComparison.OrdinalIgnoreCase;
                        int indx = descr.IndexOf("COORD", comp);
                        areaName = descr.Substring(indx).Trim();
                        return areaName;
                    }
                }
            }

            //Case 5 – Set to 'Miscellaneous' WHERE Field 'AreaName' is still NULL and 'misc' exists anywhere in the string
            strTmp2 = descr.ToUpper();
            if (String.IsNullOrEmpty(areaName) && strTmp2.Contains("MISC"))
            { areaName = "Miscellaneous"; return areaName; }

            //Case 6 – Set to 'CJTC' WHERE Field 'AreaName' is still NULL and 'CJTC' exists anywhere in the string
            strTmp2 = descr.ToUpper();
            if (String.IsNullOrEmpty(areaName) && strTmp2.Contains("CJTC"))
            { areaName = "CJTC"; return areaName; }

            //Case 7a – Has at least one dash AND there are two dashes (a dash found searching from the start and a dash found searching from the end are not in the same position).
            if (strTmp2.Contains("-"))
            {
                if (strTmp2.LastIndexOf("-") != strTmp2.IndexOf("-"))
                { areaName = descr.Substring(descr.LastIndexOf("-") + 1).Trim(); return areaName; }
                //Case 7b – Only one dash (a dash found searching from the start and a dash found searching from the end ARE in the same position).
                else // strTmp2.LastIndexOf("-") == strTmp2.IndexOf("-")
                { areaName = descr.Substring(descr.IndexOf("-") + 1).Trim(); return areaName; }

            }

            //Case 8 - Get entire string when 'AreaName' value is still NULL
            if (String.IsNullOrEmpty(areaName))
                areaName = descr.Trim();

            return areaName;
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
                    return errs;
                }
            }
            else
                //return AreaID of founded Area
                areaID = areas.FirstOrDefault().AreaID;
            return errs;
        }

        //Conversion FM 'descr' to DepartmentName
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
        public List<string> IsMemberExistInDB(string lastname, string firstname, string middlename, out tb_MemberMaster fm)
        {
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            error.errMsg = ErrorDetail.GetMsg(error.errCode);
            List<string> errs = new List<string>();
            fm = new tb_MemberMaster();
            //1. Find memberID by Full Name
            try
            {
                var fms = db.tb_MemberMaster.Where(s => s.LastName.ToUpper() == lastname.ToUpper() &&
                s.FirstName.ToUpper() == firstname.ToUpper() &&
                s.MiddleName.ToUpper() == middlename.ToUpper());
                fm = fms.FirstOrDefault();
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

        //Set Member Address IsPrimary to false from true
        public Error SetAdressesPrimaryFalse(int mID)
        {
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            error.errMsg = ErrorDetail.GetMsg(error.errCode);
            using (LRCEntities context = new LRCEntities())
            {
                var addresses = context.tb_MemberAddress.Where(t => t.MemberID == mID && t.IsPrimary == true);
                if (addresses.Count() > 0)
                {
                    foreach (var address in addresses)
                        address.IsPrimary = false;
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
                        error.errMsg = "Error #" + error.errCode.ToString() + "!" + error.errMsg;
                        return error;
                    }
                }
            }
            return error;
        }

        //Set Member Phone Numbers IsPrimary to false from true
        public Error SetPhonePrimaryFalse(int mID)
        {
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            error.errMsg = ErrorDetail.GetMsg(error.errCode);
            using (LRCEntities context = new LRCEntities())
            {
                var phones = context.tb_MemberPhoneNumbers.Where(t => t.MemberID == mID && t.IsPrimary == true);
                if (phones.Count() > 0)
                {
                    foreach (var phone in phones)
                        phone.IsPrimary = false;
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
                        error.errMsg = "Error #" + error.errCode.ToString() + "!" + error.errMsg;
                        return error;
                    }
                }
            }
            return error;
        }

        //Get tb_MemberAddress record for current Member
        //Assign MemberID for existing Member or return tb_MemberAddress.MemberID = 0 for new one
        private List<string> AssignAddress(string address, string city, string st, string postal, int mID, string uName)
        {
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            error.errMsg = ErrorDetail.GetMsg(error.errCode);
            List<string> errs = new List<string>();
            int cityId = GetCityID(city);
            tb_MemberAddress ma = new tb_MemberAddress();
            // Set IsPrimary = false for all other addresses of current member
            error = SetAdressesPrimaryFalse(mID);
            if (error.errCode != ErrorDetail.Success)
            {
                errs.Add(error.errMsg);
                return errs;
            }
            var memberAddresses = db.tb_MemberAddress.Where(s => s.MemberID == mID
                && s.HomeStreet1.ToUpper() == address.ToUpper()
                && s.CityID == cityId
                && s.ZipCode.ToUpper() == postal.ToUpper());
            //Checking if address from the list of current member addresses already exist
            if (memberAddresses.Count() > 0)
            {
                //Just return founded same as in CBU old address with current memberID. Update founded address
                ma = memberAddresses.FirstOrDefault();
                ma.Country = "USA";
                ma.SourceID = 2; //Employer
                ma.Source = "CBU";
                ma.AddressTypeID = 1; //Mailing (from tb_AddressType table)
                ma.IsPrimary = true;
                ma.ModifiedBy = uName;
                ma.ModifiedDateTime = DateTime.UtcNow;
                ma.StartDate = DateTime.UtcNow;
                ma.EndDate = null;
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
                ma.SourceID = 2; //Employer
                ma.Source = "CBU";
                ma.IsPrimary = true;
                ma.AddressTypeID = 1; //Mailing (from tb_AddressType table)
                ma.CreatedDateTime = DateTime.UtcNow;
                ma.StartDate = DateTime.UtcNow;
                ma.EndDate = null;
                db.tb_MemberAddress.Add(ma);
            }

            try
            {
                db.SaveChanges();
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
                return errs;
            }
            return errs;
        }

        //Assign tb_MemberPhoneNumbers record for current Member
        private List<string> AssignPhoneNumber(string phone, int mID, string uName)
        {
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            error.errMsg = ErrorDetail.GetMsg(error.errCode);
            List<string> errs = new List<string>();
            //Find all member phones by memberID
            tb_MemberPhoneNumbers mp = new tb_MemberPhoneNumbers();
            // Set IsPrimary = false for all other addresses of current member
            error = SetPhonePrimaryFalse(mID);
            if (error.errCode != ErrorDetail.Success)
            {
                errs.Add(error.errMsg);
                return errs;
            }
            //Check if phone from the list of current member phone already exist
            var memberPhoneNumbers = db.tb_MemberPhoneNumbers.Where(s => s.MemberID == mID && s.PhoneNumber.ToUpper() == phone.ToUpper());
            if (memberPhoneNumbers.Count() > 0)
            {
                //Obtained record with a same as in CBU phone number. Just set IsPrimary = true
                mp = memberPhoneNumbers.FirstOrDefault();
                mp.Source = "CBU";
                mp.IsPrimary = true;
                mp.PhoneTypeID = 1; //Mobile in the tb_PhoneType table
                mp.ModifiedBy = uName;
                mp.ModifiedDateTime = DateTime.UtcNow;
                var entry = db.Entry(mp);
            }
            //Current member hasn't phone as in CBU
            else
            {
                //Create the record with new phone from CBU
                mp.MemberID = mID;
                mp.PhoneNumber = phone;
                mp.Source = "CBU";
                mp.IsPrimary = true;
                mp.PhoneTypeID = 1; //Mobile in the tb_PhoneType table
                mp.CreatedDateTime = DateTime.UtcNow;
                db.tb_MemberPhoneNumbers.Add(mp);
            }

            try
            {
                db.SaveChanges();
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
                return errs;
            }
            return errs;
        }
        #endregion

    }

    public class ExcelSchedules
    {
        public string Id { get; set; }
        public string Instructor { get; set; }
        public string Campus { get; set; }
        public string Location { get; set; }
        public string Building { get; set; }
        public string Room { get; set; }
        public string Division { get; set; }
        public string ClassNumber { get; set; }
        public string CAT_NBR { get; set; }
        public string Sect { get; set; }
        public string Subject { get; set; }
        public string LecOrLab { get; set; }
        public string SB_TM { get; set; }
        public string ATT_TP { get; set; }
        public string BeginTime { get; set; }
        public string EndTime { get; set; }
        public string Days { get; set; }
        public string ClassEndDate { get; set; }

        private LRCEntities db = new LRCEntities();

        #region Faculty Schedule Import
        ///<Author>Alex</Author>
        /// <summary>
        /// Fill Out Schedule from Excel file
        /// </summary>       
        /// <returns>Status code</returns>
        public List<string> ScheduleImport(string pathToExcelFile, string sheetName, int semesterRecID, string uName)
        {
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            List<string> errs = new List<string>();
            ExcelMembers excelMembers = new ExcelMembers();
            List<string> warnings = new List<string>();
            string warning = String.Empty;

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
            foreach (var excelRec in schedules)
            {
                record++;
                tb_SemesterTaught ST = new tb_SemesterTaught();
                error = excelMembers.SplitFullName(excelRec.Instructor, "ST", record, out string lastName, out string firstName, out string middleName);
                if (error.errCode != ErrorDetail.Success)
                {
                    errs.Add(error.errMsg);
                    return errs;
                }

                //Check is Facility Member exist in DB. Returned FM = NULL means doesnt exist
                errs = excelMembers.IsMemberExistInDB(lastName, firstName, middleName, out tb_MemberMaster FM);
                if (errs.Count > 0)
                    return errs;
                if (FM == null) // Member wasn't found
                {
                    error.errCode = ErrorDetail.Failed;
                    error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!Row #" + record.ToString() + ". Instructor name: " + excelRec.Instructor + " wasn't found in the DataBase";
                    errs.Add(error.errMsg);
                    return errs;
                }

                ST.MemberID = FM.MemberID;

                errs = UpdateSchedule(excelRec, ST, semesterRecID, FM.LastSeenDate, record, out warning, uName);
                if (errs.Count > 0)
                    return errs;
                if (!String.IsNullOrEmpty(warning))
                    warnings.Add(warning);
            }
            //deleting excel file from folder  
            if ((System.IO.File.Exists(pathToExcelFile)))
            {
                System.IO.File.Delete(pathToExcelFile);
            }

            return errs;
        }

        private List<string> UpdateSchedule(ExcelSchedules excelRec, tb_SemesterTaught ST, int semesterRecID, DateTime lastSeenDate, int record, out string warning, string uName)
        {
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            List<string> errs = new List<string>();
            warning = String.Empty;

            //// Check semester Date From
            //try
            //{
            //    var semesterStartDate = db.tb_Semesters.Find(semesterRecID).DateFrom;
            //    if (lastSeenDate > semesterStartDate)
            //    {
            //        warning = "Warning!Row #" + record.ToString() + " Schedule data for current Facility Member has not been updated. Last Seen Date content is > Semester Start Date of the semester file being loaded.";
            //        return errs;
            //    }
            //}
            //catch (Exception)
            //{
            //    error.errCode = ErrorDetail.UnknownError;
            //    error.errMsg = ErrorDetail.GetMsg(error.errCode) + "!db.tb_Semesters.Find(semesterRecID)";
            //    errs.Add(error.errMsg);
            //    return errs;
            //}

            errs = GetSemesterRecID(excelRec.ClassEndDate.Trim(), record, semesterRecID, out bool scheduleStatus);
            if (errs.Count > 0)
                return errs;
            ST.SemesterRecID = semesterRecID;
            ST.ScheduleStatus = scheduleStatus;

            errs = GetClassWeekDayID(excelRec.Days, record, out int classWeekDayID);
            if (errs.Count > 0)
                return errs;
            ST.ClassWeekDayID = classWeekDayID;

            ST.Class = excelRec.ClassNumber.Trim();

            excelRec.BeginTime = excelRec.BeginTime.Trim().Insert(5, " ");
            ST.ClassStart = DateTime.ParseExact(excelRec.BeginTime.Trim(), "hh:mm tt", CultureInfo.InvariantCulture).TimeOfDay;

            excelRec.EndTime = excelRec.EndTime.Trim().Insert(5, " ");
            ST.ClassEnd = DateTime.ParseExact(excelRec.EndTime.Trim(), "hh:mm tt", CultureInfo.InvariantCulture).TimeOfDay;

            errs = GetBuildingID(excelRec.Building, excelRec.Campus, out int buildingID);
            if (errs.Count > 0)
                return errs;
            ST.BuildingID = buildingID;

            if (String.IsNullOrEmpty(excelRec.Room) && excelRec.Building.ToUpper() == "ONLINE")
                ST.Room = "ONLINE";
            else
                ST.Room = excelRec.Room.Trim();

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
                    return errs;
                }
            }

            return errs;
        }

        // Check excel spreadsheet fields are correct
        private List<string> CheckScheduleFields(List<ExcelSchedules> schedules)
        {
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
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

        private List<string> GetSemesterRecID(string classEndDate, int rec, int semesterRecID, out bool scheduleStatus)
        {
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
            
            try
            {
                classWeekDayID = db.tb_WeekDay.Where(t => t.CbuWeekDay == days).FirstOrDefault().ClassWeekDayID;
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
            ExcelMembers excelMembers = new ExcelMembers();
            List<string> errs = new List<string>();
            tb_Building tb_building = new tb_Building();
            var buildings = db.tb_Building.Where(t => t.BuildingName.ToUpper() == building.ToUpper());
            if (buildings.Count() == 0)
            {
                tb_building.BuildingName = building;

                errs = excelMembers.GetCampusID(campus, out int campusId);
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
                    return errs;
                }
            }
            else
                //return BuildingID of founded Building
                buildingID = buildings.FirstOrDefault().BuildingID;
            return errs;
        }

        #endregion
    }
}