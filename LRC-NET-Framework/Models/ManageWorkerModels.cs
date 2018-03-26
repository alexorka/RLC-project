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
}