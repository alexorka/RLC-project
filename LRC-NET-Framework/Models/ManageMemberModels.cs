using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Validation;
using System.Linq;

namespace LRC_NET_Framework.Models
{
    public class CreateMemberModel
    {
        // EMPLOYEE ID  (mandatory)
        [Required(ErrorMessage = "required")]
        public string _MemberIDNumber { get; set; } //Distinct Member Identifier

        // MEMBER NAME (mandatory)
        [Required(ErrorMessage = "required")]
        public string _FirstName { get; set; } //Employee first name
        [Required(ErrorMessage = "required")]
        public string _LastName { get; set; } //Employee middle name. Typically just initia
        public string _MiddleName { get; set; } //Employee last name

        // ADDRESS  (mandatory)
        [Required(ErrorMessage = "required")]
        public string _HomeStreet1 { get; set; } //Street number and name of employee mailing address.  May include apartment or unit number.
        public string _HomeStreet2 { get; set; }
        [Required(ErrorMessage = "required")]
        public string _City { get; set; } //City of employee mailing address
        [Required(ErrorMessage = "required")]
        public int _StateID { get; set; }//State of employee mailing address
        public IEnumerable<SelectListItem> _States { get; set; }
        [Required(ErrorMessage = "required")]
        public string _ZipCode { get; set; } //Zip code of mailing address. Typically zip 5, sometimes Zip 5 + 4
        [Required(ErrorMessage = "required")]
        public int _AddressTypeID { get; set; } //Mailing, Residence or Work
        public IEnumerable<SelectListItem> _AddressTypes { get; set; }

        // PHONE  (mandatory)
        [Phone]
        [Display(Name = "Phone Number")]
        [Required(ErrorMessage = "required")]
        public string _PhoneNumber { get; set; }
        [Required(ErrorMessage = "required")]
        public int _PhoneTypeID { get; set; } //Phone Type: Mobile, Home, Work, Unknown
        public IEnumerable<SelectListItem> _PhoneTypes { get; set; }

        // EMAIL  (optional)
        [EmailAddress]
        [Display(Name = "Email Address")]
        [Required(ErrorMessage = "required")]
        public string _EmailAddress { get; set; }
        public int _EmailTypeID { get; set; }
        public IEnumerable<SelectListItem> _EmailTypes { get; set; }

        // MEMBER STATUS (optional)
        public Nullable<int> _CategoryID { get; set; } //Member or Non-member 
        public IEnumerable<SelectListItem> _Categories { get; set; }

        // MEMBER CATEGORY (mandatory)
        [Required(ErrorMessage = "required")]
        public int _JobStatusID { get; set; } //Indicates whether the employee is a union member or not (Valid values include: Adjunct, Full-Time)
        public IEnumerable<SelectListItem> _JobStatuses { get; set; }

        // MEMBER DEPARTMENT  (optional)
        public Nullable<int> _DepartmentID { get; set; } //The department that the employee works for
        public IEnumerable<SelectListItem> _Departments { get; set; }

        // MEMBER ROLE (optional)
        public string _Area { get; set; } //The subject matter of the course (e.g., NURSE, ENGRD). Assumptions: Aka Area from the district spreadsheet.User freeform entry. 

        //// MEMBER COLLEGE (optional)
        public Nullable<int> _CampusID { get; set; } //Valid values include: ARC, FLC, SCC, CRC, DO
        public IEnumerable<SelectListItem> _Campuses { get; set; }

        //// MEMBER COLLEGE (optional)
        public Nullable<int> _DivisionID { get; set; } //Valid values include: ARC, FLC, SCC, CRC, DO
        public IEnumerable<SelectListItem> _Divisions { get; set; }

