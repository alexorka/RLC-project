//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LRC_NET_Framework
{
    using System;
    using System.Collections.Generic;
    
    public partial class tb_MemberSemesterCourse
    {
        public int MemberSemesterCourseID { get; set; }
        public int MemberSemesterID { get; set; }
        public int CourseID { get; set; }
    
        public virtual tb_Course tb_Course { get; set; }
        public virtual tb_MemberSemester tb_MemberSemester { get; set; }
    }
}