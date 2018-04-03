using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace LRC_NET_Framework.Models
{
    public class Colleges
    {
        public int _CollegeID { get; set; }
        public string _CollegeDesc { get; set; }

        public ICollection<tb_Campus> _Campuses { get; set; }
    }

    public class Campuses
    {
        public int _CampusID { get; set; }
        public string _CampusName { get; set; }

        public int _CollegeID { get; set; }
        public tb_College _Colleges { get; set; }
    }
}