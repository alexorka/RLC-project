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
    
    public partial class tb_Campus
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tb_Campus()
        {
            this.tb_Building = new HashSet<tb_Building>();
            this.tb_CampusMapping = new HashSet<tb_CampusMapping>();
            this.tb_MemberMaster = new HashSet<tb_MemberMaster>();
            this.tb_Activity = new HashSet<tb_Activity>();
        }
    
        public int CampusID { get; set; }
        public int CollegeID { get; set; }
        public string CollegeCode { get; set; }
        public string CampusName { get; set; }
        public bool IsMain { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tb_Building> tb_Building { get; set; }
        public virtual tb_College tb_College { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tb_CampusMapping> tb_CampusMapping { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tb_MemberMaster> tb_MemberMaster { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tb_Activity> tb_Activity { get; set; }
    }
}
