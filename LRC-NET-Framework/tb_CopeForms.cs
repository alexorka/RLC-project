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
    
    public partial class tb_CopeForms
    {
        public int CopeFormID { get; set; }
        public int MemberID { get; set; }
        public System.DateTime Signed { get; set; }
        public decimal MonthlyContribution { get; set; }
        public string CollectedBy { get; set; }
        public string FormImagePath { get; set; }
        public bool Processed { get; set; }
        public bool SentToEmployer { get; set; }
        public string AddedBy { get; set; }
    
        public virtual AspNetUsers AspNetUsers { get; set; }
        public virtual AspNetUsers AspNetUsers1 { get; set; }
        public virtual tb_MemberMaster tb_MemberMaster { get; set; }
    }
}
