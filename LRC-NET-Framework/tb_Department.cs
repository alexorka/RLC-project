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
    
    public partial class tb_Department
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tb_Department()
        {
            this.tb_MemberMaster = new HashSet<tb_MemberMaster>();
        }
    
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public Nullable<byte> CollegeID { get; set; }
    
        public virtual tb_College tb_College { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tb_MemberMaster> tb_MemberMaster { get; set; }
    }
}
