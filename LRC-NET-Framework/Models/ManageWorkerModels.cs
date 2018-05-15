using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace LRC_NET_Framework.Models
{
    public class ManageWorkerModels
    {
        public IEnumerable<tb_AssessmentName> _AssessmentName { get; set; }
        public tb_MemberMaster _Worker { get; set; }
    }

    public class EditWorkerModels
    {
        public int _MemberID { get; set; }
        public string _WorkerFullName { get; set; }
        //public tb_MemberMaster _Worker { get; set; }
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
        //[DisplayFormat(DataFormatString = "{0:MM'/'dd'/'yyyy}", ApplyFormatInEditMode = true)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime _HireDate { get; set; }
        public string _TwitterHandle { get; set; }
        public string _FaceBookID { get; set; }
   }

    public class AddRole
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

    public class SemesterTaughtModels
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

    public partial class ManageContactInfoModels
    {
        public int _MemberID { get; set; }

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
        public int _CityID { get; set; }
        public IEnumerable<SelectListItem> _CityStates { get; set; }
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

    public partial class AddNote
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

    public partial class AddMembershipForm
    {
        public int _MemberID { get; set; }

        [Required(ErrorMessage = "Add Date is required")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime _Signed { get; set; }
        public string _FormVersion { get; set; }
        public string _FormImagePath { get; set; }
        public int _CollectedBy { get; set; }
        public IEnumerable<tb_MembershipForms> _MembershipForms { get; set; }
    }

    public partial class AddCopeForm
    {
        public int _MemberID { get; set; }

        [Required(ErrorMessage = "Add Date is required")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime _Signed { get; set; }
        public decimal _MonthlyContribution { get; set; }
        public string _FormImagePath { get; set; }
        public int _CollectedBy { get; set; }
        public IEnumerable<tb_CopeForms> _CopeForms { get; set; }
    }

    public partial class AddDepartment
    {
        public string _DepartmentName { get; set; }
        public IEnumerable<tb_Department> _Departments { get; set; }
    }

    public partial class AlsoWorksAt
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

    public partial class AddBuilding
    {
        public int _CollegeID { get; set; }
        public IEnumerable<SelectListItem> _Colleges { get; set; }
        public int _CampusID { get; set; }
        public IEnumerable<SelectListItem> _Campuses { get; set; }
        public string _BuildingName { get; set; }
        public string _ImagePath { get; set; }
        public int _BuildingID { get; set; }
        public IEnumerable<tb_Building> _Buildings { get; set; }

    }
}


