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
    
    public partial class tb_Assessment
    {
        public int AssessmentID { get; set; }
        public int MemberID { get; set; }
        public string AssessmentDesc { get; set; }
        public byte Value { get; set; }
        public System.DateTime AssessmentDate { get; set; }
        public int AssesedBy { get; set; }
        public int AddedBy { get; set; }
        public System.DateTime AddedDateTime { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDateTime { get; set; }
    
        public virtual tb_MemberMaster tb_MemberMaster { get; set; }
    }
}