        //Check if current AreaName is present in tb_Area already and add it if not
        public static List<string> GetAreaID(string AreaName, out int areaID)
        {
            areaID = 0;
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            error.errMsg = ErrorDetail.GetMsg(error.errCode);
            List<string> errs = new List<string>();
            tb_Area tb_area = new tb_Area();
            using (LRCEntities context = new LRCEntities())
            {
                var areas = context.tb_Area.Where(t => t.AreaName.ToUpper() == AreaName.ToUpper());
                if (areas.Count() == 0)
                {
                    tb_area.AreaName = AreaName;
                    tb_area.AreaDesc = String.Empty; //??? may be add it later with some Edit Area Form
                    context.tb_Area.Add(tb_area);
                    try
                    {
                        context.SaveChanges();
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
            }
            return errs;
        }

        //Get tb_MemberAddress record for current Member
        //Assign MemberID for existing Member or return tb_MemberAddress.MemberID = 0 for new one
        public static List<string> AssignAddress(string _HomeStreet1, string _HomeStreet2, string city, string st, string postal, int addressTypeID, 
            bool isAdressPrimary, string source, int sourceID, int mID, string uName)
        {
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            error.errMsg = ErrorDetail.GetMsg(error.errCode);
            List<string> errs = new List<string>();
            //int cityId = GetCityID(city);

            using (LRCEntities context = new LRCEntities())
            {
                // Check if address(es) exist for current member
                var memberAddresses = context.tb_MemberAddress.Where(s => s.MemberID == mID).OrderByDescending(s => s.CreatedDateTime).ToArray();

                if (memberAddresses.Count() > 0) //Current member has address(es)
                {
                    int recornNumber = 0;
                    foreach (var ma in memberAddresses)
                    {
                        if (++recornNumber <= MvcApplication.MaxRecordsInAddressHistory) //Leaving 5 records only and updating them
                        {
                            if (isAdressPrimary)
                            {
                                ma.IsPrimary = false; //Set IsPrimary to false for all other member addresses from history
                                if (ma.EndDate == null) //EndDate == null means current Member Address is actual (isn't record for history)
                                    ma.EndDate = DateTime.UtcNow;
                            }
                        }
                        else //Remove the excess. In the history we leave only MaxRecordsInAddressHistory = 5 entries
                        {
                            context.tb_MemberAddress.Remove(ma);
                        }
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
                            return errs;
                        }
                    }
                }

                //Assign new address to the current Member
                //Check dublicates
                memberAddresses = memberAddresses.Where(s => s.MemberID == mID && s.HomeStreet1 == _HomeStreet1
                    && s.City == city && s.ZipCode == postal).ToArray();
                if (memberAddresses.Count() == 0) // add new address
                {
                    tb_MemberAddress address = new tb_MemberAddress()
                    {
                        MemberID = mID,
                        HomeStreet1 = _HomeStreet1,
                        HomeStreet2 = _HomeStreet2,
                        City = city,
                        ZipCode = postal,
                        StateID = Int32.Parse(st),
                        Country = "USA",
                        SourceID = sourceID, // 1 -Card/Form, 2 - Employer
                        Source = source,
                        IsPrimary = isAdressPrimary,
                        AddressTypeID = addressTypeID,
                        CreatedDateTime = DateTime.Now,
                        ModifiedBy = uName,
                        StartDate = DateTime.UtcNow.AddDays(-1),
                        EndDate = null
                    };
                    context.tb_MemberAddress.Add(address);
                }
                else // edit old address
                {
                    var address = context.tb_MemberAddress.Where(s => s.MemberID == mID && s.HomeStreet1 == _HomeStreet1
                    && s.City == city && s.ZipCode == postal).FirstOrDefault();
                    address.HomeStreet2 = _HomeStreet2;
                    address.StateID = Int32.Parse(st);
                    address.Country = "USA";
                    address.SourceID = sourceID;
                    address.Source = source;
                    address.IsPrimary = isAdressPrimary;
                    address.AddressTypeID = addressTypeID;
                    address.ModifiedBy = uName;
                    address.ModifiedDateTime = DateTime.Now;
                    address.StartDate = DateTime.UtcNow.AddDays(-1);
                    address.EndDate = null;
                }

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
                    return errs;
                }
            }
            return errs;
        }

