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
        public int _DivisionID { get; set; }
        public IEnumerable<SelectListItem> _Divisions { get; set; }
        public int _DepartmentID { get; set; }
        public IEnumerable<SelectListItem> _Departments { get; set; }
        public int _CategoryID { get; set; }
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
        //[Required(ErrorMessage = "Phone no. is required")]
        //[RegularExpression("^(?!0+$)(\\+\\d{1,3}[- ]?)?(?!0+$)\\d{10,15}$", ErrorMessage = "Please enter valid phone no.")]
        //[DataType(DataType.PhoneNumber)]
        [Phone]
        [Display(Name = "Phone Number")]
        public string _PhoneNumber { get; set; }
        public bool _IsPhonePrimary { get; set; }
        public Nullable<int> _PhoneCreatedBy { get; set; }
        public Nullable<System.DateTime> _CreatedPhoneDateTime { get; set; }
        public Nullable<int> _PhoneModifiedBy { get; set; }
        public Nullable<System.DateTime> _PhoneModifiedDateTime { get; set; }

        public int _PhoneTypeID { get; set; }
        public IEnumerable<SelectListItem> _PhoneTypes { get; set; }
        public int _PhoneRecID { get; set; }
        public IEnumerable<tb_MemberPhoneNumbers> _MemberPhoneNumbers { get; set; }

        // ADDRESS
        [Required(ErrorMessage = "Home Street is required")]
        public string _HomeStreet1 { get; set; }
        public string _HomeStreet2 { get; set; }
        public string _CityName { get; set; }
        public string _StateCode { get; set; }
        [Required(ErrorMessage = "ZIP is required")]
        public string _ZipCode { get; set; }
        public int _CreatedAdressBy { get; set; }
        [Required(ErrorMessage = "Add Date is required")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime _CreatedAdressDateTime { get; set; }
        [Required(ErrorMessage = "Status is required")]
        public bool _IsAdressPrimary { get; set; }
        //[Required(ErrorMessage = "Source is required")]
        public int _SourceID { get; set; }
        public IEnumerable<SelectListItem> _AddressSources { get; set; }
        //[Required(ErrorMessage = "City is required")]
        public int _CityID { get; set; }
        public IEnumerable<SelectListItem> _CityStates { get; set; }
        public int _MemberAddressID { get; set; }
        public IEnumerable<tb_MemberAddress> _MemberAddress { get; set; }
    }
}


