using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LRC_NET_Framework.Models
{
    public class ManageWorkerModels
    {
        public IEnumerable<tb_AssessmentName> _AssessmentName { get; set; }
        public tb_MemberMaster _Worker { get; set; }
    }
}