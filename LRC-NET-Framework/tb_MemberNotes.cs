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
    
    public partial class tb_MemberNotes
    {
        public int MemberNotesID { get; set; }
        public int MemberID { get; set; }
        public string Notes { get; set; }
        public int NoteTypeID { get; set; }
        public System.DateTime NoteDate { get; set; }
        public Nullable<int> TakenBy { get; set; }
        public Nullable<int> AddedBy { get; set; }
        public Nullable<System.DateTime> AddedDateTime { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDateTime { get; set; }
    
        public virtual tb_MemberMaster tb_MemberMaster { get; set; }
        public virtual tb_NoteType tb_NoteType { get; set; }
    }
}
