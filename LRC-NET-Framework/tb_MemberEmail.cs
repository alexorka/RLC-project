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
    
    public partial class tb_MemberEmail
    {
        public int EmailID { get; set; }
        public string EmailAddress { get; set; }
        public bool IsPrimary { get; set; }
        public int EmailTypeID { get; set; }
        public string Source { get; set; }
        public System.DateTime CreatedDateTime { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDateTime { get; set; }
        public int MemberID { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
    
        public virtual AspNetUsers AspNetUsers { get; set; }
        public virtual tb_EmailType tb_EmailType { get; set; }
        public virtual tb_MemberMaster tb_MemberMaster { get; set; }
    }
}