        //Assign tb_MemberPhoneNumbers record for current Member
        public static List<string> AssignPhoneNumber(string phone, int phoneTypeID, bool isPhonePrimary, string source, int mID, string uName)
        {
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            error.errMsg = ErrorDetail.GetMsg(error.errCode);
            List<string> errs = new List<string>();

            using (LRCEntities context = new LRCEntities())
            {
                // Check if phone(s) exist for current member
                var memberPhoneNumbers = context.tb_MemberPhoneNumbers.Where(s => s.MemberID == mID).OrderByDescending(s => s.CreatedDateTime).ToArray();

                if (memberPhoneNumbers.Count() > 0) //Current member has phone(s)
                {
                    int recornNumber = 0;
                    foreach (var mp in memberPhoneNumbers)
                    {
                        if (++recornNumber <= MvcApplication.MaxRecordsInPhoneHistory) //Leaving 10 records only and updating them
                        {
                            if (isPhonePrimary)
                            {
                                mp.IsPrimary = false; //Set IsPrimary to false for all other member phones from history
                                if (mp.EndDate == null) //EndDate == null means current Member phone is actual (isn't record for history)
                                    mp.EndDate = DateTime.UtcNow;
                            }
                        }
                        else //Remove the excess. In the history we leave only MaxRecordsInPhoneHistory = 10 entries
                        {
                            context.tb_MemberPhoneNumbers.Remove(mp);
                        }
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
                            return errs;
                        }
                    }
                }

                //Assign new phone to a current Member
                //Check dublicates
                memberPhoneNumbers = memberPhoneNumbers.Where(s => s.MemberID == mID && s.PhoneNumber == phone).ToArray();
                if (memberPhoneNumbers.Count() == 0) // add new phone
                {
                    tb_MemberPhoneNumbers phoneNumber = new tb_MemberPhoneNumbers()
                    {
                        MemberID = mID,
                        PhoneNumber = phone,
                        IsPrimary = isPhonePrimary,
                        PhoneTypeID = phoneTypeID,
                        Source = source,
                        CreatedDateTime = DateTime.Now,
                        ModifiedBy = uName,
                        StartDate = DateTime.UtcNow.AddDays(-1),
                        EndDate = null
                    };
                    context.tb_MemberPhoneNumbers.Add(phoneNumber);
                }
                else // edit old phone
                {
                    var phoneNumber = context.tb_MemberPhoneNumbers.Where(s => s.MemberID == mID && s.PhoneNumber == phone).FirstOrDefault();
                    //phoneNumber.PhoneNumber = model._PhoneNumber;
                    phoneNumber.IsPrimary = isPhonePrimary;
                    phoneNumber.PhoneTypeID = phoneTypeID;
                    phoneNumber.Source = source;
                    phoneNumber.ModifiedBy = uName;
                    phoneNumber.ModifiedDateTime = DateTime.Now;
                    phoneNumber.StartDate = DateTime.UtcNow.AddDays(-1);
                    phoneNumber.EndDate = null;
                }

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
                    return errs;
                }
                return errs;
            }
        }

        //Assign tb_MemberEmail record for current Member
        public static List<string> AssignEmail(string email, int emailTypeID, bool isEmailPrimary, string source, int mID, string uName)
        {
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            error.errMsg = ErrorDetail.GetMsg(error.errCode);
            List<string> errs = new List<string>();
            using (LRCEntities context = new LRCEntities())
            {
                // Check if email(s) exist for current member
                var memberEmailAddresses = context.tb_MemberEmail.Where(s => s.MemberID == mID).OrderByDescending(s => s.CreatedDateTime).ToArray();

                if (memberEmailAddresses.Count() > 0) //Current member has email(s)
                {
                    int recornNumber = 0;
                    foreach (var me in memberEmailAddresses)
                    {
                        if (++recornNumber <= MvcApplication.MaxRecordsInEmailHistory) //Leaving 10 records only and updating them
                        {
                            if (isEmailPrimary)
                            {
                                me.IsPrimary = false; //Set IsPrimary to false for all other member email from history
                                if (me.EndDate == null) //EndDate == null means current Member email is actual (isn't record for history)
                                    me.EndDate = DateTime.UtcNow;
                            }
                        }
                        else //Remove the excess. In the history we leave only MaxRecordsInEmailHistory = 10 entries
                        {
                            context.tb_MemberEmail.Remove(me);
                        }
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
                            return errs;
                        }
                    }
                }

                //Assign new email to a current Member
                //Check dublicates
                memberEmailAddresses = memberEmailAddresses.Where(s => s.MemberID == mID && s.EmailAddress == email).ToArray();
                if (memberEmailAddresses.Count() == 0) // add new email
                {
                    tb_MemberEmail emailAddress = new tb_MemberEmail()
                    {
                        MemberID = mID,
                        EmailAddress = email,
                        IsPrimary = isEmailPrimary,
                        EmailTypeID = emailTypeID,
                        Source = source,
                        CreatedDateTime = DateTime.Now,
                        ModifiedBy = uName,
                        StartDate = DateTime.UtcNow.AddDays(-1),
                        EndDate = null
                    };
                    context.tb_MemberEmail.Add(emailAddress);
                }
                else // edit old email
                {
                    var emailAddress = context.tb_MemberEmail.Where(s => s.MemberID == mID && s.EmailAddress == email).FirstOrDefault();
                    emailAddress.IsPrimary = isEmailPrimary;
                    emailAddress.EmailTypeID = emailTypeID;
                    emailAddress.Source = source;
                    emailAddress.ModifiedBy = uName;
                    emailAddress.ModifiedDateTime = DateTime.Now;
                    emailAddress.StartDate = DateTime.UtcNow.AddDays(-1);
                    emailAddress.EndDate = null;
                }

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
                    return errs;
                }
                return errs;
            }
        }
    }

    public class MemberDetailsModel
    {
        public IEnumerable<tb_AssessmentName> _AssessmentName { get; set; }
        public tb_MemberMaster _Member { get; set; }
    }

    public class MemberEditModel
    {
        public int _MemberID { get; set; }
        public string _MemberFullName { get; set; }
        //public tb_MemberMaster _Member { get; set; }
        public int _CollegeID { get; set; }
        public IEnumerable<SelectListItem> _Colleges { get; set; }
        public int _JobStatusID { get; set; }
        public IEnumerable<SelectListItem> _JobStatuses { get; set; }
        public Nullable<int> _DivisionID { get; set; }
        public IEnumerable<SelectListItem> _Divisions { get; set; }
        public Nullable<int> _DepartmentID { get; set; }
        public IEnumerable<SelectListItem> _Departments { get; set; }
        public Nullable<int> _CategoryID { get; set; }
        public IEnumerable<SelectListItem> _Categories { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime _HireDate { get; set; }
        public string _TwitterHandle { get; set; }
        public string _FaceBookID { get; set; }
   }

    public partial class MemberContactInfoModel
    {
        public int _MemberID { get; set; }
        public string _MemberName { get; set; }

        //PHONE
        [Phone]
        [Display(Name = "Phone Number")]
        [Required(ErrorMessage = "required")]
        public string _PhoneNumber { get; set; }
        public bool _IsPhonePrimary { get; set; }
        //public int _CollegeID { get; set; } //Back to Member List by School
        public int _PhoneTypeID { get; set; }
        public IEnumerable<SelectListItem> _PhoneTypes { get; set; }
        public IEnumerable<tb_MemberPhoneNumbers> _MemberPhoneNumbers { get; set; }

        // ADDRESS
        [Required(ErrorMessage = "required")]
        public string _HomeStreet1 { get; set; }
        public string _HomeStreet2 { get; set; }
        [Required(ErrorMessage = "required")]
        public string _City { get; set; }
        public string _StateCode { get; set; }
        [Required(ErrorMessage = "required")]
        public string _ZipCode { get; set; }
        //public int _CreatedAdressBy { get; set; }
        [Required(ErrorMessage = "Add Date is required")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime _CreatedAdressDateTime { get; set; }
        public bool _IsAdressPrimary { get; set; }
        public int _AddressTypeID { get; set; }
        public IEnumerable<SelectListItem> _AddressTypes { get; set; }
        public int _SourceID { get; set; }
        public IEnumerable<SelectListItem> _AddressSources { get; set; }
        public string _StateID { get; set; }
        public IEnumerable<SelectListItem> _States { get; set; }
        public int _MemberAddressID { get; set; }
        public IEnumerable<tb_MemberAddress> _MemberAddresses { get; set; }

        //EMAIL
        [Display(Name = "Email")]
        [EmailAddress]
        [Required(ErrorMessage = "required")]
        public string _EmailAddress { get; set; }
        public bool _IsEmailPrimary { get; set; }
        public int _EmailTypeID { get; set; }
        public IEnumerable<SelectListItem> _EmailTypes { get; set; }
        public int _MemberEmailID { get; set; }
        public IEnumerable<tb_MemberEmail> _MemberEmails { get; set; }
    }

    public class CreateRoleModel
    {
        public int _MemberID { get; set; }
        public IEnumerable<SelectListItem> _Members { get; set; }
        public int _RoleID { get; set; }
        public IEnumerable<SelectListItem> _Roles { get; set; }
        public int _BodyID { get; set; }
        public IEnumerable<SelectListItem> _Bodies { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime _StartDate { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime _EndDate { get; set; }
        public IEnumerable<tb_MemberRoles> _MemberRoles { get; set; }
    }

    public class SemesterTaughtModel
    {
        public int _MemberID { get; set; }
        public int _CollegeID { get; set; }
        public IEnumerable<SelectListItem> _Colleges { get; set; }
        public int _CampusID { get; set; }
        public IEnumerable<SelectListItem> _Campuses { get; set; }
        public int _BuildingID { get; set; }
        public IEnumerable<SelectListItem> _Buildings { get; set; }
        //public int _SemesterRecID { get; set; }
        //public IEnumerable<SelectListItem> _Semesters { get; set; }
        public string _Room { get; set; }
        public string _Class { get; set; }
        //public IEnumerable<SelectListItem> _Classes { get; set; }
        [Required]
        [DataType(DataType.Time)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:H:mm}")]
        public DateTime _StartTime { get; set; }
        [Required]
        [DataType(DataType.Time)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:H:mm}")]
        public DateTime _EndTime { get; set; }
        public int _ClassWeekDayID { get; set; }
        public IEnumerable<tb_WeekDay> _WeekDays { get; set; }
        public int _ScheduleStatusID { get; set; }
        public IEnumerable<SelectListItem> _ScheduleStatuses { get; set; }

    }

    public partial class AddNoteModel
    {
        public int _MemberID { get; set; }

        public string _Note { get; set; }
        [Required(ErrorMessage = "Add Date is required")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime _NoteDate { get; set; }
        public int _NoteTypeID { get; set; }
        public IEnumerable<SelectListItem> _NoteTypes { get; set; }
        public int _TakenBy { get; set; }
        public IEnumerable<tb_MemberNotes> _MemberNotes { get; set; }
    }

    public partial class AddMembershipFormModel
    {
        public int _MemberID { get; set; }

        [Required(ErrorMessage = "Add Date is required")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime _Signed { get; set; }
        public string _FormVersion { get; set; }
        public string _FormImagePath { get; set; }
        public string _CollectedBy { get; set; }
        public IEnumerable<tb_MembershipForms> _MembershipForms { get; set; }
    }

    public partial class AddCopeFormModel
    {
        public int _MemberID { get; set; }

        [Required(ErrorMessage = "Add Date is required")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime _Signed { get; set; }
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
        public decimal _MonthlyContribution { get; set; }
        public string _FormImagePath { get; set; }
        public string _CollectedBy { get; set; }
        public IEnumerable<tb_CopeForms> _CopeForms { get; set; }
    }

    public partial class AlsoWorksAtModel
    {
        public int _MemberID { get; set; }

        public int _EmployerID { get; set; }
        public IEnumerable<SelectListItem> _Employers { get; set; }
        public string _Note { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime _AddedDateTime { get; set; }
        public int _AddedBy { get; set; }
        public IEnumerable<tb_AlsoWorksAt> _AlsoWorksAts { get; set; }
    }

    //public partial class AddDepartmentModel
    //{
    //    public string _DepartmentName { get; set; }
    //    public IEnumerable<tb_Department> _Departments { get; set; }
    //}

    //public partial class AddBuilding
    //{
    //    public int _CollegeID { get; set; }
    //    public IEnumerable<SelectListItem> _Colleges { get; set; }
    //    public int _CampusID { get; set; }
    //    public IEnumerable<SelectListItem> _Campuses { get; set; }
    //    public string _BuildingName { get; set; }
    //    public string _ImagePath { get; set; }
    //    public int _BuildingID { get; set; }
    //    public IEnumerable<tb_Building> _Buildings { get; set; }

    //}
}


