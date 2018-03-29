using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LRC_NET_Framework.Models
{
    public class AssessActivityModels
    {
        public tb_Assessment _Assessment { get; set; }
        public tb_Activity _Activity { get; set; }
        public IEnumerable<tb_ActivityStatus> _ActivityStatus { get; set; }
        public IEnumerable<tb_MemberActivity> _MemberActivity { get; set; }
    }

    //public class AddOrEditAssessmentModels
    //{
    //    public int _MemberID { get; set; }
    //    public string _BackgroundStory { get; set; }
    //    public string _Fears { get; set; }
    //    public string _AttitudeTowardUnion { get; set; }
    //    public string _IDdLeaders { get; set; }
    //    public string _FollowUp { get; set; }
    //    //public tb_Assessment _Assessment { get; set; }
    //    public int _AssessmentNameID { get; set; }
    //    public IEnumerable<tb_AssessmentName> _AssessmentName { get; set; }
    //    public int _AssessmentValueID { get; set; }
    //    public IEnumerable<tb_AssessmentValue> _AssessmentValue { get; set; }
    //}
}