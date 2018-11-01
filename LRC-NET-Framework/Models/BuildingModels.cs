using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Validation;

namespace LRC_NET_Framework.Models
{
    public class AddBuildingModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "required")]
        public int _CollegeID { get; set; }
        public IEnumerable<SelectListItem> _Colleges { get; set; }
        [Required(ErrorMessage = "required")]
        public int _CampusID { get; set; }
        public IEnumerable<SelectListItem> _Campuses { get; set; }
        [Required(ErrorMessage = "required")]
        public string _BuildingName { get; set; }
        public IEnumerable<tb_College> _tb_College { get; set; }
    }
}